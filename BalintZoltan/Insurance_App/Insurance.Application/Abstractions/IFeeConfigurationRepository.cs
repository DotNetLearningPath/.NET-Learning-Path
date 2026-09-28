using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

public interface IFeeConfigurationRepository
{
    Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default);

    Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
