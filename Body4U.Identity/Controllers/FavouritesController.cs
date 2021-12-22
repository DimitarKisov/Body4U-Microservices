namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Identity.Services.Favourites;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Authorize]
    public class FavouritesController : ApiController
    {
        private readonly IFavouritesService favouritesService;

        public FavouritesController(IFavouritesService favouritesService)
        {
            this.favouritesService = favouritesService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<ActionResult> Add(AddToFavouritesRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.favouritesService.Add(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }

        [HttpPost]
        [Route(nameof(Remove))]
        public async Task<ActionResult> Remove(RemoveFromFavouritesRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.favouritesService.Remove(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }
    }
}
