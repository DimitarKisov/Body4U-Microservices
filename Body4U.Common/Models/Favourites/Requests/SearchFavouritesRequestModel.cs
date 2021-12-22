namespace Body4U.Common.Models.Favourites.Requests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class SearchFavouritesRequestModel
    {
        [Required]
        public List<int> ArticlesIds { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
