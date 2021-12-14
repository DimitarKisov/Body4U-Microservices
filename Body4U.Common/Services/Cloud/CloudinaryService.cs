namespace Body4U.Common.Services.Cloud
{
    using Body4U.Common.Models.Cloud.Responses;
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }
        
        public async Task<Result<UploadImageResponseModel>> UploadImage(Stream image, string publicId, string folder)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription($"{publicId}", image),
                PublicId = publicId,
                Format = "jpeg",
                Folder = folder
                
            };
            var uploadResult = await this.cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                return Result<UploadImageResponseModel>.Failure(uploadResult.Error.Message);
            }

            return Result<UploadImageResponseModel>.SuccessWith(new UploadImageResponseModel { PublicId = publicId, Url = uploadResult.Uri.AbsoluteUri });
        }

        public async Task<Result> DeleteImage(string publicId, string folder)
        {
            var deleteParams = new DeletionParams($"{folder}/{publicId}");

            var result = await this.cloudinary.DestroyAsync(deleteParams);
            if (result.Result != "ok")
            {
                return Result.Failure(result.Error.Message);
            }

            return Result.Success;
        }
    }
}
