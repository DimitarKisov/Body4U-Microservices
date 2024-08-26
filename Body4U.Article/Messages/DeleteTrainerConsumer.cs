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

    public class DeleteTrainerConsumer : IConsumer<DeleteTrainerMessage>
    {
        private readonly ArticleDbContext dbContext;

        public DeleteTrainerConsumer(ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<DeleteTrainerMessage> context)
        {
            try
            {
                var messageType = context.Message.GetType();
                var propertyFilter = nameof(DeleteTrainerMessage.Identifier);
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

                await this.dbContext.Messages.AddAsync(message);

                var trainer = await this.dbContext
                    .Trainers
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == context.Message.ApplicationUserId);

                if (trainer != null)
                {
                    this.dbContext.Trainers.Remove(trainer);
                    await this.dbContext.SaveChangesAsync();
                }
                else
                {
                    Log.Error(nameof(DeleteTrainerConsumer) + "-" + TrainerNotFound + "-" + context.Message.ApplicationUserId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(DeleteTrainerConsumer)}.{nameof(Consume)}");
            }
        }
    }
}
