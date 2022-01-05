﻿namespace Body4U.Article.Messages
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Common.Messages.Article;
    using MassTransit;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    public class CreateTrainerConsumer : IConsumer<CreateTrainerMessage>
    {
        private readonly ArticleDbContext dbContext;

        public CreateTrainerConsumer(ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<CreateTrainerMessage> context)
        {
            try
            {
                var trainer = new Trainer()
                {
                    ApplicationUserId = context.Message.ApplicationUserId,
                    CreatedOn = context.Message.CreatedOn,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.Lastname
                };

                await this.dbContext
                .Trainers
                .AddAsync(trainer);

                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CreateTrainerConsumer)}.{nameof(Consume)}");
            }
        }
    }
}
