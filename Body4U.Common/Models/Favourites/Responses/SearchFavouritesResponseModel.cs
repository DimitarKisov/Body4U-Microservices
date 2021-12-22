namespace Body4U.Identity.Models.Favourites.Responses
{
    using System.Collections.Generic;

    public class SearchFavouritesResponseModel
    {
        public ICollection<FavouriteResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
