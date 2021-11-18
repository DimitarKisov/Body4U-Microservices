namespace Body4U.EmailSender.Services
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Mail;

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
            => this.configuration = configuration;

        public void SendEmailAsync(string to, string subject, string htmlContent)
        {
            try
            {
                var host = this.configuration.GetSection("MailSettings")["Host"];
                var port = int.Parse(this.configuration.GetSection("MailSettings")["Port"]);
                var username = this.configuration.GetSection("MailSettings").GetSection("Credentials")["UserName"];
                var password = this.configuration.GetSection("MailSettings").GetSection("Credentials")["Password"];

                var client = new SmtpClient()
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password)
                };

                var displayName = this.configuration.GetSection("MailSettings").GetSection("Credentials")["Displayname"];

                var fromEmail = new MailAddress(username, displayName);
                var toEmail = new MailAddress(to);
                var message = new MailMessage
                {
                    From = fromEmail,
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                client.SendCompleted += ClientSendComplete;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(EmailService)}.{nameof(this.SendEmailAsync)}", ex);
            }
        }

        private void ClientSendComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Log.Error($"{nameof(EmailService)}.{nameof(this.ClientSendComplete)}", e.Error);
            }
        }
    }
}
