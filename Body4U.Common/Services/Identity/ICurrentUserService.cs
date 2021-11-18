namespace Body4U.Common.Services.Identity
{
    public interface ICurrentUserService
    {
        string UserId { get; }

        bool IsAdmin { get; }
    }
}
