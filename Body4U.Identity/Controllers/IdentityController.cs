namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Messages.Identity;
    using Body4U.Common.Models;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Identity.Models.Requests.Identity;
    using Body4U.Identity.Services;
    using Body4U.Identity.Services.Identity;
    using MassTransit;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identityService;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly IBus publisher;

        public IdentityController(
            IIdentityService identityService,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            IBus publisher)
        {
            this.identityService = identityService;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
            this.publisher = publisher;
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(Register))]
        public async Task<ActionResult> Register([FromForm] RegisterUserRequestModel request) //TODO: Ако е с атрибут [FromForm] в слагер можем да изберем снимка, но в Postman не можем да го пуснем, защото не байндва нито едно пропърти. Ако е без атрибуте [FromForm] в постман работи, но в слагер не можем да изберем файл. Виж дали има начин да се направи да работи и с двете
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.Register(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            var confirmationLink = Url.Action(nameof(VerifyEmail), "Identity",
                   new { UserId = result.Data.UserId, Token = result.Data.Token }, Request.Scheme, Request.Host.ToString());

            await this.publisher.Publish(new SendEmailMessage()
            {
                To = result.Data.Email,
                Subject = EmailConfirmSubject,
                HtmlContent = string.Format(EmailConfirmHtmlContent, confirmationLink)
            });

            return this.Ok(result.Data.ErrorsInImageUploading);
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<ActionResult> Login(LoginUserRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.Login(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize]
        [Route(nameof(MyProfile))]
        public async Task<ActionResult> MyProfile()
        {
            var result = await this.identityService.MyProfile();
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Authorize]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit([FromForm] EditMyProfileRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(AddProfilePicture))]
        public async Task<ActionResult> AddProfilePicture([FromForm] AddProfilePictureRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.AddProfilePicture(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route(nameof(DeleteProfilePicture))]
        public async Task<ActionResult> DeleteProfilePicture(DeleteProfilePictureRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.DeleteProfilePicture(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPut]
        [Authorize]
        [Route(nameof(ChangePassword))]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.ChangePassword(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost]
        [Route(nameof(ForgotPassword))]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.ForgotPassword(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            if (result.Data.Email == null && result.Data.Token == null && result.Data.UserId == null)
            {
                return Ok();
            }

            var confirmationLink = Url.Action(nameof(ResetPassword), "Identity",
                   new { UserId = result.Data.UserId, Token = result.Data.Token }, Request.Scheme, Request.Host.ToString());

            await this.publisher.Publish(new SendEmailMessage()
            {
                To = result.Data.Email,
                Subject = ForgotPasswordSubject,
                HtmlContent = string.Format(ForgotPasswordHtmlContent, confirmationLink)
            });

            return this.Ok();
        }

        [HttpPost]
        [Route(nameof(ResetPassword))]
        public async Task<ActionResult> ResetPassword([FromQuery] string userId, [FromQuery] string token, ResetPasswordRequestModel request)
        {
            if (userId == null || token == null)
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.ResetPassword(userId, token, request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }

        [HttpPost]
        [Route(nameof(VerifyEmail))]
        public async Task<ActionResult> VerifyEmail([FromQuery] VerifyEmailRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.VerifyEmail(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        [Route(nameof(GenerateRefreshToken))]
        public async Task<ActionResult> GenerateRefreshToken()
        {
            var result = await this.jwtTokenGeneratorService.GenerateRefreshToken();
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [AuthorizeAdministrator]
        [Route(nameof(Users))]
        public async Task<ActionResult> Users(SearchUsersRequestModel request)
        {
            var result = await this.identityService.Users(request);
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [AuthorizeAdministrator]
        [Route(nameof(Roles))]
        public async Task<ActionResult> Roles()
        {
            var result = await this.identityService.Roles();
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }

        [HttpPost]
        [AuthorizeAdministrator]
        [Route(nameof(EditUserRoles))]
        public async Task<ActionResult> EditUserRoles(EditUserRolesRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.EditUserRoles(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpGet]
        [Route(nameof(GetUserInfo) + PathSeparator + Id)]
        public async Task<ActionResult> GetUserInfo(string id)
        {
            var result = await this.identityService.GetUserInfo(id);
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(result.Data);
        }
    }
}
