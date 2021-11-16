namespace Body4U.EmailSender.Messages
{
    using Body4U.Common.Messages.Identity;
    using Body4U.EmailSender.Services;
    using MassTransit;
    using System.Threading.Tasks;

    public class SendEmailConsumer : IConsumer<SendEmailMessage>
    {
        private readonly IEmailService emailService;

        public SendEmailConsumer(IEmailService emailService)
            => this.emailService = emailService;

        public async Task Consume(ConsumeContext<SendEmailMessage> context)
            => await Task.Run(() => this.emailService.SendEmailAsync(context.Message.To, context.Message.Subject, context.Message.HtmlContent));
    }
}
