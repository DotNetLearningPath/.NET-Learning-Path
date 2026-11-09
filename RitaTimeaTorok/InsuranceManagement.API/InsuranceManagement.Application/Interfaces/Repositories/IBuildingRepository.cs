using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        Task<IReadOnlyList<Building>> GetByClientIdAsync(Guid clientId);
        Task<bool> ExistsByClientIdAsync(Guid clientId);
        Task<bool> ExistsByCityIdAsync(Guid cityId);
    }
}
