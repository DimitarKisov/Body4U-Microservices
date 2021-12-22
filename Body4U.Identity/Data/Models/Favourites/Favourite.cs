namespace Body4U.Identity.Data.Models.Favourites
{
    using Body4U.Identity.Data.Models.Identity;
    using System.ComponentModel.DataAnnotations;

    public class Favourite
    {
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public int ArticleId { get; set; }
    }
}
