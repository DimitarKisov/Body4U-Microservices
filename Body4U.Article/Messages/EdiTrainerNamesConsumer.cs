namespace Body4U.Article.Messages
{
    using Body4U.Article.Data;
    using Body4U.Common.Messages;
    using Body4U.Common.Messages.Article;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Trainer;

    public class EdiTrainerNamesConsumer : IConsumer<EditTrainerNamesMessage>
    {
        private readonly ArticleDbContext dbContext;

        public EdiTrainerNamesConsumer(ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<EditTrainerNamesMessage> context)
        {
            try
            {
                var messageType = context.Message.GetType();
                var propertyFilter = nameof(EditTrainerNamesMessage.Identifier);
                var identifier = context.Message.Identifier;

                var isDublicated = await this.dbContext
                    .Messages
                    .FromSql($"SELECT * FROM Messages WHERE Type = '{messageType.AssemblyQualifiedName}' AND JSON_VALUE(Data, '$.{propertyFilter}') = '{identifier}'")
                    .AnyAsync();

                if (isDublicated)
                {
                    return;
                }

                var message = new Message(context.Message);

                message.MarkAsPublished();

                this.dbContext.Messages.Add(message);

                var trainer = await this.dbContext
                .Trainers
                .FirstOrDefaultAsync(x => x.ApplicationUserId == context.Message.ApplicationUserId);

                if (trainer != null)
                {
                    trainer.FirstName = context.Message.FirstName;
                    trainer.LastName = context.Message.LastName;
                    trainer.ModifiedBy = context.Message.ModifiedBy;
                    trainer.ModifiedOn = DateTime.Now;

                    await this.dbContext.SaveChangesAsync();
                }
                else
                {
                    Log.Error(nameof(EdiTrainerNamesConsumer) + "-" + TrainerNotFound + "-" + context.Message.ApplicationUserId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(EdiTrainerNamesConsumer)}.{nameof(Consume)}");
            }
        }
    }
}
