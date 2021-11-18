namespace Body4U.EmailSender.Services
{
    using Body4U.Common;

    public interface IEmailService
    {
        void SendEmailAsync(string to, string subject, string htmlContent);
    }
}
