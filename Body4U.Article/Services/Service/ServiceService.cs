namespace Body4U.Article.Services.Service
{
    using Body4U.Article.Data;
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Common;
    using Body4U.Common.Services.Identity;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Service;

    public class ServiceService : IServiceService
    {
        private readonly ArticleDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public ServiceService(
            ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result<int>> Create(CreateServiceRequestModel request)
        {
            try
            {
                var nameExists = await this.dbContext
                    .Services
                    .AnyAsync(x => x.Name == request.Name);

                if (nameExists)
                {
                    return Result<int>.Failure(NameTaken);
                }

                if (!Enum.IsDefined(typeof(ServiceType), request.ServiceType))
                {
                    return Result<int>.Failure(WrongServiceType);
                }

                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                    .Id;

                var service = new Service
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    ServiceType = (ServiceType)request.ServiceType,
                    TrainerId = trainerId
                };

                await this.dbContext.Services.AddAsync(service);
                await this.dbContext.SaveChangesAsync();

                return Result<int>.SuccessWith(service.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ServiceService)}.{nameof(Create)}");
                return Result<int>.Failure(string.Format(Wrong, nameof(Create)));
            }
        }
    }
}
