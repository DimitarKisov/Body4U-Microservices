﻿namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Article.Services.Article;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Models.Favourites.Requests;
    using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult> Create([FromForm] CreateArticleRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.CreatedAtAction(nameof(Get), new { id = result.Data }, result.Data);
        }

        [HttpPut]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit([FromForm] EditArticleRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
        public async Task<ActionResult> Get(int id)
        {
            var result = await this.articleService.Get(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Search))]
        public async Task<ActionResult> Search([FromQuery] SearchArticlesRequestModel request)
        {
            var result = await this.articleService.Search(request);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(AutocompleteArticleTitle) + PathSeparator + Term)]
        public async Task<ActionResult> AutocompleteArticleTitle(string term)
        {
            var result = await this.articleService.AutocompleteArticleTitle(term);

            return this.Ok(result.Data);
        }

        [HttpGet]
        [Route(nameof(ArticleExists) + PathSeparator + Id)]
        public async Task<ActionResult> ArticleExists(int id)
        {
            var result = await this.articleService.ArticleExists(id);

            return this.Ok(result.Data);
        }

        [HttpPost]
        [Route(nameof(Favourites))]
        public async Task<ActionResult> Favourites(SearchFavouritesRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.articleService.Favourites(request);

            return this.Ok(result.Data);
        }
    }
}
