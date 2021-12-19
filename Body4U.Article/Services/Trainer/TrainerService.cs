namespace Body4U.Article.Services.Trainer
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Article.Models.Requests.Trainer;
    using Body4U.Article.Models.Responses.Trainer;
    using Body4U.Common;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using SixLabors.ImageSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Trainer;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Trainer;

    public class TrainerService : ITrainerService
    {
        private readonly ArticleDbContext dbContext;
        private readonly ICurrentUserService currentUserService;
        private readonly ICloudinaryService cloudinaryService;

        public TrainerService(
            ArticleDbContext dbContext,
            ICurrentUserService currentUserService,
            ICloudinaryService cloudinaryService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<Result<MyTrainerProfileResponseModel>> MyProfile()
        {
            try
            {
                var trainer = await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

                //if (trainer == null)
                //{
                //    return Result<MyTrainerProfileResponseModel>.Failure(string.Format(TrainerNotFound, this.currentUserService.UserId));
                //}

                var result = new MyTrainerProfileResponseModel
                {
                    Id = trainer.Id,
                    Bio = trainer.Bio,
                    ShortBio = trainer.ShortBio,
                    FacebookUrl = trainer.FacebookUrl,
                    InstagramUrl = trainer.InstagramUrl,
                    YoutubeChannelUrl = trainer.YoutubeChannelUrl
                };

                return Result<MyTrainerProfileResponseModel>.SuccessWith(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(MyProfile)}");
                return Result<MyTrainerProfileResponseModel>.Failure(string.Format(Wrong, nameof(MyProfile)));
            }
        }

        public async Task<Result<List<string>>> MyImages()
        {
            try
            {
                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

                //if (trainerId == null)
                //{
                //    return Result<List<string>>.Failure(string.Format(TrainerNotFound, this.currentUserService.UserId));
                //}

                var result = await this.dbContext
                .TrainerImagesDatas
                .Where(x => x.TrainerId == (int)trainerId)
                .Select(x => x.Url)
                .ToListAsync();

                return Result<List<string>>.SuccessWith(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(MyImages)}");
                return Result<List<string>>.Failure(string.Format(Wrong, nameof(MyImages)));
            }
        }

        public async Task<Result<List<string>>> MyVideos()
        {
            try
            {
                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

                //if (trainerId == null)
                //{
                //    return Result<List<string>>.Failure(string.Format(TrainerNotFound, this.currentUserService.UserId));
                //}

                var result = await this.dbContext
                    .TrainerVideos
                    .Where(x => x.TrainerId == (int)trainerId)
                    .Select(x => x.VideoUrl)
                    .ToListAsync();

                return Result<List<string>>.SuccessWith(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(MyVideos)}");
                return Result<List<string>>.Failure(string.Format(Wrong, nameof(MyVideos)));
            }
        }

        public async Task<Result> Edit(EditMyTrainerProfileRequestModel request)
        {
            try
            {
                var trainer = await this.dbContext.Trainers.FindAsync(new object[] { request.Id });

                if (request.Id != trainer.Id && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                if (trainer == null)
                {
                    return Result.Failure(TrainerNotFound);
                }

                if (!string.IsNullOrWhiteSpace(request.Bio))
                {
                    trainer.Bio = request.Bio.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.ShortBio))
                {
                    trainer.ShortBio = request.ShortBio.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.FacebookUrl))
                {
                    trainer.FacebookUrl = request.FacebookUrl.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.InstagramUrl))
                {
                    trainer.InstagramUrl = request.InstagramUrl.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.YoutubeChannelUrl))
                {
                    trainer.YoutubeChannelUrl = request.YoutubeChannelUrl.Trim();
                }

                if (trainer.Bio != null && trainer.ShortBio != null)
                {
                    trainer.IsReadyToWrite = true;
                    trainer.IsReadyToVisualize = true;
                }
                else
                {
                    trainer.IsReadyToWrite = false;
                    trainer.IsReadyToVisualize = false;
                }

                trainer.ModifiedOn = DateTime.Now;
                trainer.ModifiedBy = currentUserService.UserId;

                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }

        public async Task<Result> UploadTrainerImages(UploadImagesRequestModel request)
        {
            try
            {
                if (request.Images.Count > ImagesCountLimit)
                {
                    return Result.Failure(string.Format(MaxAllowedImages, ImagesCountLimit));
                }

                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

                if (trainerId == null)
                {
                    return Result.Failure(TrainerNotFound);
                }

                var trainerImagesCount = await this.dbContext
                    .TrainerImagesDatas
                    .Where(x => x.TrainerId == (int)trainerId)
                    .CountAsync();

                var imagesCountLeft = ImagesCountLimit - trainerImagesCount;
                if (request.Images.Count > imagesCountLeft)
                {
                    return Result.Failure(string.Format(TooManyImages, ImagesCountLimit, imagesCountLeft));
                }

                var imagesStreams = new List<Stream>();
                var addImages = false;
                if (request.Images.Count > 0)
                {
                    if (request.Images.
                        Any(x => x.ContentType != "image/jpeg" &&
                                 x.ContentType != "image/jpg" &&
                                 x.ContentType != "image/png" &&
                                 x.ContentType != "image/bmp"))
                    {
                        return Result.Failure(MultipleWrongImageFormats);
                    }

                    foreach (var image in request.Images)
                    {
                        using (var imageResult = Image.Load(image.OpenReadStream()))
                        {
                            if (imageResult.Width < TrainerImageWidth || imageResult.Height < TrainerImageHeight)
                            {
                                return Result.Failure(string.Format(MultipleWrongWidthOrHeight, TrainerImageWidth, TrainerImageHeight));
                            }
                        }
                    }

                    addImages = true;
                    var errosInImageUploading = new List<string>();

                    if (addImages)
                    {
                        foreach (var imageStream in imagesStreams)
                        {
                            var id = Guid.NewGuid().ToString();
                            var totalImages = await this.dbContext.TrainerImagesDatas.CountAsync();
                            var folder = $"Trainer/Images/{totalImages % 1000}";

                            var result = await this.cloudinaryService.UploadImage(imageStream, id, folder);
                            if (result.Succeeded)
                            {
                                var trainerImageData = new TrainerImageData
                                {
                                    Id = result.Data.PublicId,
                                    Url = result.Data.Url,
                                    Folder = folder,
                                    TrainerId = (int)trainerId
                                };

                                await this.dbContext.TrainerImagesDatas.AddAsync(trainerImageData);
                            }
                            else
                            {
                                errosInImageUploading.AddRange(result.Errors);
                            }
                        }

                        await this.dbContext.SaveChangesAsync();
                    }

                    return Result.Success;
                }

                return Result.Failure(MultipleNoImagesChosen);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(UploadTrainerImages)}");
                return Result.Failure(string.Format(Wrong, nameof(UploadTrainerImages)));
            }
        }

        public async Task<Result> DeleteTrainerImage(DeleteImageRequestModel request)
        {
            try
            {
                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

                if (trainerId == null)
                {
                    return Result.Failure(TrainerNotFound);
                }

                if (request.TrainerId != (int)trainerId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                var image = await this.dbContext
                    .TrainerImagesDatas
                    .FirstOrDefaultAsync(x => x.TrainerId == request.TrainerId);

                if (image == null)
                {
                    return Result.Failure(ImageNotFound);
                }

                var result = await this.cloudinaryService.DeleteImage(image.Id, image.Folder);
                if (result.Succeeded)
                {
                    this.dbContext.TrainerImagesDatas.Remove(image);
                    await this.dbContext.SaveChangesAsync();

                    return Result.Success;
                }

                return Result.Failure(result.Errors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(DeleteTrainerImage)}");
                return Result.Failure(string.Format(Wrong, nameof(DeleteTrainerImage)));
            }
        }
    }
}