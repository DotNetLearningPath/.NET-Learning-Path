namespace SupportAPI.Application.Abstractions
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        void SetUserId(Guid userId);

    }
}
