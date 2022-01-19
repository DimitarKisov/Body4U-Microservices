namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests.Trainer;
    using Body4U.Article.Services.Trainer;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class TrainerController : ApiController
    {
        private readonly ITrainerService trainerService;

        public TrainerController(ITrainerService trainerService)
            => this.trainerService = trainerService;

        [HttpGet]
        [Route(nameof(MyProfile))]
        public async Task<ActionResult> MyProfile()
        {
            var result = await this.trainerService.MyProfile();
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpGet]
        [Route(nameof(MyImages))]
        public async Task<ActionResult> MyImages()
        {
            var result = await this.trainerService.MyImages();
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpGet]
        [Route(nameof(MyVideos))]
        public async Task<ActionResult> MyVideos()
        {
            var result = await this.trainerService.MyVideos();
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditMyTrainerProfileRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.trainerService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [Route(nameof(UploadTrainerImages))]
        public async Task<ActionResult> UploadTrainerImages([FromForm] UploadImagesRequestModel request) //For testing purposes I have to use postman, because swagger does not supports multiple files uploading
        {
            var result = await this.trainerService.UploadTrainerImages(request);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpDelete]
        [Route(nameof(DeleteTrainerImage))]
        public async Task<ActionResult> DeleteTrainerImage(DeleteImageRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.trainerService.DeleteTrainerImage(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return NoContent();
        }
    }
}
