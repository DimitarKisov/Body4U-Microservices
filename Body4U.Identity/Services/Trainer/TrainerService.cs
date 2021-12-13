﻿namespace Body4U.Identity.Services.Trainer
{
    using Body4U.Common;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models.Identity;
    using Body4U.Identity.Data.Models.Trainer;
    using Body4U.Identity.Models.Requests.Trainer;
    using Body4U.Identity.Models.Responses.Trainer;
    using Microsoft.AspNetCore.Identity;
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
        private readonly IdentityDbContext dbContext;
        private readonly ICurrentUserService currentUserService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICloudinaryService cloudinaryService;

        public TrainerService(
            IdentityDbContext dbContext,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
            this.userManager = userManager;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<Result<MyTrainerProfileResponseModel>> MyProfile()
        {
            try
            {
                var trainer = await this.dbContext.Trainers.FindAsync(new object[] { currentUserService.TrainerId });
                if (trainer == null)
                {
                    return Result<MyTrainerProfileResponseModel>.Failure(string.Format(TrainerNotFound, currentUserService.TrainerId));
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
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(MyProfile)}");
                return Result<MyTrainerProfileResponseModel>.Failure(string.Format(Wrong, nameof(MyProfile)));
            }
        }

        public async Task<Result> Edit(EditMyTrainerProfileRequestModel request)
        {
            try
            {
                if (request.Id != this.currentUserService.TrainerId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                var trainer = await this.dbContext.Trainers.FindAsync(new object[] { request.Id });
                if (trainer == null)
                {
                    return Result.Failure(string.Format(TrainerNotFound, currentUserService.TrainerId));
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

                var trainerImagesCount = await this.dbContext
                    .TrainerImagesDatas
                    .Where(x => x.TrainerId == (int)this.currentUserService.TrainerId)
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
                                var userImageData = new TrainerImageData
                                {
                                    Id = result.Data.PublicId,
                                    Url = result.Data.Url,
                                    TrainerId = (int)this.currentUserService.TrainerId
                                };

                                await this.dbContext.TrainerImagesDatas.AddAsync(userImageData);
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
                if (request.TrainerId != this.currentUserService.TrainerId && !this.currentUserService.IsAdministrator)
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

                var result = await this.cloudinaryService.DeleteImage(image.Id);
                if (!result.Succeeded)
                {
                    return Result.Failure(result.Errors);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(TrainerService)}.{nameof(DeleteTrainerImage)}");
                return Result.Failure(string.Format(Wrong, nameof(DeleteTrainerImage)));
            }
        }
    }
}