namespace Body4U.Common.Models.Favourites.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class AddToFavouritesRequestModel
    {
        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public int ArticleId { get; set; }
    }
}
