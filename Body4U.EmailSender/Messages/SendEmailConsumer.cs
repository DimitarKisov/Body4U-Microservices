namespace Body4U.EmailSender.Messages
{
    using Body4U.Common.Messages;
    using Body4U.Common.Messages.Identity;
    using Body4U.EmailSender.Data;
    using Body4U.EmailSender.Services;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class SendEmailConsumer : IConsumer<SendEmailMessage>
    {
        private readonly IEmailService emailService;
        private readonly EmailSenderDbContext dbContext;

        public SendEmailConsumer(
            IEmailService emailService,
            EmailSenderDbContext dbContext)
        {
            this.emailService = emailService;
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<SendEmailMessage> context)
        {
            var messageType = context.Message.GetType();
            var propertyFilter = nameof(SendEmailMessage.Identifier);
            var identifier = context.Message.Identifier;

            //This check is made because the background worker may try to publish it before we mark the message as published from the destination where comes from.
            //Example: We register a user, creates a message and then publish it but we have some delay of 5 seconds for example. The background worker comes first, executes the code below and see that there is no message. Then he creates it, marks it as published and add it to the database. The 5 seconds delay we've had as a delay are over and then we try to publish the message AGAIN. But this time when the code below executes we will see that it's duplicated so it will not publish it again.

            //Check if there is any message from this type and identifier...
            var isDublicated = await this.dbContext
                .Messages
                .FromSqlRaw($"SELECT * FROM Messages WHERE Type = '{messageType.AssemblyQualifiedName}' AND JSON_VALUE(Data, '$.{propertyFilter}') = '{identifier}'")
                .AnyAsync();

            if (isDublicated)
            {
                return;
            }

            //...if there isn't we create it
            var message = new Message(context.Message);

            //mark it as published
            message.MarkAsPublished();

            //and add it to database and save the changes
            await this.dbContext.Messages.AddAsync(message);
            await this.dbContext.SaveChangesAsync();

            await Task.Run(() => this.emailService.SendEmailAsync(context.Message.To, context.Message.Subject, context.Message.HtmlContent));
        }
    }
}
