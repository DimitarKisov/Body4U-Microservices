namespace Body4U.Common.Services
{
    using Body4U.Common.Messages;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class DataService<TEntity> : IDataService<TEntity>
        where TEntity : class
    {
        protected DataService(DbContext dbContext)
            => this.DbContext = dbContext;

        protected DbContext DbContext { get; }

        public async Task MarkMessageAsPublished(int id)
        {
            var message = await this.DbContext
                .FindAsync<Message>(new object[] { id });

            message.MarkAsPublished();

            await this.DbContext.SaveChangesAsync();
        }

        public async Task Save(TEntity entity = null, params Message[] messages)
        {
            if (entity != null)
            {
                this.DbContext.Update(entity);
            }

            foreach (var message in messages)
            {
                await this.DbContext.AddAsync(message);
            }

            await this.DbContext.SaveChangesAsync();
        }
    }
}
