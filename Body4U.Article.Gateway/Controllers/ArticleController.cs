﻿namespace Body4U.Article.Gateway.Controllers
{
    using Body4U.Article.Gateway.Models.Responses;
    using Body4U.Article.Gateway.Services;
    using Body4U.Common.Controllers;
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Comment.Requests;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Identity.Models.Favourites.Responses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Refit;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Article;
    using static Body4U.Common.Constants.MessageConstants.Common;

    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ArticleController : ApiController
    {
        private readonly IArticleService articleService;
        private readonly IIdentityService identityService;
        private readonly IFavouritesService favouritesService;
        private readonly ICommentService commentService;

        public ArticleController(
            IArticleService articleService,
            IIdentityService identityService,
            IFavouritesService favouritesService,
            ICommentService commentService)
        {
            this.articleService = articleService;
            this.identityService = identityService;
            this.favouritesService = favouritesService;
            this.commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
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
                return this.ProcessApiErrors(ex);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Comments))]
        public async Task<ActionResult<List<SearchCommentsResponseModel>>> Comments(SearchCommentsRequestModel request)
        {
            try
            {
                var comments = await this.commentService.Search(request);
                var users = await this.identityService.GetUsersInfo(comments.Select(x => x.ApplicationUserId).ToList());

                foreach (var comment in comments)
                {
                    var userFullName = users.First(x => x.Id == comment.ApplicationUserId).FullName;
                    comment.AuthorName = userFullName;
                }

                return this.Ok(comments);
            }
            catch (ApiException ex)
            {
                return this.ProcessApiErrors(ex);
            }
        }

        [HttpPost]
        [Route(nameof(AddToFavourites))]
        public async Task<ActionResult> AddToFavourites(AddToFavouritesRequestModel request)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }

                var articleExists = await this.articleService.ArticleExists(request.ArticleId);
                if (!articleExists)
                {
                    this.ModelState.Clear();
                    return this.NotFound(ArticleMissing);
                }

                await this.favouritesService.Add(request);

                return this.NoContent();
            }
            catch (ApiException ex)
            {
                return this.ProcessApiErrors(ex);
            }
        }

        [HttpDelete]
        [Route(nameof(RemoveFromFavourites))]
        public async Task<ActionResult> RemoveFromFavourites(RemoveFromFavouritesRequestModel request)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }

                var articleExists = await this.articleService.ArticleExists(request.ArticleId);
                if (!articleExists)
                {
                    this.ModelState.Clear();
                    return this.NotFound(ArticleMissing);
                }

                await this.favouritesService.Remove(request);

                return this.NoContent();
            }
            catch (ApiException ex)
            {
                return this.ProcessApiErrors(ex);
            }
        }

        [HttpGet]
        [Route(nameof(Favourites))]
        public async Task<ActionResult<SearchFavouritesResponseModel>> Favourites([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
        {
            try
            {
                var articlesIds = await this.favouritesService.Mines();
                var favouriteArticles = await this.articleService.Favourites(new SearchFavouritesRequestModel()
                {
                    ArticlesIds = articlesIds,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });

                return this.Ok(favouriteArticles);
            }
            catch (ApiException ex)
            {
                return this.ProcessApiErrors(ex);
            }
        }

        private BadRequestObjectResult ProcessApiErrors(ApiException ex)
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
                errors.Add(ServerError);
            }

            return this.BadRequest(errors);
        }
    }
}
