namespace Body4U.Article.Services.Service
{
    using Body4U.Article.Data;
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Article.Models.Responses.Services;
    using Body4U.Common;
    using Body4U.Common.Models.Service;
    using Body4U.Common.Services.Identity;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Service;

    public class ServiceService : IServiceService
    {
        private readonly ArticleDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public ServiceService(
            ArticleDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
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

                if (!Enum.IsDefined(typeof(ServiceDifficulty), request.ServiceDifficulty))
                {
                    return Result<int>.Failure(WrongServiceDifficulty);
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
                    ServiceDifficulty = (ServiceDifficulty)request.ServiceDifficulty,
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

        public async Task<Result<GetServiceResponseModel>> Get(int id)
        {
            try
            {
                var service = await this.dbContext
                    .Services
                    .FindAsync(new object[] { id });

                if (service == null)
                {
                    return Result<GetServiceResponseModel>.Failure(ServiceMissing);
                }

                return Result<GetServiceResponseModel>.SuccessWith(new GetServiceResponseModel
                {
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    ServiceType = (int)service.ServiceType,
                    ServiceDifficulty = (int)service.ServiceDifficulty
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ServiceService)}.{nameof(Get)}");
                return Result<GetServiceResponseModel>.Failure(string.Format(Wrong, nameof(Get)));
            }
        }

        public async Task<Result> Edit(EditServiceRequestModel request)
        {
            try
            {
                var service = await this.dbContext
                    .Services
                    .FindAsync(new object[] { request.Id });

                if (service == null)
                {
                    return Result.Failure(ServiceMissing);
                }

                var trainerId = (await this.dbContext
                    .Trainers
                    .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                    .Id;

                if (service.TrainerId != trainerId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                service.Name = request.Name;
                service.Description = request.Description;
                service.ServiceType = (ServiceType)request.ServiceType;
                service.ServiceDifficulty = (ServiceDifficulty)request.ServiceDifficulty;
                service.Price = request.Price;

                await this.dbContext.SaveChangesAsync();

                return Result.Success;

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ServiceService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }
    }
}
