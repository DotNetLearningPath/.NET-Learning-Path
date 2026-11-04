using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.Abstractions
{
    public interface IUserRequestsRepository
    {
        Task AddRequestAsync(Request request, CancellationToken cancellationToken);
        Task<Request?> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
