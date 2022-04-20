namespace Body4U.Guide.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Guide.Models.Requests.Exercise;
    using Body4U.Guide.Services.Exercise;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class ExerciseController : ApiController
    {
        private readonly IExerciseService exerciseService;

        public ExerciseController(IExerciseService exerciseService)
            => this.exerciseService = exerciseService;

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateExerciseRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.exerciseService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.CreatedAtAction(nameof(Get), new {id = result.Data.Id}, result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
        public async Task<ActionResult> Get(int id)
        {
            var result = await this.exerciseService.Get(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Search))]
        public async Task<ActionResult> Search([FromQuery] SearchExercisesRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.exerciseService.Search(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditExerciseRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.exerciseService.Edit(request);
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
            var result = await this.exerciseService.Delete(id);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(AutocompleteExerciseName) + PathSeparator + Term)]
        public async Task<ActionResult> AutocompleteExerciseName(string term)
        {
            var result = await this.exerciseService.AutocompleteExerciseName(term);

            return this.Ok(result);
        }
    }
}
