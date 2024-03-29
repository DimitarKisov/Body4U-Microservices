﻿namespace Body4U.Common.Services.Cloud
{
    using Body4U.Common.Models.Cloud.Responses;
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }
        
        public async Task<Result<UploadImageResponseModel>> UploadImage(Stream image, string imageType, string publicId, string folder)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription($"{publicId}", image),
                PublicId = publicId,
                Format = imageType.Split("/")[1],
                Folder = folder
                
            };
            var uploadResult = await this.cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                return Result<UploadImageResponseModel>.Failure(InternalServerError, uploadResult.Error.Message);
            }

            return Result<UploadImageResponseModel>.SuccessWith(new UploadImageResponseModel { PublicId = publicId, Url = uploadResult.Uri.AbsoluteUri });
        }

        public async Task<Result> DeleteImage(string publicId, string folder)
        {
            var deleteParams = new DeletionParams($"{folder}/{publicId}");

            var result = await this.cloudinary.DestroyAsync(deleteParams);
            if (result.Result != "ok")
            {
                return Result.Failure(InternalServerError, result.Error.Message);
            }

            return Result.Success;
        }
    }
}
