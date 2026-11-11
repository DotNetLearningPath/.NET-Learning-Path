using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface ICountyRepository : IGenericRepository<County>
    {
        Task<bool> ExistsByCountryIdAsync(Guid countryId);
    }
}
