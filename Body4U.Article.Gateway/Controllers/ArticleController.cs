namespace Body4U.Article.Gateway.Controllers
{
    using Body4U.Article.Gateway.Models.Responses;
    using Body4U.Article.Gateway.Services;
    using Body4U.Common.Controllers;
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
        private readonly IIdentityService identityService;

        public ArticleController(
            IArticleService articleService,
            IIdentityService identityService)
        {
            this.articleService = articleService;
            this.identityService = identityService;
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<ActionResult<GetArticleResultResponseModel>> Get(int id)
        {
            try
            {
                var articleInfo = await this.articleService.Get(id);
                var trainerInfo = await this.identityService.GetUserInfo(articleInfo.ApplicationUserId);

                return this.Ok(new GetArticleResultResponseModel()
                {
                    Title = articleInfo.Title,
                    Content = articleInfo.Content,
                    TrainerImageUrl = trainerInfo.ProfileImageUrl,
                    CreatedOn = articleInfo.CreatedOn,
                    ArticleType = articleInfo.ArticleType,
                    TrainerId = articleInfo.TrainerId,
                    TrainerShortBio = articleInfo.ShortBio,
                    TrainerFacebookUrl = articleInfo.TrainerFacebookUrl,
                    TrainerInstagramUrl = articleInfo.TrainerInstagramUrl,
                    TrainerYoutubeChannelUrl = articleInfo.TrainerYoutubeChannelUrl,
                    TrainerFullName = trainerInfo.FullName,
                    TrainerAge = trainerInfo.Age
                });
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }

        private BadRequestObjectResult ProccessErrors(ApiException ex)
        {
            var errors = new List<string>();

            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                errors.Add(HttpStatusCode.NotFound.ToString());
                return this.BadRequest(errors);
            }

            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                errors.Add(HttpStatusCode.Unauthorized.ToString());
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
