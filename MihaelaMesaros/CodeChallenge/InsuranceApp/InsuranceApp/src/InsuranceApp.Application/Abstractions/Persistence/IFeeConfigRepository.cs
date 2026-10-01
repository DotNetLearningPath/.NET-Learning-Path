using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IFeeConfigRepository
{
    Task<IReadOnlyList<FeeConfig>> GetFeeConfigsAsync(CancellationToken cancellationToken);

    Task<FeeConfig?> GetFeeConfigByIdAsync(Guid feeConfigId, CancellationToken cancellationToken);

    Task<FeeConfig?> GetFeeConfigForUpdateAsync(Guid feeConfigId, CancellationToken cancellationToken);

    Task AddFeeConfigAsync(FeeConfig feeConfig, CancellationToken cancellationToken);

    Task SaveFeeConfigChangesAsync(CancellationToken cancellationToken);
}
