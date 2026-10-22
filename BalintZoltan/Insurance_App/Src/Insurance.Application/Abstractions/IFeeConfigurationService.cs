using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Fees;
namespace Insurance.Application.Abstractions;

public interface IFeeConfigurationService
{
    Task<PagedResult<FeeConfigurationDto>> ListFeeAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task<FeeConfigurationDto> CreateFeeAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken);

    Task<FeeConfigurationDto> UpdateFeeAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken);

    Task DeactivateFeeAsync(
        Guid id,
        CancellationToken cancellationToken);
}
