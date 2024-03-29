﻿namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Article.Models.Responses.Article;
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Models.Favourites.Responses;
    using Ganss.Xss;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using SixLabors.ImageSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Article;
    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.Article;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Trainer;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;
    
    public class ArticleService : IArticleService
    {
        private readonly ArticleDbContext dbContext;
        private readonly ICloudinaryService cloudinaryService;
        private readonly ICurrentUserService currentUserService;

        public ArticleService(
            ArticleDbContext dbContext,
            ICloudinaryService cloudinaryService,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.cloudinaryService = cloudinaryService;
            this.currentUserService = currentUserService;
        }

        public async Task<Result<CreateArticleResponseModel>> Create(CreateArticleRequestModel request)
        {
            if (request.Image.Length > 0)
            {
                if (request.Image.ContentType != "image/jpeg" &&
                    request.Image.ContentType != "image/jpg" &&
                    request.Image.ContentType != "image/png" &&
                    request.Image.ContentType != "image/bmp")
                {
                    return Result<CreateArticleResponseModel>.Failure(BadRequest, WrongImageFormat);
                }

                using (var imageResult = Image.Load(request.Image.OpenReadStream()))
                {
                    if (imageResult.Width < MinImageWidth || imageResult.Height < MinImageHeight)
                    {
                        return Result<CreateArticleResponseModel>.Failure(BadRequest, string.Format(WrongWidthOrHeight, MinImageWidth, MinImageHeight));
                    }
                }
            }
            else
            {
                return Result<CreateArticleResponseModel>.Failure(BadRequest, EmptyFile);
            }

            if (!Enum.IsDefined(typeof(ArticleType), request.ArticleType))
            {
                return Result<CreateArticleResponseModel>.Failure(BadRequest, WrongArticleType);
            }

            var isTitleTaken = await this.dbContext
                .Articles
                .AnyAsync(x => x.Title == request.Title);

            if (isTitleTaken)
            {
                return Result<CreateArticleResponseModel>.Failure(Conflict, TitleTaken);
            }

            var trainer = await this.dbContext
                .Trainers
                .Select(x => new
                {
                    x.Id,
                    x.ApplicationUserId,
                    x.IsReadyToWrite
                })
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

            if (trainer == null)
            {
                return Result<CreateArticleResponseModel>.Failure(NotFound, TrainerNotFound);
            }

            if (trainer.IsReadyToWrite == false)
            {
                return Result<CreateArticleResponseModel>.Failure(Forbidden, NotReady);
            }

            var id = Guid.NewGuid().ToString();
            var totalImages = await this.dbContext.ArticleImageDatas.CountAsync();
            var folder = $"Article/Images/{totalImages % 1000}";

            var uploadImageResult = await this.cloudinaryService.UploadImage(request.Image.OpenReadStream(), request.Image.ContentType, id, folder);
            if (!uploadImageResult.Succeeded)
            {
                Log.Error($"{nameof(ArticleService)}.{nameof(Create)} when uploading image to cloudinary service with errors:" + string.Join(Environment.NewLine, uploadImageResult.Errors));
                return Result<CreateArticleResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            var articleId = 0;
            var strategy = this.dbContext.Database.CreateExecutionStrategy();
            var result = false;

            result = await strategy.Execute(async () =>
            {
                using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                {
                    var article = new Article
                    {
                        Title = request.Title.Trim(),
                        Content = request.Content,
                        ArticleType = (ArticleType)request.ArticleType,
                        Sources = request.Sources,
                        CreatedOn = DateTime.Now,
                        TrainerId = trainer.Id
                    };

                    try
                    {
                        this.dbContext.Articles.Add(article);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        await this.cloudinaryService.DeleteImage(id, folder);

                        Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Create)} when adding article in database.");
                        return false;
                    }

                    articleId = article.Id;

                    var articleImageData = new ArticleImageData
                    {
                        Id = uploadImageResult.Data.PublicId,
                        Url = uploadImageResult.Data.Url,
                        Folder = folder,
                        ArticleId = article.Id
                    };

                    try
                    {
                        this.dbContext.ArticleImageDatas.Add(articleImageData);
                        await this.dbContext.SaveChangesAsync();    
                    }
                    catch (Exception ex)
                    {
                        await this.cloudinaryService.DeleteImage(id, folder);

                        Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Create)} when adding articleImageData in database.");
                        return false;
                    }

                    await transaction.CommitAsync();
                    return true;
                }
            });

            if (!result)
            {
                return Result<CreateArticleResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            return Result<CreateArticleResponseModel>.SuccessWith(new CreateArticleResponseModel() { Id = articleId });
        }

        public async Task<Result> Edit(EditArticleRequestModel request)
        {
            if (!Enum.IsDefined(typeof(ArticleType), request.ArticleType))
            {
                return Result<int>.Failure(BadRequest, WrongArticleType);
            }

            if (request.Image.Length > 0)
            {
                using (var imageResult = Image.Load(request.Image.OpenReadStream()))
                {
                    if (imageResult.Width < MinImageWidth || imageResult.Height < MinImageHeight)
                    {
                        return Result<int>.Failure(BadRequest, string.Format(WrongWidthOrHeight, MinImageWidth, MinImageHeight));
                    }
                }
            }
            else
            {
                return Result.Failure(BadRequest, NoImage);
            }

            var isTitleTaken = await this.dbContext
               .Articles
               .AnyAsync(x => x.Title == request.Title);

            if (isTitleTaken)
            {
                return Result<int>.Failure(Conflict, TitleTaken);
            }

            var article = await this.dbContext
                .Articles
                .FindAsync(new object[] { request.Id });

            if (article == null)
            {
                return Result.Failure(NotFound, ArticleMissing);
            }

            var authorId = (await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                .Id;

            if (authorId == null)
            {
                return Result.Failure(NotFound, TrainerNotFound);
            }

            if (article.TrainerId != authorId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden, WrongRights);
            }

            var imageData = await this.dbContext
                .ArticleImageDatas
                .FirstAsync(x => x.ArticleId == article.Id);

            var deleteImageResult = await this.cloudinaryService.DeleteImage(imageData.Id, imageData.Folder);
            if (deleteImageResult.Succeeded)
            {
                try
                {
                    this.dbContext.ArticleImageDatas.Remove(imageData);
                    await this.dbContext.SaveChangesAsync(); //I'm saving it here so the row where it creates the folder will be correct. Otherwise it will still count the article that we deleted in the previous line
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Edit)} while removing image from database.");
                    return Result.Failure(InternalServerError, UnhandledError);
                }

                var id = Guid.NewGuid().ToString();
                var totalImages = await this.dbContext.ArticleImageDatas.CountAsync();
                var folder = $"Article/Images/{totalImages % 1000}";

                var uploadImageResult = await this.cloudinaryService.UploadImage(request.Image.OpenReadStream(), request.Image.ContentType, id, folder);
                if (uploadImageResult.Succeeded)
                {
                    article.Title = request.Title.Trim();
                    article.Content = request.Content.Trim();
                    article.ArticleType = (ArticleType)request.ArticleType;
                    article.Sources = request.Sources;

                    var articleImageData = new ArticleImageData
                    {
                        Id = uploadImageResult.Data.PublicId,
                        Url = uploadImageResult.Data.Url,
                        Folder = folder,
                        ArticleId = article.Id
                    };

                    try
                    {
                        this.dbContext.ArticleImageDatas.Add(articleImageData);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        await this.cloudinaryService.DeleteImage(id, folder);

                        Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Edit)} while adding image in database.");
                        return Result.Failure(InternalServerError, UnhandledError);
                    }

                    return Result.Success;
                }

                Log.Error($"{nameof(ArticleService)}/{nameof(Edit)} when uploading image to cloudinary service with errors:" + string.Join(Environment.NewLine, uploadImageResult.Errors));
                return Result.Failure(InternalServerError, UnhandledError);
            }

            Log.Error($"{nameof(ArticleService)}/{nameof(Edit)} when deleting image from cloudinary service with errors:" + string.Join(Environment.NewLine, deleteImageResult.Errors));
            return Result.Failure(InternalServerError, UnhandledError);
        }

        public async Task<Result> Delete(DeleteArticleRequestModel request)
        {
            var article = await this.dbContext
                    .Articles
                    .Select(x => new Article()
                    {
                        Id = x.Id,
                        TrainerId = x.TrainerId,
                    })
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (article == null)
            {
                return Result.Failure(NotFound, ArticleMissing);
            }

            var authorId = (await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                .Id;

            if (authorId == null)
            {
                return Result.Failure(NotFound, TrainerNotFound);
            }

            if (article.TrainerId != authorId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden, WrongRights);
            }

            var articleImageData = await this.dbContext
                .ArticleImageDatas
                .Select(x => new ArticleImageData()
                {
                    Id = x.Id,
                    Folder = x.Folder
                })
                .FirstOrDefaultAsync(x => x.ArticleId == article.Id);

            if (articleImageData == null)
            {
                return Result.Failure(NotFound, ImageMissing);
            }

            var deleteImageResult = await this.cloudinaryService.DeleteImage(articleImageData.Id, articleImageData.Folder);
            if (!deleteImageResult.Succeeded)
            {
                Log.Error($"{nameof(ArticleService)}/{nameof(Delete)} where deleting image from cloudinary with errors:" + string.Join(Environment.NewLine, deleteImageResult.Errors));
            }

            try
            {
                this.dbContext.ArticleImageDatas.Remove(articleImageData);
                this.dbContext.Articles.Remove(article);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Delete)} while removing article with it's image from database.");
                return Result.Failure(InternalServerError, string.Format(Wrong, nameof(Delete)));
            }

            return Result.Success;
        }

        public async Task<Result<GetArticleResponseModel>> Get(int id)
        {
            var article = await this.dbContext
                .Articles
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Content,
                    x.CreatedOn,
                    x.ArticleType,
                    x.TrainerId
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (article == null)
            {
                return Result<GetArticleResponseModel>.Failure(NotFound, ArticleMissing);
            }

            var articleImageUrl = (await this.dbContext
                .ArticleImageDatas
                .FirstOrDefaultAsync(x => x.ArticleId == article.Id))?
                .Url;

            var trainerInformation = await this.dbContext
                .Trainers
                .Select(x => new
                {
                    x.Id,
                    x.ShortBio,
                    x.FacebookUrl,
                    x.InstagramUrl,
                    x.YoutubeChannelUrl,
                    x.ApplicationUserId
                })
                .FirstOrDefaultAsync(x => x.Id == article.TrainerId);

            var sanitizedContent = new HtmlSanitizer()
                .Sanitize(article.Content);

            return Result<GetArticleResponseModel>.SuccessWith(new GetArticleResponseModel()
            {
                Title = article.Title,
                Content = sanitizedContent,
                ImageUrl = articleImageUrl,
                CreatedOn = article.CreatedOn,
                ArticleType = (int)article.ArticleType,
                TrainerId = trainerInformation.Id,
                ShortBio = trainerInformation.ShortBio,
                TrainerFacebookUrl = trainerInformation.FacebookUrl,
                TrainerInstagramUrl = trainerInformation.InstagramUrl,
                TrainerYoutubeChannelUrl = trainerInformation.YoutubeChannelUrl,
                ApplicationUserId = trainerInformation.ApplicationUserId
            });
        }

        public async Task<Result<SearchArticlesResponseModel>> Search(SearchArticlesRequestModel request)
        {
            var articles = this.dbContext
                .Articles
                .Select(x => new ArticleResponseModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Content = x.Content,
                    ImageUrl = x.ArticleImageData.Url,
                    TrainerFirstName = x.Trainer.FirstName,
                    TrainerLastName = x.Trainer.LastName,
                    CreatedOn = x.CreatedOn,
                    ArticleType = (int)x.ArticleType
                })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.TrainerName))
            {
                var firstName = request.TrainerName.Split().First();
                var lastName = request.TrainerName.Split().Last();

                articles = articles.Where(x => x.TrainerFirstName == firstName && x.TrainerLastName == lastName);
            }

            var totalRecords = await articles.CountAsync();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var sortingOrder = request.OrderBy;
            var sortingField = request.SortBy;

            Expression<Func<ArticleResponseModel, object>> sortingExpression = x => x.CreatedOn;

            if (!string.IsNullOrWhiteSpace(sortingField))
            {
                if (sortingField.ToLower() == "date")
                {
                    sortingExpression = x => x.CreatedOn;
                }
                else if (sortingField.ToLower() == "title")
                {
                    sortingExpression = x => x.Title;
                }
            }

            if (sortingOrder != null && sortingOrder.ToLower() == Desc)
            {
                articles = articles.OrderByDescending(sortingExpression);
            }
            else
            {
                articles = articles.OrderBy(sortingExpression);
            }

            var data = await articles
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<SearchArticlesResponseModel>.SuccessWith(new SearchArticlesResponseModel { Data = data, TotalRecords = totalRecords });
        }

        public async Task<Result<Dictionary<int, string>>> AutocompleteArticleTitle(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var articlesTitles = await this.dbContext.Articles
                .Select(x => new
                {
                    Id = x.Id,
                    Title = x.Title
                })
                .Where(x => x.Title.ToLower().Contains(term.ToLower()))
                .ToDictionaryAsync(x => x.Id, x => x.Title);

                return Result<Dictionary<int, string>>.SuccessWith(articlesTitles);
            }

            return Result<Dictionary<int, string>>.SuccessWith(new Dictionary<int, string>());
        }

        public async Task<Result<bool>> ArticleExists(int id)
        {
            var exists = await this.dbContext
                     .Articles
                     .AnyAsync(x => x.Id == id);

            return Result<bool>.SuccessWith(exists);
        }

        public async Task<Result<SearchFavouritesResponseModel>> Favourites(SearchFavouritesRequestModel request)
        {
            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;

            var articles = await this.dbContext
                .Articles
                .Where(x => request.ArticlesIds.Contains(x.Id))
                .Select(x => new
                {
                    ArticleId = x.Id,
                    Title = x.Title,
                    ImageUrl = this.dbContext.ArticleImageDatas.FirstOrDefault(y => y.ArticleId == x.Id).Url
                })
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //We do this so they will be sorted in the way the user added them in his favourites. There may be another way to do this with LINQ (and maybe with better performance) but right now I'm not looking for it.
            var data = new List<FavouriteResponseModel>();
            foreach (var id in request.ArticlesIds)
            {
                var article = articles.FirstOrDefault(x => x.ArticleId == id);
                data.Add(new FavouriteResponseModel()
                {
                    ArticleId = article.ArticleId,
                    Title = article.Title,
                    ImageUrl = article.ImageUrl
                });
            }

            var totalRecords = await this.dbContext
                .Articles
                .CountAsync();

            return Result<SearchFavouritesResponseModel>.SuccessWith(new SearchFavouritesResponseModel() { Data = data, TotalRecords = totalRecords });
        }
    }
}
