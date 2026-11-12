using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IFeeConfigurationRepository : IGenericRepository<FeeConfiguration>
    {
        Task<IReadOnlyList<FeeConfiguration>> GetApplicableFeeConfigurationsAsync(DateOnly effectiveDate);
    }
}
