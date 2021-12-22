namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common;
    using Body4U.Common.Models.Favourites.Requests;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFavouritesService
    {
        [Post("/Favourites/Add")]
        Task Add(AddToFavouritesRequestModel request);

        [Post("/Favourites/Remove")]
        Task Remove(RemoveFromFavouritesRequestModel request);

        [Get("/Favourites/Mines")]
        Task<List<int>> Mines();
    }
}
