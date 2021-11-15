namespace Body4U.Identity.Controllers
{
    using Body4U.Common.Controllers;
    using Body4U.Common.Messages.Identity;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Services;
    using MassTransit;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identityService;
        private readonly IBus publisher;

        public IdentityController(
            IIdentityService identityService,
            IBus publisher)
        {
            this.identityService = identityService;
            this.publisher = publisher;
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

            await this.publisher.Publish(new SendEmailConfirmationMessage()
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
