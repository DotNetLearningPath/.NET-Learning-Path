using Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Abstractions;

public interface IFeeConfigurationRepository
{
    Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default);

    Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken = default);

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
        CancellationToken cancellationToken = default);
}
