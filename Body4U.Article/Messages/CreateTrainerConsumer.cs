﻿namespace Body4U.Article.Messages
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Common.Messages;
    using Body4U.Common.Messages.Article;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
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
                
                var messageType = context.Message.GetType();
                var propertyFilter = nameof(CreateTrainerMessage.Identifier);
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

                var trainer = new Trainer()
                {
                    ApplicationUserId = context.Message.ApplicationUserId,
                    CreatedOn = context.Message.CreatedOn,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.Lastname
                };

                this.dbContext.Trainers.Add(trainer);

                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CreateTrainerConsumer)}.{nameof(Consume)}");
            }
        }
    }
}
