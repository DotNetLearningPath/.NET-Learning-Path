using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class FeeConfigurationRepository(InsuranceDbContext context)
        : GenericRepository<FeeConfiguration>(context), IFeeConfigurationRepository
    {
        public async Task<IReadOnlyList<FeeConfiguration>> GetApplicableFeeConfigurationsAsync(DateOnly effectiveDate) =>
            await _context.FeeConfigurations
                .AsNoTracking()
                .Where(configuration => configuration.IsActive &&
                    configuration.EffectiveFrom <= effectiveDate &&
                    (!configuration.EffectiveTo.HasValue || configuration.EffectiveTo.Value >= effectiveDate))
                .OrderBy(configuration => configuration.Id)
                .ToListAsync();
    }
}
