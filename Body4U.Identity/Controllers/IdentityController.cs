namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Messages.Identity;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Services;
    using MassTransit;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identityService;
        private readonly ICurrentUserService currentUserService;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly IBus publisher;

        public IdentityController(
            IIdentityService identityService,
            ICurrentUserService currentUserService,
            IBus publisher,
            IJwtTokenGeneratorService jwtTokenGeneratorService)
        {
            this.identityService = identityService;
            this.currentUserService = currentUserService;
            this.publisher = publisher;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<ActionResult> Register(RegisterUserRequestModel request)
        {
            if (!ModelState.IsValid)
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

            return this.Ok();
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<ActionResult> Login(LoginUserRequestModel request)
        {
            if (!ModelState.IsValid)
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

        [HttpPut]
        [Authorize]
        [Route(nameof(ChangePassword))]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.identityService.ChangePassword(request, currentUserService.UserId);
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
            if (!ModelState.IsValid)
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

            if (!ModelState.IsValid)
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
        [Authorize]
        [Route(nameof(GenerateRefreshToken))]
        public async Task<ActionResult> GenerateRefreshToken()
        {
            var result = await this.jwtTokenGeneratorService.GenerateRefreshToken(currentUserService.UserId);
            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [Route(nameof(VerifyEmail))]
        public async Task<ActionResult> VerifyEmail([FromQuery] VerifyEmailRequestModel request)
        {
            if (!ModelState.IsValid)
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
    }
}
