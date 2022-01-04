namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Article.Services.Service;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class ServiceController : ApiController
    {
        private readonly IServiceService serviceService;

        public ServiceController(IServiceService serviceService)
            => this.serviceService = serviceService;

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateServiceRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.serviceService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }
    }
}
