namespace Body4U.Article.Gateway.Services.Identity
{
    using Refit;
    using System.Threading.Tasks;

    public interface ITrainerService
    {
        [Get("/Trainer/CanTrainerWrite")]
        Task<bool> CanTrainerWrite(int trainerId);
    }
}
