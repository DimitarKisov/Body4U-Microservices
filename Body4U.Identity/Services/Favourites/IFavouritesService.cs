﻿namespace Body4U.Identity.Services.Favourites
{
    using Body4U.Common;
    using Body4U.Common.Models.Favourites.Requests;
    using System.Threading.Tasks;

    public interface IFavouritesService
    {
        Task<Result> Add(AddToFavouritesRequestModel request);

        Task<Result> Remove(RemoveFromFavouritesRequestModel request);
    }
}
