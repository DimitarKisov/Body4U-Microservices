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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

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
                this.dbContext.Supplements.Add(supplement);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(SupplementService)}/{nameof(Create)}");
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

        public async Task<Result<SearchSupplementsResponseModel>> Search(SearchSupplementsRequestModel request)
        {
            var supplements = this.dbContext
                .Supplements
                .Select(x => new SupplementResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .AsQueryable();

            var totalRecords = await supplements.CountAsync();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var sortingOrder = request.OrderBy;
            var sortingField = request.SortBy;

            Expression<Func<SupplementResponseModel, object>> sortingExpression = x => x.Id;

            if (!string.IsNullOrWhiteSpace(sortingField))
            {
                if (sortingField.ToLower() == "name")
                {
                    sortingExpression = x => x.Name;
                }
            }

            if (sortingOrder != null && sortingOrder.ToLower() == Desc)
            {
                supplements = supplements.OrderByDescending(sortingExpression);
            }
            else
            {
                supplements = supplements.OrderBy(sortingExpression);
            }

            var data = await supplements
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<SearchSupplementsResponseModel>.SuccessWith(new SearchSupplementsResponseModel() { Data = data, TotalRecords = totalRecords });
        }

        public async Task<Result> Edit(EditSupplementRequestModel request)
        {
            if (!Enum.IsDefined(typeof(SupplementCategory), request.Category))
            {
                return Result.Failure(Conflict, WrongSupplementCategory);
            }

            var nameExists = await this.dbContext
                .Supplements
                .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result.Failure(Conflict, NameTaken);
            }

            var supplement = await this.dbContext
                .Supplements
                .FindAsync(new object[] { request.Id });

            if (supplement == null)
            {
                return Result.Failure(NotFound, SupplementMissing);
            }

            supplement.Name = request.Name;
            supplement.Description = request.Description;
            supplement.Category = (SupplementCategory)request.Category;

            try
            {
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(SupplementService)}/{nameof(Edit)}");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result> Delete(int id)
        {
            var supplementExists = await this.dbContext
                .Supplements
                .AnyAsync(x => x.Id == id);

            if (!supplementExists)
            {
                return Result.Failure(NotFound, SupplementMissing);
            }

            try
            {
                this.dbContext.Supplements.Remove(new Supplement() { Id = id });
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(SupplementService)}/{nameof(Delete)}");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result<Dictionary<int, string>>> AutocompleteSupplementName(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var supplementsNames = await this.dbContext.Supplements
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .Where(x => x.Name.ToLower().Contains(term.ToLower()))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

                return Result<Dictionary<int, string>>.SuccessWith(supplementsNames);
            }

            return Result<Dictionary<int, string>>.SuccessWith(new Dictionary<int, string>());
        }
    }
}
