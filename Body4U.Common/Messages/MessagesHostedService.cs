namespace Body4U.Common.Messages
{
    using Hangfire;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MessagesHostedService : IHostedService
    {
        private readonly IRecurringJobManager recurringJob;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IBus publisher;

        public MessagesHostedService(
            IRecurringJobManager recurringJob,
            IServiceScopeFactory serviceScopeFactory,
            IBus publisher)
        {
            this.recurringJob = recurringJob;
            this.serviceScopeFactory = serviceScopeFactory;
            this.publisher = publisher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<DbContext>();

                if (!dbContext.Database.CanConnect())
                {
                    dbContext.Database.Migrate();
                }

                this.recurringJob.AddOrUpdate(
                    nameof(MessagesHostedService),
                    () => this.ProcessPendingMessages(),
                    "*/10 * * * * *"); //Every 10 seconds

                return Task.CompletedTask;
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void ProcessPendingMessages()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<DbContext>();

                //Check for messages that are not published
                var messages = dbContext
                    .Set<Message>()
                    .Where(x => !x.Published)
                    .OrderBy(x => x.Id)
                    .ToList();

                foreach (var message in messages)
                {
                    //This is done because if I try to publish it with sending in json object and type of the message it gives me exception
                    var deserializedMessage = JsonConvert.DeserializeObject(message.Data, message.Type);

                    this.publisher
                        .Publish(deserializedMessage)
                        .GetAwaiter()
                        .GetResult();

                    message.MarkAsPublished();

                    dbContext.SaveChanges();
                }
            };
        }
    }
}
