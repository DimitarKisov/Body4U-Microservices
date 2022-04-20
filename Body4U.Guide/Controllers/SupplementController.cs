namespace Body4U.Guide.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Guide.Models.Requests.Supplement;
    using Body4U.Guide.Services.Supplement;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class SupplementController : ApiController
    {
        private readonly ISupplementService supplementService;

        public SupplementController(ISupplementService supplementService)
            => this.supplementService = supplementService;

        [HttpPost]
        [Route(nameof(Create))]
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

            return this.CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
        public async Task<ActionResult> Get(int id)
        {
            var result = await this.supplementService.Get(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Search))]
        public async Task<ActionResult> Search([FromQuery] SearchSupplementsRequestModel request)
        {
            var result = await this.supplementService.Search(request);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditSupplementRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.supplementService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<ActionResult> Delete([FromBody] int id)
        {
            var result = await this.supplementService.Delete(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(AutocompleteSupplementName) + PathSeparator + Term)]
        public async Task<ActionResult> AutocompleteSupplementName(string term)
        {
            var result = await this.supplementService.AutocompleteSupplementName(term);

            return this.Ok(result);
        }
    }
}
