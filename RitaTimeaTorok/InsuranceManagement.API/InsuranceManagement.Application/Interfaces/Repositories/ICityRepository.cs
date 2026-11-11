using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface ICityRepository : IGenericRepository<City>
    {
        Task<bool> ExistsByCountyIdAsync(Guid countyId);
    }
}
