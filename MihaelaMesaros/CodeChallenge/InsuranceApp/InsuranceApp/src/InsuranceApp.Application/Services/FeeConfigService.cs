using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class FeeConfigService(IFeeConfigRepository feeConfigRepository, ILogger<FeeConfigService> logger) : IFeeConfigService
{
    public async Task<Result<IReadOnlyList<FeeConfigDto>>> GetFeeConfigsAsync(CancellationToken cancellationToken)
    {
        var fees = await feeConfigRepository.GetFeeConfigsAsync(cancellationToken);

        var feeDtos = fees.Select(MapFeeConfigToDto).ToList();

        return Result<IReadOnlyList<FeeConfigDto>>.Success(feeDtos);
    }

    public async Task<Result<FeeConfigDto>> GetFeeConfigByIdAsync(Guid feeConfigId, CancellationToken cancellationToken)
    {
        if (feeConfigId == Guid.Empty)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.InvalidFeeConfigId);
        }

        var feeConfig = await feeConfigRepository.GetFeeConfigByIdAsync(feeConfigId, cancellationToken);

        if (feeConfig is null)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.NotFound(feeConfigId));
        }

        return Result<FeeConfigDto>.Success(MapFeeConfigToDto(feeConfig));
    }

    public async Task<Result<FeeConfigDto>> CreateFeeConfigAsync(CreateFeeConfigDto createFeeConfigDto, CancellationToken cancellationToken)
    {
        createFeeConfigDto = createFeeConfigDto with
        {
            Name = createFeeConfigDto.Name?.Trim()!
        };

        var validationFeeConfig = ValidateFeeConfig(
            createFeeConfigDto.Name, 
            createFeeConfigDto.FeeType, 
            createFeeConfigDto.Percentage, 
            createFeeConfigDto.EffectiveFrom, 
            createFeeConfigDto.EffectiveTo
        );

        if (validationFeeConfig is not null)
        {
            return Result<FeeConfigDto>.Failure(validationFeeConfig);
        }

        var fee = new FeeConfig
        {
            Name = createFeeConfigDto.Name,
            FeeType = createFeeConfigDto.FeeType,
            Percentage = createFeeConfigDto.Percentage,
            EffectiveFrom = createFeeConfigDto.EffectiveFrom,
            EffectiveTo = createFeeConfigDto.EffectiveTo,
            IsActive = createFeeConfigDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await feeConfigRepository.AddFeeConfigAsync(fee, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Fee configuration {FeeConfigId} created.", fee.FeeConfigId);
        }

        return Result<FeeConfigDto>.Success(MapFeeConfigToDto(fee));
    }

    public async Task<Result<FeeConfigDto>> UpdateFeeConfigAsync(Guid feeConfigId, UpdateFeeConfigDto updateFeeConfigDto, CancellationToken cancellationToken)
    {
        if (feeConfigId == Guid.Empty)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.InvalidFeeConfigId);
        }

        updateFeeConfigDto = updateFeeConfigDto with
        {
            Name = updateFeeConfigDto.Name?.Trim()!
        };

        var validationFeeConfig = ValidateFeeConfig(
            updateFeeConfigDto.Name, 
            updateFeeConfigDto.FeeType, 
            updateFeeConfigDto.Percentage, 
            updateFeeConfigDto.EffectiveFrom, 
            updateFeeConfigDto.EffectiveTo
        );

        if (validationFeeConfig is not null)
        {
            return Result<FeeConfigDto>.Failure(validationFeeConfig);
        }

        var fee = await feeConfigRepository.GetFeeConfigForUpdateAsync(feeConfigId, cancellationToken);

        if (fee is null)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.NotFound(feeConfigId));
        }

        fee.Name = updateFeeConfigDto.Name;
        fee.FeeType = updateFeeConfigDto.FeeType;
        fee.Percentage = updateFeeConfigDto.Percentage;
        fee.EffectiveFrom = updateFeeConfigDto.EffectiveFrom;
        fee.EffectiveTo = updateFeeConfigDto.EffectiveTo;
        fee.IsActive = updateFeeConfigDto.IsActive;
        fee.ModifiedAt = DateTime.UtcNow;

        await feeConfigRepository.SaveFeeConfigChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Fee configuration {FeeConfigId} updated.", feeConfigId);
        }

        return Result<FeeConfigDto>.Success(MapFeeConfigToDto(fee));
    }

    private static Error? ValidateFeeConfig(string? name, FeeType type, decimal percentage, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FeeConfigErrors.NameRequired;
        }

        if (name.Length is < FeeConfigConstraints.NameMinLength or > FeeConfigConstraints.NameMaxLength)
        {
            return FeeConfigErrors.InvalidNameLength;
        }

        if (!Enum.IsDefined(type))
        {
            return FeeConfigErrors.InvalidType;
        }

        if (!DecimalValidation.HasValidScale(percentage, FeeConfigConstraints.PercentageScale))
        {
            return FeeConfigErrors.InvalidPercentageScale;
        }

        if (percentage is < FeeConfigConstraints.MinPercentage or > FeeConfigConstraints.MaxPercentage)
        {
            return FeeConfigErrors.InvalidPercentage;
        }

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            return FeeConfigErrors.InvalidEffectivePeriod;
        }

        return null;
    }

    private static FeeConfigDto MapFeeConfigToDto(FeeConfig fee)
    {
        return new FeeConfigDto(
            fee.FeeConfigId,
            fee.Name,
            fee.FeeType,
            fee.Percentage,
            fee.EffectiveFrom,
            fee.EffectiveTo,
            fee.IsActive
        );
    }
}
