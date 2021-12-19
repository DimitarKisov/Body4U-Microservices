namespace Body4U.Article.Services.Trainer
{
    using Body4U.Article.Models.Requests.Trainer;
    using Body4U.Article.Models.Responses.Trainer;
    using Body4U.Common;
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
