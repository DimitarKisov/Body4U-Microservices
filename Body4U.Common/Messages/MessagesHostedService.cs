namespace Body4U.Common.Messages
{
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class MessagesHostedService(
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        IBus publisher) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ProcessPendingMessages(serviceScopeFactory, publisher);
                var milliseconds = configuration.GetValue<int>("BackgroundWorkerMilliseconds");
                await Task.Delay(milliseconds, stoppingToken);
            }
        }

        private void ProcessPendingMessages(IServiceScopeFactory serviceScopeFactory, IBus publisher)
        {
            using (var scope = serviceScopeFactory.CreateScope())
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

                    publisher
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
