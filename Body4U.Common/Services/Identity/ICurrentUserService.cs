namespace Body4U.Common.Services.Identity
{
    public interface ICurrentUserService
    {
        string UserId { get; }

        int? TrainerId { get; }

        bool IsAdministrator { get; }
    }
}
