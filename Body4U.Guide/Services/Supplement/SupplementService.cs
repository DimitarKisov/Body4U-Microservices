namespace Body4U.Guide.Services.Supplement
{
    using Body4U.Common;
    using Body4U.Guide.Data;
    using Body4U.Guide.Data.Models;
    using Body4U.Guide.Models.Requests.Supplement;
    using Body4U.Guide.Models.Responses.Supplement;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;
    using static Body4U.Common.Constants.MessageConstants.Supplement;

    public class SupplementService : ISupplementService
    {
        private readonly GuideDbContext dbContext;

        public SupplementService(GuideDbContext dbContext)
            => this.dbContext = dbContext;

        public async Task<Result<CreateSupplementResponseModel>> Create(CreateSupplementRequestModel request)
        {
            if (!Enum.IsDefined(typeof(SupplementCategory), request.Category))
            {
                return Result<CreateSupplementResponseModel>.Failure(BadRequest, WrongSupplementCategory);
            }

            var nameTaken = await this.dbContext
                .Supplements
                .AnyAsync(x => x.Name == request.Name);

            if (nameTaken)
            {
                return Result<CreateSupplementResponseModel>.Failure(Conflict, NameTaken);
            }

            var supplement = new Supplement()
            {
                Name = request.Name,
                Description = request.Description,
                Category = (SupplementCategory)request.Category
            };

            try
            {
                await this.dbContext.Supplements.AddAsync(supplement);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(SupplementService)}/{nameof(Create)} when adding and saving supplement.");
                return Result<CreateSupplementResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            return Result<CreateSupplementResponseModel>.SuccessWith(new CreateSupplementResponseModel() { Id = supplement.Id });
        }

        public async Task<Result<GetSupplementResponseModel>> Get(int id)
        {
            var supplement = await this.dbContext
                .Supplements
                .Where(x => x.Id == id)
                .Select(x => new GetSupplementResponseModel()
                {
                    Name = x.Name,
                    Description = x.Description
                })
                .FirstOrDefaultAsync();

            if (supplement == null)
            {
                return Result<GetSupplementResponseModel>.Failure(NotFound, SupplementMissing);
            }

            return Result<GetSupplementResponseModel>.SuccessWith(supplement);
        }
    }
}
