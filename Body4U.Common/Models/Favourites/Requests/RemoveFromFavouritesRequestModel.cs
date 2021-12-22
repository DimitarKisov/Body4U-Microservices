namespace Body4U.Common.Models.Favourites.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class RemoveFromFavouritesRequestModel
    {
        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public int ArticleId { get; set; }
    }
}
