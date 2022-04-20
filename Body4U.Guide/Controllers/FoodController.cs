namespace Body4U.Guide.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Guide.Models.Requests.Food;
    using Body4U.Guide.Services.Food;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class FoodController : ApiController
    {
        private readonly IFoodService foodService;

        public FoodController(IFoodService foodService)
            => this.foodService = foodService;

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateFoodRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.foodService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.CreatedAtAction(nameof(Get), new { id = result.Data }, result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
        public async Task<ActionResult> Get(int id)
        {
            var result = await this.foodService.Get(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Search))]
        public async Task<ActionResult> Search([FromQuery] SearchFoodsRequestModel request)
        {
            var result = await this.foodService.Search(request);
            if (!result.Succeeded)
            {
                this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(AutocompleteFoodName) + PathSeparator + Term)]
        public async Task<ActionResult> AutocompleteFoodName(string term)
        {
            var result = await this.foodService.AutocompleteFoodName(term);

            return this.Ok(result.Data);
        }
    }
}
