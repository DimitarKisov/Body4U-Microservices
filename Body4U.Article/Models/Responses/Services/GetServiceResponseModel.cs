namespace Body4U.Article.Models.Responses.Services
{
    public class GetServiceResponseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public int ServiceType { get; set; }

        public int ServiceDifficulty { get; set; }
    }
}
