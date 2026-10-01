using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.DTO.Common;

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

    Task<IReadOnlyCollection<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
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

    Task<IReadOnlyCollection<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
        DateTime effectiveAt,
        CancellationToken cancellationToken = default);

    Task<FeeConfiguration?> GetFeeConfigurationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default);

    Task DeactivateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);
}
