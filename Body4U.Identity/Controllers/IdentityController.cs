namespace Body4U.Identity.Controllers
{
    using Body4U.Common;
    using Body4U.Common.Controllers;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identityService;
        private readonly IEmailService emailService;

        public IdentityController(
            IIdentityService identityService,
            IEmailService emailService)
        {
            this.identityService = identityService;
            this.emailService = emailService;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<ActionResult<Result>> Register(RegisterUserRequestModel request)
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

            var confirmationLink = Url.Action("VerifyEmail", "Identity",
                   new { UserId = result.Data.UserId, Token = result.Data.Token }, Request.Scheme, Request.Host.ToString());

            var sendEmailResult = this.emailService.SendEmailAsync(result.Data.Email, EmailConfirmSubject, string.Format(EmailConfirmHtmlContent, confirmationLink));
            if (!sendEmailResult.Succeeded)
            {
                return this.BadRequest(sendEmailResult.Errors);
            }

            return sendEmailResult;
        }
    }
}
