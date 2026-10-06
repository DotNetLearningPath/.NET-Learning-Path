using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Fees;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;
namespace Insurance.Application.Services;

public sealed class FeeConfigurationService(
    IFeeConfigurationRepository repository) : IFeeConfigurationService
{
    public async Task<PagedResult<FeeConfigurationDto>> ListAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var page = await repository.ListFeeConfigurationsAsync(pagination, cancellationToken);
        return new PagedResult<FeeConfigurationDto>
        {
            Items = [.. page.Items.Select(MapToFeeConfigurationDto)],
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
    public async Task<FeeConfigurationDto> CreateAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fee = new FeeConfiguration(
            request.Name,
            request.Type,
            request.Percentage,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.IsActive);
        await repository.AddFeeConfigurationAsync(fee, cancellationToken);
        return MapToFeeConfigurationDto(fee);
    }
    public async Task<FeeConfigurationDto> UpdateAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fee = await repository.GetFeeConfigurationByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Fee configuration was not found.");

        fee.Update(
            request.Name,
            request.Type,
            request.Percentage,
            request.EffectiveFrom,
            request.EffectiveTo);

        if (request.IsActive)
        {
            fee.Activate();
        }
        else
        {
            fee.Deactivate();
        }

        await repository.UpdateFeeConfigurationAsync(fee, cancellationToken);
        return MapToFeeConfigurationDto(fee);
    }
    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fee = await repository.GetFeeConfigurationByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Fee configuration was not found.");

        await repository.DeactivateFeeConfigurationAsync(fee, cancellationToken);
    }

    private static FeeConfigurationDto MapToFeeConfigurationDto(FeeConfiguration fee)
    {
        return new FeeConfigurationDto
        {
            Id = fee.Id,
            Name = fee.Name,
            Type = fee.Type,
            Percentage = fee.Percentage,
            EffectiveFrom = fee.EffectiveFrom,
            EffectiveTo = fee.EffectiveTo,
            IsActive = fee.IsActive
        };
    }
}
