namespace Body4U.Guide.Services.Food
{
    using Body4U.Common;
    using Body4U.Guide.Data;
    using Body4U.Guide.Data.Models;
    using Body4U.Guide.Models.Requests.Food;
    using Body4U.Guide.Models.Responses.Food;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Food;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class FoodService : IFoodService
    {
        private readonly GuideDbContext dbContext;

        public FoodService(GuideDbContext dbContext)
            => this.dbContext = dbContext;

        public async Task<Result<CreateFoodResponseModel>> Create(CreateFoodRequestModel request)
        {
            if (!Enum.IsDefined(typeof(FoodCategory), request.FoodCategory))
            {
                return Result<CreateFoodResponseModel>.Failure(BadRequest, WrongFoodCategory);
            }

            var nameExists = await this.dbContext
                .Foods
                .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result<CreateFoodResponseModel>.Failure(Conflict, NameTaken);
            }

            var strategy = this.dbContext.Database.CreateExecutionStrategy();
            var foodId = 0;

            var result = await strategy.Execute(async () =>
            {
                using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                {
                    var food = new Food()
                    {
                        Name = request.Name,
                        Protein = request.Protein,
                        Carbohydrates = request.Carbohydrates,
                        Fats = request.Fats,
                        Calories = request.Calories,
                        FoodCategory = (FoodCategory)request.FoodCategory
                    };

                    try
                    {
                        await this.dbContext.Foods.AddAsync(food);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{nameof(FoodService)}/{nameof(Create)} when adding food in database.");
                        return false;
                    }

                    foodId = food.Id;

                    var otherFoodValues = new OtherFoodValues()
                    {
                        Water = request.OtherValues.Water,
                        Fiber = request.OtherValues.Fiber,
                        Calcium = request.OtherValues.Calcium,
                        Magnesium = request.OtherValues.Magnesium,
                        Potassium = request.OtherValues.Potassium,
                        Zinc = request.OtherValues.Zinc,
                        Manganese = request.OtherValues.Manganese,
                        VitaminC = request.OtherValues.VitaminC,
                        VitaminA = request.OtherValues.VitaminA,
                        VitaminE = request.OtherValues.VitaminE,
                        Sugars = request.OtherValues.Sugars,
                        FoodId = food.Id
                    };

                    try
                    {
                        await this.dbContext.OtherFoodValues.AddAsync(otherFoodValues);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{nameof(FoodService)}/{nameof(Create)} when adding otherFoodValues in database.");
                        return false;
                    }

                    await transaction.CommitAsync();
                    return true;
                }
            });

            if (!result)
            {
                return Result<CreateFoodResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            return Result<CreateFoodResponseModel>.SuccessWith(new CreateFoodResponseModel() { Id = foodId });
        }

        public async Task<Result<GetFoodResponseModel>> Get(int id)
        {
            var food = await this.dbContext
                .Foods
                .Select(x => new GetFoodResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Protein = x.Protein,
                    Carbohydrates = x.Carbohydrates,
                    Fats = x.Fats,
                    Calories = x.Calories
                })
                .FirstOrDefaultAsync(x => x.Id == x.Id);

            if (food == null)
            {
                return Result<GetFoodResponseModel>.Failure(NotFound, FoodMissing);
            }

            var otherFoodValues = await this.dbContext
                .OtherFoodValues
                .Where(x => x.FoodId == food.Id)
                .Select(x => new GetOtherFoodValuesResponseModel()
                {
                    Water = x.Water,
                    Fiber = x.Fiber,
                    Calcium = x.Calcium,
                    Magnesium = x.Magnesium,
                    Potassium = x.Potassium,
                    Zinc = x.Zinc,
                    Manganese = x.Manganese,
                    VitaminC = x.VitaminC,
                    VitaminA = x.VitaminA,
                    VitaminE = x.VitaminE,
                    Sugars = x.Sugars
                })
                .FirstOrDefaultAsync();

            if (otherFoodValues == null)
            {
                return Result<GetFoodResponseModel>.Failure(NotFound, OtherValuesMissing);
            }

            food.OtherValues = otherFoodValues;

            return Result<GetFoodResponseModel>.SuccessWith(food);
        }

        public async Task<Result<SearchFoodsResponseModel>> Search(SearchFoodsRequestModel request)
        {
            var foods = this.dbContext
                    .Foods
                    .Select(x => new FoodResponseModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Protein = x.Protein,
                        Carbohydrates = x.Carbohydrates,
                        Fats = x.Fats,
                        Calories = x.Calories
                    })
                    .AsQueryable();

