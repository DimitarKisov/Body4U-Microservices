namespace Body4U.Identity.Services.Trainer
{
    using Body4U.Common;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Models.Responses.Trainer;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.TrainerConstants;

    public class TrainerService : ITrainerService
    {
        private readonly IdentityDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public TrainerService(
            IdentityDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
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
    }
}
