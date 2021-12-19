namespace Body4U.Article.Gateway.Controllers
{
    using Body4U.Article.Gateway.Services.Article;
    using Body4U.Article.Gateway.Services.Identity;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Services.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Refit;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class ArticleController : ApiController
    {
        private readonly IArticleService articleService;
        private readonly ITrainerService trainerService;
        private readonly ICurrentUserService currentUserService;

        public ArticleController(
            ITrainerService trainerService,
            IArticleService articleService,
            ICurrentUserService currentUserService)
        {
            this.trainerService = trainerService;
            this.articleService = articleService;
            this.currentUserService = currentUserService;
        }

        //[HttpPost]
        //[AuthorizeTrainer]
        //[Route(nameof(Create))]
        //public async Task<ActionResult<int>> Create([FromForm] CreateArticleRequestModel request)
        //{
        //    try
        //    {
        //        var canTrainerWrite = await this.trainerService.CanTrainerWrite((int)this.currentUserService.TrainerId);
        //        if (canTrainerWrite)
        //        {
        //            //TODO: За да изпратя снимка с рефит, следвах документацията, но пак не сработва. Ако не успея да го оправя го направи така, че да не трябва да се проверява дали треньорът може да пише или не.
        //            return await this.articleService.Create(request);
        //        }

        //        return this.BadRequest("Trainer cannot write");
        //    }
        //    catch (ApiException ex)
        //    {
        //        return this.ProccessErrors(ex);
        //    }
        //}

        private BadRequestObjectResult ProccessErrors(ApiException ex)
        {
            var errors = new List<string>();

            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                errors.Add(HttpStatusCode.NotFound.ToString());
                return this.BadRequest(errors);
            }
            else if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                errors.Add(HttpStatusCode.Unauthorized.ToString());
                return this.BadRequest(errors);
            }
            else if (ex.StatusCode == HttpStatusCode.MethodNotAllowed)
            {
                errors.Add(HttpStatusCode.MethodNotAllowed.ToString());
                return this.BadRequest(errors);
            }

            if (ex.ContentHeaders != null)
            {
                JsonConvert
                    .DeserializeObject<List<string>>(ex.Content)
                    .ForEach(error => errors.Add(error));

                return this.BadRequest(errors);
            }

            var vaex = ex as ValidationApiException;

            if (ex.HasContent && vaex != null)
            {
                foreach (var kvp in vaex.Content.Errors)
                {
                    foreach (var value in kvp.Value)
                    {
                        errors.Add(value);
                    }
                }
            }
            else
            {
                errors.Add(InternalServerError);
            }

            return this.BadRequest(errors);
        }
    }
}
