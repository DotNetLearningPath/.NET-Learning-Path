namespace SupportAPI.Application.Abstractions
{
    public interface ICurrentUser
    {
        public Guid UserId { get; }
    }
}