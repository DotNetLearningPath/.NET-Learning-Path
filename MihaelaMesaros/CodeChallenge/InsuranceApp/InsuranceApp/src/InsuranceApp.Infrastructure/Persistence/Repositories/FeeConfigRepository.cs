using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class FeeConfigRepository(InsuranceDbContext dbContext) : IFeeConfigRepository
{
    public async Task<IReadOnlyList<FeeConfig>> GetFeeConfigsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.FeeConfigs
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.FeeConfigId)
            .ToListAsync(cancellationToken);
    }

    public Task<FeeConfig?> GetFeeConfigByIdAsync(Guid feeConfigId, CancellationToken cancellationToken)
    {
        return dbContext.FeeConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.FeeConfigId == feeConfigId, cancellationToken);
    }

    public Task<FeeConfig?> GetFeeConfigForUpdateAsync(Guid feeConfigId, CancellationToken cancellationToken)
    {
        return dbContext.FeeConfigs.FirstOrDefaultAsync(x => x.FeeConfigId == feeConfigId, cancellationToken);
    }

    public async Task AddFeeConfigAsync(FeeConfig feeConfig, CancellationToken cancellationToken)
    {
        dbContext.FeeConfigs.Add(feeConfig);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveFeeConfigChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}