namespace Body4U.Admin.Controllers
{
    using Body4U.Admin.Models.Identity;
    using Body4U.Admin.Services.Identity;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : AdministratorController
    {
        private readonly IIdentityService identityService;

        public IdentityController(
            IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] LoginUserRequestModel request)
        {
            try
            {
                return await identityService.Login(request);
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<SearchUsersResponseModel>> Users([FromBody] SearchUsersRequestModel request)
        {
            try
            {
                return await this.identityService.AllUsers(request);
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }

        private BadRequestObjectResult ProccessErrors(ApiException ex)
        {
            var errors = new List<string>();

            if (ex.ContentHeaders != null)
            {
                JsonConvert
                    .DeserializeObject<List<string>>(ex.Content)
                    .ForEach(error => errors.Add(error));

                return this.BadRequest(errors);
            }

            var vaex = (ValidationApiException)ex;

            if (ex.HasContent)
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
