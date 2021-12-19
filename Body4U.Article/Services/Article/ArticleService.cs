namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Article.Models.Requests;
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Common;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using SixLabors.ImageSharp;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using static Body4U.Common.Constants.DataConstants.Article;

    using static Body4U.Common.Constants.MessageConstants.Article;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Trainer;

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
            try
            {
                var trainer = await this.dbContext
                    .Trainers
                    .Select(x => new
                    {
                        x.Id,
                        x.ApplicationUserId,
                        x.IsReadyToWrite
                    })
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

                //if (trainer == null)
                //{
                //    return Result<int>.Failure(TrainerNotFound);
                //}

                if (trainer.IsReadyToWrite == false)
                {
                    return Result<int>.Failure(NotReady);
                }

                var isTitleTaken = await this.dbContext
                    .Articles
                    .AnyAsync(x => x.Title == request.Title);

                if (isTitleTaken)
                {
                    return Result<int>.Failure(TitleTaken);
                }

                if (!Enum.IsDefined(typeof(ArticleType), request.ArticleType))
                {
                    return Result<int>.Failure(WrongArticleType);
                }

                if (request.Image.Length > 0)
                {
                    if (request.Image.ContentType != "image/jpeg" &&
                        request.Image.ContentType != "image/jpg" &&
                        request.Image.ContentType != "image/png" &&
                        request.Image.ContentType != "image/bmp")
                    {
                        return Result<int>.Failure(WrongImageFormat);
                    }

                    using (var imageResult = Image.Load(request.Image.OpenReadStream()))
                    {
                        if (imageResult.Width < MinImageWidth || imageResult.Height < MinImageHeight)
                        {
                            return Result<int>.Failure(string.Format(WrongWidthOrHeight, MinImageWidth, MinImageHeight));
                        }

                        var id = Guid.NewGuid().ToString();
                        var totalImages = await this.dbContext.ArticleImageDatas.CountAsync();
                        var folder = $"Article/Images/{totalImages % 1000}";

                        var uploadImageResult = await this.cloudinaryService.UploadImage(request.Image.OpenReadStream(), id, folder);
                        if (uploadImageResult.Succeeded)
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

                            var articleImageData = new ArticleImageData
                            {
                                Id = uploadImageResult.Data.PublicId,
                                Url = uploadImageResult.Data.Url,
                                Folder = folder,
                                ArticleId = article.Id
                            };

                            await this.dbContext.ArticleImageDatas.AddAsync(articleImageData);
                            await this.dbContext.SaveChangesAsync();

                            return Result<int>.SuccessWith(article.Id);
                        }

                        return Result<int>.Failure(uploadImageResult.Errors);
                    }
                }

                return Result<int>.Failure(EmptyFile);

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ArticleService)}.{nameof(Create)}");
                return Result<int>.Failure(string.Format(Wrong, nameof(Create)));
            }
        }

        public async Task<Result> Edit(EditArticleRequestModel request)
        {
            try
            {
                var article = await this.dbContext
                   .Articles
                   .FindAsync(new object[] { request.Id });

                if (article == null)
                {
                    return Result.Failure(ArticleMissing);
                }

                var authorId = (await this.dbContext
                    .Trainers
                    .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                    .Id;

                if (article.TrainerId != authorId)
                {
                    return Result.Failure(WrongWrights);
                }

                var isTitleTaken = await this.dbContext
                   .Articles
                   .AnyAsync(x => x.Title == request.Title);

                if (isTitleTaken)
                {
                    return Result<int>.Failure(TitleTaken);
                }

                if (!Enum.IsDefined(typeof(ArticleType), request.ArticleType))
                {
                    return Result<int>.Failure(WrongArticleType);
                }

                if (request.Image.Length > 0)
                {
                    if (request.Image.ContentType != "image/jpeg" &&
                        request.Image.ContentType != "image/jpg" &&
                        request.Image.ContentType != "image/png" &&
                        request.Image.ContentType != "image/bmp")
                    {
                        return Result<int>.Failure(WrongImageFormat);
                    }

                    using (var imageResult = Image.Load(request.Image.OpenReadStream()))
                    {
                        if (imageResult.Width < MinImageWidth || imageResult.Height < MinImageHeight)
                        {
                            return Result<int>.Failure(string.Format(WrongWidthOrHeight, MinImageWidth, MinImageHeight));
                        }

                        var imageData = (await this.dbContext
                            .ArticleImageDatas
                            .Select(x => new
                            {
                                x.Id,
                                x.ArticleId,
                                x.Folder
                            })
                            .FirstAsync(x => x.ArticleId == article.Id));

                        var deleteImageResult = await this.cloudinaryService.DeleteImage(imageData.Id, imageData.Folder);
                        if (deleteImageResult.Succeeded)
                        {
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

                                await this.dbContext.ArticleImageDatas.AddAsync(articleImageData);
                                await this.dbContext.SaveChangesAsync();

                                return Result.Success;
                            }

                            return Result.Failure(uploadImageResult.Errors);
                        }

                        return Result.Failure(deleteImageResult.Errors);
                    }
                }

                return Result.Failure(NoImage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ArticleService)}.{nameof(Edit)}");
                return Result<int>.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }

        public async Task<Result> Delete(DeleteArticleRequestModel request)
        {
            try
            {
                var article = await this.dbContext
                    .Articles
                    .FindAsync(new object[] { request.Id });

                if (article == null)
                {
                    return Result.Failure(ArticleMissing);
                }

                var authorId = (await this.dbContext
                    .Trainers
                    .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                    .Id;

                if (article.TrainerId != authorId)
                {
                    return Result.Failure(WrongWrights);
                }

                var articleImageData = await this.dbContext
                    .ArticleImageDatas
                    .FirstOrDefaultAsync(x => x.ArticleId == article.Id);

                var deleteImageResult = await this.cloudinaryService.DeleteImage(articleImageData.Id, articleImageData.Folder);
                if (!deleteImageResult.Succeeded)
                {
                    Log.Error(string.Join(Environment.NewLine, deleteImageResult.Errors), $"{nameof(ArticleService)}.{nameof(Delete)}");
                }

                this.dbContext.ArticleImageDatas.Remove(articleImageData);
                this.dbContext.Articles.Remove(article);

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ArticleService)}.{nameof(Delete)}");
                return Result<int>.Failure(string.Format(Wrong, nameof(Delete)));
            }
        }
    }
}
