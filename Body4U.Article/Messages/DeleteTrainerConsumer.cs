namespace Body4U.Article.Messages
{
    using Body4U.Article.Data;
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
                    Log.Error(TrainerNotFound + "-" + context.Message.ApplicationUserId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(DeleteTrainerConsumer)}.{nameof(Consume)}");
            }
        }
    }
}
