namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Models.Favourites.Responses;
    using Ganss.XSS;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using SixLabors.ImageSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public async Task<Result<int>> Create(CreateArticleRequestModel request)
        {
            if (request.Image.Length > 0)
            {
                if (request.Image.ContentType != "image/jpeg" &&
                    request.Image.ContentType != "image/jpg" &&
                    request.Image.ContentType != "image/png" &&
                    request.Image.ContentType != "image/bmp")
                {
                    return Result<int>.Failure(BadRequest, WrongImageFormat);
                }

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
                return Result<int>.Failure(BadRequest, EmptyFile);
            }

            if (!Enum.IsDefined(typeof(ArticleType), request.ArticleType))
            {
                return Result<int>.Failure(BadRequest, WrongArticleType);
            }

            var isTitleTaken = await this.dbContext
                .Articles
                .AnyAsync(x => x.Title == request.Title);

            if (isTitleTaken)
            {
                return Result<int>.Failure(Conflict, TitleTaken);
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
                return Result<int>.Failure(NotFound, TrainerNotFound);
            }

            if (trainer.IsReadyToWrite == false)
            {
                return Result<int>.Failure(Conflict, NotReady); //TODO: 409?
            }

            var id = Guid.NewGuid().ToString();
            var totalImages = await this.dbContext.ArticleImageDatas.CountAsync();
            var folder = $"Article/Images/{totalImages % 1000}";

            var uploadImageResult = await this.cloudinaryService.UploadImage(request.Image.OpenReadStream(), id, folder);
            if (uploadImageResult.Succeeded)
            {
                var articleId = 0;
                var strategy = this.dbContext.Database.CreateExecutionStrategy();
                var result = false;
                
                try
                {
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

                            await this.dbContext.Articles.AddAsync(article);
                            await this.dbContext.SaveChangesAsync();

                            articleId = article.Id;

                            var articleImageData = new ArticleImageData
                            {
                                Id = uploadImageResult.Data.PublicId,
                                Url = uploadImageResult.Data.Url,
                                Folder = folder,
                                ArticleId = article.Id
                            };

                            await this.dbContext.ArticleImageDatas.AddAsync(articleImageData);
                            await this.dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();
                            return true;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{nameof(ArticleService)}.{nameof(Create)} when trying to insert data in database.");
                    return Result<int>.Failure(InternalServerError, string.Format(Wrong, nameof(Create)));
                }

                if (result)
                {
                    return Result<int>.SuccessWith(articleId);
                }
            }

            Log.Error($"{nameof(ArticleService)}.{nameof(Create)} when uploading image to cloudinary service with errors:" + string.Join(Environment.NewLine, uploadImageResult.Errors));
            return Result<int>.Failure(InternalServerError, string.Format(Wrong, nameof(Create)));
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
                return Result.Failure(Forbidden);
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
                    return Result.Failure(InternalServerError, string.Format(Wrong, nameof(Edit)));
                }

                var id = Guid.NewGuid().ToString();
                var totalImages = await this.dbContext.ArticleImageDatas.CountAsync();
                var folder = $"Article/Images/{totalImages % 1000}";

                var uploadImageResult = await this.cloudinaryService.UploadImage(request.Image.OpenReadStream(), id, folder);
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
                        await this.dbContext.ArticleImageDatas.AddAsync(articleImageData);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{nameof(ArticleService)}/{nameof(Edit)} while adding image in database.");
                        return Result.Failure(InternalServerError, string.Format(Wrong, nameof(Edit)));
                    }

                    return Result.Success;
                }

                Log.Error($"{nameof(ArticleService)}.{nameof(Edit)} when uploading image to cloudinary service with errors:" + string.Join(Environment.NewLine, uploadImageResult.Errors));
                return Result.Failure(InternalServerError, string.Format(Wrong, nameof(Edit)));
            }

            Log.Error($"{nameof(ArticleService)}.{nameof(Edit)} when deleting image from cloudinary service with errors:" + string.Join(Environment.NewLine, deleteImageResult.Errors));
            return Result.Failure(InternalServerError, string.Format(Wrong, nameof(Edit)));
        }

        public async Task<Result> Delete(DeleteArticleRequestModel request)
        {
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
                return Result.Failure(Forbidden);
            }

            var articleImageData = await this.dbContext
                .ArticleImageDatas
                .FirstOrDefaultAsync(x => x.ArticleId == article.Id);

            if (articleImageData == null)
            {
                return Result.Failure(NotFound, ImageMissing);
            }

            var deleteImageResult = await this.cloudinaryService.DeleteImage(articleImageData.Id, articleImageData.Folder);
            if (!deleteImageResult.Succeeded)
            {
                Log.Error($"{nameof(ArticleService)}.{nameof(Delete)} where deleting image from cloudinary with errors:" + string.Join(Environment.NewLine, deleteImageResult.Errors));
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
                .FindAsync(new object[] { id });

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
                    ImageUrl = this.dbContext.ArticleImageDatas.First(y => y.ArticleId == x.Id).Url,
                    TrainerName = this.dbContext.Trainers.First(y => y.Id == x.Id).FullName,
                    CreatedOn = x.CreatedOn,
                    ArticleType = (int)x.ArticleType
                })
                .AsQueryable();

            var totalRecords = await articles.CountAsync();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var sortingOrder = request.OrderBy!;
            var sortingField = request.SortBy!;

            var orderBy = "Id";

            if (!string.IsNullOrWhiteSpace(sortingField))
            {
                if (sortingField.ToLower() == "title")
                {
                    orderBy = nameof(request.Title);
                }
                else if (sortingField.ToLower() == "trainername")
                {
                    orderBy = nameof(request.TrainerName);
                }
                else if (sortingField.ToLower() == "articletype")
                {
                    orderBy = nameof(request.ArticleType);
                }
            }

            if (sortingOrder != null && sortingOrder.ToLower() == Desc)
            {
                articles = articles.OrderByDescending(x => orderBy);
            }
            else
            {
                articles = articles.OrderBy(x => orderBy);
            }

            var data = await articles
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<SearchArticlesResponseModel>.SuccessWith(new SearchArticlesResponseModel { Data = data, TotalRecords = totalRecords });
        }

        public async Task<Result<List<string>>> AutocompleteArticleTitle(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var articlesTitles = await this.dbContext.Articles
                .Select(x => x.Title.ToLower())
                .Where(x => x.Contains(term.ToLower()))
                .ToListAsync();

                return Result<List<string>>.SuccessWith(articlesTitles);
            }

            return Result<List<string>>.SuccessWith(new List<string>());
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
