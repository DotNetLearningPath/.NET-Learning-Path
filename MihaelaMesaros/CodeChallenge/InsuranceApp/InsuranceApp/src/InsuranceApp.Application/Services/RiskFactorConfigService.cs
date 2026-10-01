using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class RiskFactorConfigService(IRiskFactorConfigRepository riskFactorConfigRepository, ILogger<RiskFactorConfigService> logger) : IRiskFactorConfigService
{
    public async Task<Result<IReadOnlyList<RiskFactorConfigDto>>> GetRiskFactorConfigsAsync(CancellationToken cancellationToken)
    {
        var riskFactorConfigs = await riskFactorConfigRepository.GetRiskFactorConfigsAsync(cancellationToken);

        var riskFactorConfigDtos = riskFactorConfigs.Select(MapRiskFactorConfigToDto).ToList();

        return Result<IReadOnlyList<RiskFactorConfigDto>>.Success(riskFactorConfigDtos);
    }

    public async Task<Result<RiskFactorConfigDto>> GetRiskFactorConfigByIdAsync(Guid riskFactorConfigId, CancellationToken cancellationToken)
    {
        if (riskFactorConfigId == Guid.Empty)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.InvalidRiskFactorConfigId);
        }

        var riskFactorConfig = await riskFactorConfigRepository.GetRiskFactorConfigByIdAsync(riskFactorConfigId, cancellationToken);

        if (riskFactorConfig is null)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.NotFound(riskFactorConfigId));
        }

        return Result<RiskFactorConfigDto>.Success(MapRiskFactorConfigToDto(riskFactorConfig));
    }

    public async Task<Result<RiskFactorConfigDto>> CreateRiskFactorConfigAsync(CreateRiskFactorConfigDto createRiskFactorConfigDto, CancellationToken cancellationToken)
    {
        var validationRiskFactorConfig = ValidateRiskFactorConfig(
            createRiskFactorConfigDto.Level,
            createRiskFactorConfigDto.ReferenceId,
            createRiskFactorConfigDto.AdjustmentPercentage);

        if (validationRiskFactorConfig is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(validationRiskFactorConfig);
        }

        var validationReference = await ValidateReferenceAsync(
            createRiskFactorConfigDto.Level,
            createRiskFactorConfigDto.ReferenceId,
            cancellationToken);

        if (validationReference is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(validationReference);
        }

        var validationDuplicateRiskConfig = await ValidateDuplicateRiskConfigAsync(
            createRiskFactorConfigDto.Level,
            createRiskFactorConfigDto.ReferenceId,
            null,
            cancellationToken);

        if (validationDuplicateRiskConfig is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(validationDuplicateRiskConfig);
        }

        var riskFactorConfig = new RiskFactorConfig
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = createRiskFactorConfigDto.Level,
            ReferenceId = createRiskFactorConfigDto.ReferenceId,
            AdjustmentPercentage = createRiskFactorConfigDto.AdjustmentPercentage,
            IsActive = createRiskFactorConfigDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        
        try
        {
            await riskFactorConfigRepository.AddRiskFactorConfigAsync(riskFactorConfig, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.AlreadyExists);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Risk factor configuration {RiskFactorConfigId} created.", riskFactorConfig.RiskFactorConfigId);
        }

        return Result<RiskFactorConfigDto>.Success(MapRiskFactorConfigToDto(riskFactorConfig));
    }

    public async Task<Result<RiskFactorConfigDto>> UpdateRiskFactorConfigAsync(Guid riskFactorConfigId, UpdateRiskFactorConfigDto updateRiskFactorConfigDto, CancellationToken cancellationToken)
    {
        if (riskFactorConfigId == Guid.Empty)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.InvalidRiskFactorConfigId);
        }

        var riskFactorConfig = await riskFactorConfigRepository.GetRiskFactorConfigForUpdateAsync(riskFactorConfigId, cancellationToken);

        if (riskFactorConfig is null)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.NotFound(riskFactorConfigId));
        }

        var validationRiskFactorConfig = ValidateRiskFactorConfig(
            updateRiskFactorConfigDto.Level,
            updateRiskFactorConfigDto.ReferenceId,
            updateRiskFactorConfigDto.AdjustmentPercentage);

        if (validationRiskFactorConfig is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(validationRiskFactorConfig);
        }

        var referenceError = await ValidateReferenceAsync(
            updateRiskFactorConfigDto.Level,
            updateRiskFactorConfigDto.ReferenceId,
            cancellationToken);

        if (referenceError is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(referenceError);
        }

        // 5. Check duplicates, excluding the current configuration.
        var validationDuplicateRiskConfig = await ValidateDuplicateRiskConfigAsync(
            updateRiskFactorConfigDto.Level,
            updateRiskFactorConfigDto.ReferenceId,
            riskFactorConfigId,
            cancellationToken);

        if (validationDuplicateRiskConfig is not null)
        {
            return Result<RiskFactorConfigDto>.Failure(validationDuplicateRiskConfig);
        }

        riskFactorConfig.Level = updateRiskFactorConfigDto.Level;
        riskFactorConfig.ReferenceId = updateRiskFactorConfigDto.ReferenceId;
        riskFactorConfig.AdjustmentPercentage = updateRiskFactorConfigDto.AdjustmentPercentage;
        riskFactorConfig.IsActive = updateRiskFactorConfigDto.IsActive;
        riskFactorConfig.ModifiedAt = DateTime.UtcNow;

        
        try
        {
            await riskFactorConfigRepository.SaveRiskFactorConfigChangesAsync(cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<RiskFactorConfigDto>.Failure(RiskFactorConfigErrors.AlreadyExists);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Risk factor configuration {RiskFactorConfigId} updated.", riskFactorConfig.RiskFactorConfigId);
        }

        return Result<RiskFactorConfigDto>.Success(MapRiskFactorConfigToDto(riskFactorConfig));
    }

    private static Error? ValidateRiskFactorConfig(RiskFactorLevel level, Guid referenceId, decimal adjustmentPercentage)
    {
        if (!Enum.IsDefined(level))
        {
            return RiskFactorConfigErrors.InvalidLevel;
        }

        if (referenceId == Guid.Empty)
        {
            return RiskFactorConfigErrors.InvalidReferenceId;
        }

        if (!DecimalValidation.HasValidScale(adjustmentPercentage, RiskFactorConfigConstraints.AdjustmentPercentageScale))
        {
            return RiskFactorConfigErrors.InvalidAdjustmentPercentageScale;
        }

        if (adjustmentPercentage < RiskFactorConfigConstraints.MinAdjustmentPercentage 
            || adjustmentPercentage > RiskFactorConfigConstraints.MaxAdjustmentPercentage)
        {
            return RiskFactorConfigErrors.InvalidAdjustmentPercentage;
        }

        return null;
    }

    private async Task<Error?> ValidateReferenceAsync(RiskFactorLevel level, Guid referenceId, CancellationToken cancellationToken)
    {
        var referenceExists = await riskFactorConfigRepository.ReferenceExistsAsync(level, referenceId, cancellationToken);

        return referenceExists ? null : RiskFactorConfigErrors.ReferenceNotFound;
    }

    private async Task<Error?> ValidateDuplicateRiskConfigAsync(
        RiskFactorLevel level,
        Guid referenceId,
        Guid? excludeRiskFactorConfigId,
        CancellationToken cancellationToken)
    {
        var alreadyExists =
            await riskFactorConfigRepository.RiskFactorConfigExistsAsync(
                level,
                referenceId,
                excludeRiskFactorConfigId,
                cancellationToken);

        return alreadyExists ? RiskFactorConfigErrors.AlreadyExists : null;
    }

    private static RiskFactorConfigDto MapRiskFactorConfigToDto(RiskFactorConfig riskFactorConfig)
    {
        return new RiskFactorConfigDto(
            riskFactorConfig.RiskFactorConfigId,
            riskFactorConfig.Level,
            riskFactorConfig.ReferenceId,
            riskFactorConfig.AdjustmentPercentage,
            riskFactorConfig.IsActive);
    }
}