namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Messages;
    using Body4U.Common.Messages.Identity;
    using Body4U.Common.Models;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Identity.Data;
    using Body4U.Identity.Models.Requests.Identity;
    using Body4U.Identity.Services;
    using Body4U.Identity.Services.Identity;
    using MassTransit;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identityService;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly IBus publisher;
        private readonly IdentityDbContext dbContext;

        public IdentityController(
            IIdentityService identityService,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            IBus publisher,
            IdentityDbContext dbContext)
        {
            this.identityService = identityService;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
            this.publisher = publisher;
            this.dbContext = dbContext;
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Route(nameof(Register))]
        public async Task<ActionResult> Register([FromForm] RegisterUserRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.Register(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            var confirmationLink = Url.Action(nameof(VerifyEmail), "Identity",
                   new { UserId = result.Data.UserId, Token = result.Data.Token }, Request.Scheme, Request.Host.ToString());

            var messageData = new SendEmailMessage()
            {
                To = result.Data.Email,
                Subject = EmailConfirmSubject,
                HtmlContent = string.Format(EmailConfirmHtmlContent, confirmationLink)
            };

            //Create the message
            var message = new Message(messageData);

            //Save it in database
            await this.identityService.Save(null, message);

            try
            {
                //Publish the message
                await this.publisher.Publish(messageData);

                //Mark it as published
                message.MarkAsPublished();

                //And save the changes
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityController)}/{nameof(Register)} when {Publish}");
            }

            if (result.Data.ErrorsInImageUploading != null)
            {
                return this.Ok(result.Data.ErrorsInImageUploading);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
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
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Authorize]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditMyProfileRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            if (result.Data.Email == null && result.Data.Token == null && result.Data.UserId == null)
            {
                return this.NoContent();
            }

            var confirmationLink = Url.Action(nameof(ResetPassword), "Identity",
                   new { UserId = result.Data.UserId, Token = result.Data.Token }, Request.Scheme, Request.Host.ToString());

            await this.publisher.Publish(new SendEmailMessage()
            {
                To = result.Data.Email,
                Subject = ForgotPasswordSubject,
                HtmlContent = string.Format(ForgotPasswordHtmlContent, confirmationLink)
            });

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpPost]
        [Authorize]
        [Route(nameof(GenerateRefreshToken))]
        public async Task<ActionResult> GenerateRefreshToken()
        {
            var result = await this.jwtTokenGeneratorService.GenerateRefreshToken();
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
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
                return this.ProcessErrors(result);
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
                return this.ProcessErrors(result);
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
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpGet]
        [Route(nameof(GetUserInfo) + PathSeparator + Id)]
        public async Task<ActionResult> GetUserInfo(string id)
        {
            var result = await this.identityService.GetUserInfo(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpGet]
        [Route(nameof(GetUsersInfo))]
        public async Task<ActionResult> GetUsersInfo([FromBody] List<string> ids)
        {
            var result = await this.identityService.GetUsersInfo(ids);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }
    }
}
