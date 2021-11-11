namespace Body4U.Identity.Services
{
    using Body4U.Common;

    public interface IEmailService
    {
        Result SendEmailAsync(string to, string subject, string htmlContent);
    }
}
