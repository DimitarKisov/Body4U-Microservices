namespace Body4U.Article.Models.Requests.Trainer
{
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;

    public class UploadImagesRequestModel
    {
        public UploadImagesRequestModel()
            => this.Images = new List<IFormFile>();

        public ICollection<IFormFile> Images { get; set; }
    }
}
