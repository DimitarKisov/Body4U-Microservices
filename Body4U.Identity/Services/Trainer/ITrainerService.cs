namespace Body4U.Identity.Services.Trainer
{
    using Body4U.Common;
    using Body4U.Identity.Models.Requests.Trainer;
    using Body4U.Identity.Models.Responses.Trainer;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITrainerService
    {
        Task<Result<MyTrainerProfileResponseModel>> MyProfile();

        Task<Result<List<string>>> MyImages();

        Task<Result<List<string>>> MyVideos();

        Task<Result> Edit(EditMyTrainerProfileRequestModel request);

        Task<Result> UploadTrainerImages(UploadImagesRequestModel request);

        Task<Result> DeleteTrainerImage(DeleteImageRequestModel request);
    }
}
