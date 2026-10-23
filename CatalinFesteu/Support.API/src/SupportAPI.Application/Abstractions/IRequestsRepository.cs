using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.Abstractions
{
    public interface IRequestsRepository
    {
        Task AddRequestAsync(Request request, CancellationToken cancellationToken);
        Task<Request?> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        // Task GetAllRequestsForUserAsync(Guid userId, CancellationToken cancellationToken); this one requires pagination
    }
}
