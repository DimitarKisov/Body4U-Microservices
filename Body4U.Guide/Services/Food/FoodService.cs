namespace Body4U.Guide.Services.Food
{
    using Body4U.Common;
    using Body4U.Guide.Data;
    using Body4U.Guide.Models.Requests.Food;
    using System;
    using System.Threading.Tasks;

    public class FoodService : IFoodService
    {
        private readonly GuideDbContext dbContext;

        public FoodService(GuideDbContext dbContext)
            => this.dbContext = dbContext;

        public async Task<Result<int>> Create(CreateFoodRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}
