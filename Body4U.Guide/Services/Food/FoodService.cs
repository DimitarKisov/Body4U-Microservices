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
    using System.Linq;
    using System.Threading.Tasks;

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
    }
}
