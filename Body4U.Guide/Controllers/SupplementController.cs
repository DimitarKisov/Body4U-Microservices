namespace Body4U.Guide.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Guide.Models.Requests.Supplement;
    using Body4U.Guide.Services.Supplement;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class SupplementController : ApiController
    {
        private readonly ISupplementService supplementService;

        public SupplementController(ISupplementService supplementService)
            => this.supplementService = supplementService;

        public async Task<ActionResult> Create(CreateSupplementRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.supplementService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.CreatedAtAction(/*nameof(Get)*/"Get", new { id = result.Data.Id }, result.Data);
        }
    }
}
