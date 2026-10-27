using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IBuildingRepository
    {
        Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        
        Task<IReadOnlyList<Building>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken);

        Task AddAsync(Building building, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
