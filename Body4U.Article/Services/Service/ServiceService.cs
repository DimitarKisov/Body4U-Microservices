﻿namespace Body4U.Article.Services.Service
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Trainer;
    using static Body4U.Common.Constants.MessageConstants.Service;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

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

        public async Task<Result<CreateServiceResponseModel>> Create(CreateServiceRequestModel request)
        {
            if (!Enum.IsDefined(typeof(ServiceType), request.ServiceType))
            {
                return Result<CreateServiceResponseModel>.Failure(BadRequest, WrongServiceType);
            }

            if (!Enum.IsDefined(typeof(ServiceDifficulty), request.ServiceDifficulty))
            {
                return Result<CreateServiceResponseModel>.Failure(BadRequest, WrongServiceDifficulty);
            }

            var nameExists = await this.dbContext
                .Services
                .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result<CreateServiceResponseModel>.Failure(Conflict, NameTaken);
            }

            var trainerId = (await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId))?
                .Id;

            if (trainerId == null)
            {
                return Result<CreateServiceResponseModel>.Failure(NotFound, TrainerNotFound);
            }

            var service = new Service
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ServiceType = (ServiceType)request.ServiceType,
                ServiceDifficulty = (ServiceDifficulty)request.ServiceDifficulty,
                TrainerId = (int)trainerId
            };

            try
            {
                this.dbContext.Services.Add(service);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ServiceService)}.{nameof(Create)}");
                return Result<CreateServiceResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            return Result<CreateServiceResponseModel>.SuccessWith(new CreateServiceResponseModel() { Id = service.Id });
        }

        public async Task<Result<List<AllServicesResponseModel>>> All(int trainerId)
        {
            var services = await this.dbContext
                    .Services
                    .Where(x => x.TrainerId == trainerId)
                    .Select(x => new AllServicesResponseModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ServiceType = (int)x.ServiceType,
                        ServiceDifficulty = (int)x.ServiceDifficulty
                    })
                    .ToListAsync();

            return Result<List<AllServicesResponseModel>>.SuccessWith(services);
        }

        public async Task<Result<GetServiceResponseModel>> Get(int id)
        {
            var service = await this.dbContext
                .Services
                .Where(x => x.Id == id)
                .Select(x => new GetServiceResponseModel()
                {
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    ServiceDifficulty = (int)x.ServiceDifficulty,
                    ServiceType = (int)x.ServiceType
                })
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return Result<GetServiceResponseModel>.Failure(NotFound, ServiceMissing);
            }

            return Result<GetServiceResponseModel>.SuccessWith(service);
        }

        public async Task<Result> Edit(EditServiceRequestModel request)
        {
            if (!Enum.IsDefined(typeof(ServiceType), request.ServiceType))
            {
                return Result.Failure(BadRequest, WrongServiceType);
            }

            if (!Enum.IsDefined(typeof(ServiceDifficulty), request.ServiceDifficulty))
            {
                return Result.Failure(BadRequest, WrongServiceDifficulty);
            }

            var service = await this.dbContext
                .Services
                .FindAsync(new object[] { request.Id });

            if (service == null)
            {
                return Result.Failure(NotFound, ServiceMissing);
            }

            var trainerId = (await this.dbContext
                .Trainers
                .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                .Id;

            if (service.TrainerId != trainerId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden);
            }

            service.Name = request.Name;
            service.Description = request.Description;
            service.ServiceType = (ServiceType)request.ServiceType;
            service.ServiceDifficulty = (ServiceDifficulty)request.ServiceDifficulty;
            service.Price = request.Price;

            await this.dbContext.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result> Delete(int id)
        {
            var service = await this.dbContext
                    .Services
                    .Select(x => new Service()
                    {
                        Id = x.Id,
                        TrainerId = x.TrainerId
                    })
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (service == null)
            {
                return Result.Failure(NotFound, ServiceMissing);
            }

            var trainerId = (await this.dbContext
                .Trainers
                .FirstAsync(x => x.ApplicationUserId == this.currentUserService.UserId))
                .Id;

            if (service.TrainerId != trainerId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden);
            }

            try
            {
                this.dbContext.Services.Remove(service);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ServiceService)}.{nameof(Delete)}");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }
    }
}
