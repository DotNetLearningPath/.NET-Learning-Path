using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IBuildingService
    {
        Task<Building> CreateAsync(Building building, CancellationToken cancellationToken);

        Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Building>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken);

        Task<Building?> UpdateAsync(Building building, CancellationToken cancellationToken);
    }
}
