namespace Body4U.Common.Services.Cloud
{
    using Body4U.Common.Models.Cloud.Responses;
    using System.IO;
    using System.Threading.Tasks;

    public interface ICloudinaryService
    {
        Task<Result<UploadImageResponseModel>> UploadImage(Stream image, string publicId, string folder);

        Task<Result> DeleteImage(string publicId, string folder);
    }
}
