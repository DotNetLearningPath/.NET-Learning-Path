using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.RiskFactors;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public sealed class RiskFactorConfigurationService(
    IRiskFactorRepository repository) : IRiskFactorConfigurationService
{
    public async Task<PagedResult<RiskFactorConfigurationDto>> ListRiskFactorAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var page = await repository.ListRiskFactorsAsync(pagination, cancellationToken);
        return new PagedResult<RiskFactorConfigurationDto>
        {
            Items = [.. page.Items.Select(MapToRiskFactorDto)],
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }

    public async Task<RiskFactorConfigurationDto> GetRiskFactorByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var configuration = await GetByIdAsync(id, cancellationToken);
        return MapToRiskFactorDto(configuration);
    }

    public async Task<RiskFactorConfigurationDto> CreateRiskFactorAsync(
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var configuration = new RiskFactorConfiguration(
            request.Level,
            NormalizeReference(request.Level, request.Reference),
            request.AdjustmentPercentage,
            request.IsActive);
        await repository.AddRiskFactorAsync(configuration, cancellationToken);
        return MapToRiskFactorDto(configuration);
    }

    public async Task<RiskFactorConfigurationDto> UpdateRiskFactorAsync(
        Guid id,
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var configuration = await GetByIdAsync(id, cancellationToken);

        configuration.Update(
            request.Level,
            NormalizeReference(request.Level, request.Reference),
            request.AdjustmentPercentage);
        if (request.IsActive)
        {
            configuration.Activate();
        }
        else
        {
            configuration.Deactivate();
        }

        await repository.UpdateRiskFactorAsync(configuration, cancellationToken);
        return MapToRiskFactorDto(configuration);
    }

    public async Task<RiskFactorConfigurationDto> ActivateRiskAsync(Guid id, CancellationToken cancellationToken)
    {
        var configuration = await GetByIdAsync(id, cancellationToken);
        configuration.Activate();
        await repository.UpdateRiskFactorAsync(configuration, cancellationToken);
        return MapToRiskFactorDto(configuration);
    }

    public async Task<RiskFactorConfigurationDto> DeactivateRiskFactorAsync(Guid id, CancellationToken cancellationToken)
    {
        var configuration = await GetByIdAsync(id, cancellationToken);
        configuration.Deactivate();
        await repository.UpdateRiskFactorAsync(configuration, cancellationToken);
        return MapToRiskFactorDto(configuration);
    }

    private async Task<RiskFactorConfiguration> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetRiskFactorByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Risk factor configuration was not found.");

    private static string NormalizeReference(
        Insurance.Domain.Enums.RiskFactorLevel level,
        string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        var normalized = reference.Trim();
        if (level == Insurance.Domain.Enums.RiskFactorLevel.BuildingType)
        {
            if (!Enum.TryParse<Insurance.Domain.Enums.BuildingType>(normalized, true, out var buildingType)
                || !Enum.IsDefined(buildingType))
            {
                throw new ArgumentException("Reference must be a valid building type.", nameof(reference));
            }
            return buildingType.ToString();
        }

        if (!Enum.IsDefined(level))
        {
            throw new ArgumentException("Risk factor level is not valid.", nameof(level));
        }
        if (!Guid.TryParse(normalized, out var geographyId))
        {
            throw new ArgumentException("Geography reference must be a valid identifier.", nameof(reference));
        }
        return geographyId.ToString();
    }

    private static RiskFactorConfigurationDto MapToRiskFactorDto(RiskFactorConfiguration configuration) => new()
    {
        Id = configuration.Id,
        Level = configuration.Level,
        Reference = configuration.Reference,
        AdjustmentPercentage = configuration.AdjustmentPercentage,
        IsActive = configuration.IsActive
    };
}
