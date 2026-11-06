using SupportAPI.Application.Abstractions;

namespace SupportAPI.Api
{
    public class HttpCurrentUser : ICurrentUser
    {
        public Guid UserId { get; private set; }

        public void SetUserId(Guid userId)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
            UserId = userId;
        }
    }
}
