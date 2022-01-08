namespace Body4U.Article.Messages
{
    using Body4U.Article.Data;
    using Body4U.Common.Messages.Identity;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Trainer;

    public class EdiTrainerNamesConsumer : IConsumer<EditUserNamesMessage>
    {
        private readonly ArticleDbContext dbContext;

        public EdiTrainerNamesConsumer(ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<EditUserNamesMessage> context)
        {
            try
            {
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
