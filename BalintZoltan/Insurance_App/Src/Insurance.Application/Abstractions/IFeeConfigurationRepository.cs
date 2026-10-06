using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Abstractions;

public interface IFeeConfigurationRepository
{
    Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken);

    Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken);

    Task<List<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
        DateTime effectiveAt,
        CancellationToken cancellationToken);

    Task<FeeConfiguration?> GetFeeConfigurationByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task UpdateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken);

    Task DeactivateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken);

    Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);
}
