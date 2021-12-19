namespace Body4U.Admin.Controllers
{
    using Body4U.Admin.Services.Identity;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class IdentityController : AdministratorController
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
            => this.identityService = identityService;


        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Login))]
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
        [Route(nameof(Users))]
        public async Task<ActionResult<SearchUsersResponseModel>> Users([FromQuery] SearchUsersRequestModel request)
        {
            try
            {
                return await this.identityService.Users(request);
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }

        [HttpPost]
        [Route(nameof(Roles))]
        public async Task<ActionResult<List<RoleResponseModel>>> Roles()
        {
            try
            {
                return await this.identityService.Roles();
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }

        [HttpPost]
        [Route(nameof(EditUserRoles))]
        public async Task<ActionResult> EditUserRoles([FromBody] EditUserRolesRequestModel request)
        {
            try
            {
                await this.identityService.EditUserRoles(request);
                return Ok();
            }
            catch (ApiException ex)
            {
                return this.ProccessErrors(ex);
            }
        }
    }
}
