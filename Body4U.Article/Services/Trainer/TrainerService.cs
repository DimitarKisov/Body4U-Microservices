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
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

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
            var trainer = await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

            if (trainer == null)
            {
                return Result<MyTrainerProfileResponseModel>.Failure(NotFound, TrainerNotFound);
            }

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

        public async Task<Result<List<string>>> MyImages()
        {
            var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

            if (trainerId == null)
            {
                return Result<List<string>>.Failure(NotFound, TrainerNotFound);
            }

            var result = await this.dbContext
            .TrainerImagesDatas
            .Where(x => x.TrainerId == (int)trainerId)
            .Select(x => x.Url)
            .ToListAsync();

            return Result<List<string>>.SuccessWith(result);
        }

        public async Task<Result<List<string>>> MyVideos()
        {
            var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

            if (trainerId == null)
            {
                return Result<List<string>>.Failure(NotFound, TrainerNotFound);
            }

            var result = await this.dbContext
                .TrainerVideos
                .Where(x => x.TrainerId == (int)trainerId)
                .Select(x => x.VideoUrl)
                .ToListAsync();

            return Result<List<string>>.SuccessWith(result);
        }

        public async Task<Result> Edit(EditMyTrainerProfileRequestModel request)
        {
            var trainer = await this.dbContext
                .Trainers
                .FindAsync(new object[] { request.Id });

            if (trainer == null)
            {
                return Result.Failure(NotFound, TrainerNotFound);
            }

            if (request.Id != trainer.Id && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden);
            }

            trainer.Bio = request.Bio.Trim();
            trainer.ShortBio = request.ShortBio.Trim();
            trainer.FacebookUrl = request.FacebookUrl.Trim();
            trainer.InstagramUrl = request.InstagramUrl.Trim();
            trainer.YoutubeChannelUrl = request.YoutubeChannelUrl.Trim();

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

        public async Task<Result> UploadTrainerImages(UploadImagesRequestModel request)
        {
            if (request.Images.Count() > 0)
            {
                if (request.Images
                        .Any(x => x.ContentType != "image/jpeg" &&
                             x.ContentType != "image/jpg" &&
                             x.ContentType != "image/png" &&
                             x.ContentType != "image/bmp"))
                {
                    return Result.Failure(BadRequest, MultipleWrongImageFormats);
                }

                foreach (var image in request.Images)
                {
                    var imageStream = image.OpenReadStream();
                    using (var imageResult = Image.Load(imageStream))
                    {
                        if (imageResult.Width < TrainerImageWidth || imageResult.Height < TrainerImageHeight)
                        {
                            return Result.Failure(BadRequest, string.Format(MultipleWrongWidthOrHeight, TrainerImageWidth, TrainerImageHeight));
                        }
                    }
                }

                if (request.Images.Count > ImagesCountLimit)
                {
                    return Result.Failure(Conflict, string.Format(MaxAllowedImages, ImagesCountLimit)); //TODO: 409?
                }

                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                    .Id;

                if (trainerId == null)
                {
                    return Result.Failure(NotFound, TrainerNotFound);
                }

                var trainerImagesCount = await this.dbContext
                    .TrainerImagesDatas
                    .Where(x => x.TrainerId == (int)trainerId)
                    .CountAsync();

                var imagesCountLeft = ImagesCountLimit - trainerImagesCount;
                if (request.Images.Count > imagesCountLeft)
                {
                    return Result.Failure(Conflict, string.Format(TooManyImages, ImagesCountLimit, imagesCountLeft)); //TODO: 409?
                }

                var errosInImageUploading = new List<string>();

                foreach (var image in request.Images)
                {
                    var id = Guid.NewGuid().ToString();
                    var totalImages = await this.dbContext.TrainerImagesDatas.CountAsync();
                    var folder = $"Trainer/Images/{totalImages % 1000}";

                    var result = await this.cloudinaryService.UploadImage(image.OpenReadStream(), id, folder);
                    if (result.Succeeded)
                    {
                        var trainerImageData = new TrainerImageData
                        {
                            Id = result.Data.PublicId,
                            Url = result.Data.Url,
                            Folder = folder,
                            TrainerId = (int)trainerId
                        };

                        try
                        {
                            await this.dbContext.TrainerImagesDatas.AddAsync(trainerImageData);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"{nameof(TrainerService)}/{nameof(UploadTrainerImages)} while adding images in database.");
                            return Result.Failure(InternalServerError, string.Format(Wrong, nameof(UploadTrainerImages)));
                        }
                    }
                    else
                    {
                        errosInImageUploading.AddRange(result.Errors);
                    }
                }

                if (errosInImageUploading.Count() > 0)
                {
                    Log.Error($"{nameof(TrainerService)}.{nameof(DeleteTrainerImage)} when uploading images to cloudinary service with errors:" + string.Join(Environment.NewLine, errosInImageUploading));
                    return Result.Failure(InternalServerError, string.Format(Wrong, nameof(UploadTrainerImages)) + " when trying to upload images to clouadinary.");
                }

                try
                {
                    await this.dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{nameof(TrainerService)}/{nameof(UploadTrainerImages)} while saving images in database.");
                    return Result.Failure(InternalServerError, string.Format(Wrong, nameof(UploadTrainerImages)));
                }

                return Result.Success;
            }

            return Result.Failure(BadRequest, MultipleNoImagesChosen);
        }

        public async Task<Result> DeleteTrainerImage(DeleteImageRequestModel request)
        {
            var trainerId = (await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                .Id;

            if (trainerId == null)
            {
                return Result.Failure(NotFound, TrainerNotFound);
            }

            if (request.TrainerId != (int)trainerId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden);
            }

            var image = await this.dbContext
                .TrainerImagesDatas
                .FirstOrDefaultAsync(x => x.TrainerId == request.TrainerId);

            if (image == null)
            {
                return Result.Failure(NotFound, ImageNotFound);
            }

            var result = await this.cloudinaryService.DeleteImage(image.Id, image.Folder);
            if (result.Succeeded)
            {
                try
                {
                    this.dbContext.TrainerImagesDatas.Remove(image);
                    await this.dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{nameof(TrainerService)}.{nameof(DeleteTrainerImage)}");
                    return Result.Failure(InternalServerError, string.Format(Wrong, nameof(DeleteTrainerImage)));
                }

                return Result.Success;
            }

            Log.Error($"{nameof(TrainerService)}.{nameof(DeleteTrainerImage)} when deleting image from cloudinary service with errors:" + string.Join(Environment.NewLine, result.Errors));
            return Result.Failure(InternalServerError, string.Format(Wrong, nameof(DeleteTrainerImage)) + " when trying to delete image from clouadinary.");
        }
    }
}