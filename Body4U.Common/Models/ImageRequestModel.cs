namespace Body4U.Common.Models
{
    using System.IO;

    public class ImageRequestModel
    {
        public string FileName { get; set; }

        public string Type { get; set; }

        public Stream Content { get; set; }
    }
}
