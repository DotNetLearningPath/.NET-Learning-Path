using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Fees;
namespace Insurance.Application.Abstractions;

public interface IFeeConfigurationService
{
    Task<PagedResult<FeeConfigurationDto>> ListAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task<FeeConfigurationDto> CreateAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken);

    Task<FeeConfigurationDto> UpdateAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken);

    Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken);
}
