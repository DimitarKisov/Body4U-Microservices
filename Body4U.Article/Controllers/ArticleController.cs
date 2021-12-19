namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests;
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Article.Services.Article;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class ArticleController : ApiController
    {
        private readonly IArticleService articleService;

        public ArticleController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateArticleRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditArticleRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<ActionResult> Delete(DeleteArticleRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Delete(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }
    }
}
