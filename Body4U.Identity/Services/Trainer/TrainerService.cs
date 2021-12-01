namespace Body4U.Identity.Services.Trainer
{
    using Body4U.Common;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models;
    using Body4U.Identity.Models.Requests.Trainer;
    using Body4U.Identity.Models.Responses.Trainer;
    using Microsoft.AspNetCore.Identity;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.TrainerConstants;

    public class TrainerService : ITrainerService
    {
        private readonly IdentityDbContext dbContext;
        private readonly ICurrentUserService currentUserService;
        private readonly UserManager<ApplicationUser> userManager;

        public TrainerService(
            IdentityDbContext dbContext,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
            this.userManager = userManager;
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
                Log.Error($"{nameof(TrainerService)}.{nameof(MyProfile)}", ex);
                return Result<MyTrainerProfileResponseModel>.Failure(string.Format(Wrong, nameof(MyProfile)));
            }
        }

        public async Task<Result> Edit(EditMyTrainerProfileRequestModel request)
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
    }
}
