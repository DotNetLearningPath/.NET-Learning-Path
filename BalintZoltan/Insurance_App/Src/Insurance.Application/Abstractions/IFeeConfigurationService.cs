using Application.DTO.Common;
using Insurance.Application.DTO.Fees;
namespace Insurance.Application.Abstractions;

public interface IFeeConfigurationService
{
    Task<PagedResult<FeeConfigurationDto>> ListAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<FeeConfigurationDto> CreateAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task<FeeConfigurationDto> UpdateAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
