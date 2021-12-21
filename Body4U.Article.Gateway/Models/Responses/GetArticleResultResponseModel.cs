namespace Body4U.Article.Gateway.Models.Responses
{
    using System;

    public class GetArticleResultResponseModel
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string TrainerImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ArticleType { get; set; }

        public int TrainerId { get; set; }

        public string TrainerShortBio { get; set; }

        public string TrainerFacebookUrl { get; set; }

        public string TrainerInstagramUrl { get; set; }

        public string TrainerYoutubeChannelUrl { get; set; }

        public string TrainerFullName { get; set; }

        public int? TrainerAge { get; set; }
    }
}
