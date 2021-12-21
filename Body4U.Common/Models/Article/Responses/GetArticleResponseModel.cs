namespace Body4U.Common.Models.Article.Responses
{
    using System;

    public class GetArticleResponseModel
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ArticleType { get; set; }

        public int TrainerId { get; set; }

        public string ShortBio { get; set; }

        public string TrainerFacebookUrl { get; set; }

        public string TrainerInstagramUrl { get; set; }

        public string TrainerYoutubeChannelUrl { get; set; }

        public string ApplicationUserId { get; set; }
    }
}
