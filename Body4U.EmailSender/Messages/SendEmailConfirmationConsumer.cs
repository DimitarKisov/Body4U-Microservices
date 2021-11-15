namespace Body4U.EmailSender.Messages
{
    using Body4U.Common.Messages.Identity;
    using Body4U.EmailSender.Services;
    using MassTransit;
    using System.Threading.Tasks;

    public class SendEmailConfirmationConsumer : IConsumer<SendEmailConfirmationMessage>
    {
        private readonly IEmailService emailService;

        public SendEmailConfirmationConsumer(IEmailService emailService)
            => this.emailService = emailService;

        public async Task Consume(ConsumeContext<SendEmailConfirmationMessage> context)
            => await Task.Run(() => this.emailService.SendEmailAsync(context.Message.To, context.Message.Subject, context.Message.HtmlContent));
    }
}
