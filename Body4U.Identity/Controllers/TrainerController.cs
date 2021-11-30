namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Identity.Infrastructure;
    using Body4U.Identity.Services.Trainer;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class TrainerController : ApiController
    {
        private readonly ITrainerService trainerService;

        public TrainerController(ITrainerService trainerService)
        {
            this.trainerService = trainerService;
        }

        [HttpGet]
        [Route(nameof(MyProfile))]
        public async Task<ActionResult> MyProfile()
        {
            var result = await this.trainerService.MyProfile();
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }
    }
}
