using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Application.DTOs.FeeConfig;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IFeeConfigService
{
    Task<Result<IReadOnlyList<FeeConfigDto>>> GetFeeConfigsAsync(CancellationToken cancellationToken);

    Task<Result<FeeConfigDto>> GetFeeConfigByIdAsync(Guid feeConfigId, CancellationToken cancellationToken);

    Task<Result<FeeConfigDto>> CreateFeeConfigAsync(CreateFeeConfigDto dto, CancellationToken cancellationToken);

    Task<Result<FeeConfigDto>> UpdateFeeConfigAsync(Guid feeConfigId, UpdateFeeConfigDto dto, CancellationToken cancellationToken);
}