            var totalRecords = await foods.CountAsync();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var sortingOrder = request.OrderBy!;
            var sortingField = request.SortBy!;

            Expression<Func<FoodResponseModel, object>> sortingExpression = x => x.Id;

            if (!string.IsNullOrWhiteSpace(sortingField))
            {
                if (sortingField.ToLower() == "name")
                {
                    sortingExpression = x => x.Name;
                }
                else if (sortingField.ToLower() == "protein")
                {
                    sortingExpression = x => x.Protein;
                }
                else if (sortingField.ToLower() == "carbohydrates")
                {
                    sortingExpression = x => x.Carbohydrates;
                }
                else if (sortingField.ToLower() == "fats")
                {
                    sortingExpression = x => x.Fats;
                }
                else if (sortingField.ToLower() == "calories")
                {
                    sortingExpression = x => x.Calories;
                }
            }

            if (sortingOrder != null && sortingOrder.ToLower() == Desc)
            {
                foods = foods.OrderByDescending(sortingExpression);
            }
            else
            {
                foods = foods.OrderBy(sortingExpression);
            }

            var data = await foods
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<SearchFoodsResponseModel>.SuccessWith(new SearchFoodsResponseModel() { Data = data, TotalRecords = totalRecords });
        }

        public async Task<Result> Edit(EditFoodRequestModel request)
        {
            if (!Enum.IsDefined(typeof(FoodCategory), request.FoodCategory))
            {
                return Result.Failure(BadRequest, WrongFoodCategory);
            }

            var nameExists = await this.dbContext
                .Foods
                .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result.Failure(Conflict, NameTaken);
            }

            var food = await this.dbContext
                .Foods
                .FindAsync(new object[] { request.Id });

            if (food == null)
            {
                return Result.Failure(NotFound, FoodMissing);
            }

            food.Name = request.Name.Trim();
            food.Protein = request.Protein;
            food.Carbohydrates = request.Carbohydrates;
            food.Fats = request.Fats;
            food.Calories = request.Calories;

            if (request.OtherFoodValues != null)
            {
                var otherFoodValues = await this.dbContext
                .OtherFoodValues
                .FindAsync(request.OtherFoodValues.Id);

                if (otherFoodValues == null)
                {
                    return Result.Failure(NotFound, OtherValuesMissing);
                }

                otherFoodValues.Water = request.OtherFoodValues.Water;
                otherFoodValues.Fiber = request.OtherFoodValues.Fiber;
                otherFoodValues.Calcium = request.OtherFoodValues.Calcium;
                otherFoodValues.Magnesium = request.OtherFoodValues.Magnesium;
                otherFoodValues.Potassium = request.OtherFoodValues.Potassium;
                otherFoodValues.Zinc = request.OtherFoodValues.Zinc;
                otherFoodValues.VitaminC = request.OtherFoodValues.VitaminC;
                otherFoodValues.VitaminA = request.OtherFoodValues.VitaminA;
                otherFoodValues.VitaminE = request.OtherFoodValues.VitaminE;
                otherFoodValues.Sugars = request.OtherFoodValues.Sugars;
            }

            try
            {
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(FoodService)}/{nameof(Edit)} while editing food or it's other values");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result> Delete(int id)
        {
            var food = await this.dbContext
                .Foods
                .Select(x => new Food()
                {
                    Id = x.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (food == null)
            {
                return Result.Failure(NotFound, FoodMissing);
            }

            var otherFoodValues = await this.dbContext
                .OtherFoodValues
                .Where(x => x.FoodId == food.Id)
                .Select(x => new OtherFoodValues()
                {
                    Id = x.Id
                })
                .FirstOrDefaultAsync();

            if (otherFoodValues != null)
            {
                try
                {
                    this.dbContext.OtherFoodValues.Remove(otherFoodValues);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{nameof(FoodService)}/{nameof(Edit)} while removing food's values");
                    return Result.Failure(InternalServerError, UnhandledError);
                }
            }

            try
            {
                this.dbContext.Foods.Remove(food);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(FoodService)}/{nameof(Edit)} while removing food or saving changes");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result<Dictionary<int, string>>> AutocompleteFoodName(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var foodsNames = await this.dbContext.Foods
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .Where(x => x.Name.ToLower().Contains(term.ToLower()))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

                return Result<Dictionary<int, string>>.SuccessWith(foodsNames);
            }

            return Result<Dictionary<int, string>>.SuccessWith(new Dictionary<int, string>());
        }
    }
}
