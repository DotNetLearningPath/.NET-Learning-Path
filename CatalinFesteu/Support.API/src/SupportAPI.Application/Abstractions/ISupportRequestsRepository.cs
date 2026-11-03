using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.Abstractions
{
    public interface ISupportRequestsRepository
    {
        Task AddRequestAsync(SupportRequest request, CancellationToken cancellationToken);
        Task<SupportRequest?> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        // Task GetAllRequestsForUserAsync(Guid userId, CancellationToken cancellationToken); this one requires pagination
    }
}
