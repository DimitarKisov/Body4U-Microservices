namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
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
                            //TODO: Дали е добра идея да направя тригер, който да изтрива потребител от ролята треньор ако няма запис за него в базата, за да избегна постоянно проверката дали има или не? Или пък да я правя още в началото, за да не прави другата логика на горните редове?
                            var trainerId = (await this.dbContext
                                .Trainers
                                .Select(x => new
                                {
                                    x.Id,
                                    x.ApplicationUserId
                                })
                                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                                .Id;

                            if (trainerId == null)
                            {
                                //Еди кво си...
                            }

                            var article = new Article
                            {
                                Title = request.Title.Trim(),
                                Content = request.Content,
                                ArticleType = (ArticleType)request.ArticleType,
                                Sources = request.Sources,
                                CreatedOn = DateTime.Now,
                                TrainerId = (int)trainerId
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
    }
}
