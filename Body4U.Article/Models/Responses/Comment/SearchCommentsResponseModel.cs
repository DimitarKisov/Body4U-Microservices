namespace Body4U.Article.Models.Requests.Comment
{
    using System;

    public class SearchCommentsResponseModel
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string AuthorName { get; set; }

        public string ApplicationUserId { get; set; }

        public DateTime DatePosted { get; set; }
    }
}
