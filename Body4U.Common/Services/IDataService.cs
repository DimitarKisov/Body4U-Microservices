namespace Body4U.Common.Services
{
    using Body4U.Common.Messages;
    using System.Threading.Tasks;

    public interface IDataService<TEntity>
        where TEntity : class
    {
        Task MarkMessageAsPublished(string id);

        Task Save(TEntity entity = null, params Message[] messages);
    }
}
