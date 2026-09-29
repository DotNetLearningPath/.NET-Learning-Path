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

    public async Task<Result<FeeConfigDto>> CreateFeeConfigAsync(CreateFeeConfigDto dto, CancellationToken cancellationToken)
    {
        dto = dto with
        {
            Name = dto.Name?.Trim()!
        };

        var validationError = ValidateFeeConfig(dto.Name, dto.FeeType, dto.Percentage, dto.EffectiveFrom, dto.EffectiveTo);

        if (validationError is not null)
        {
            return Result<FeeConfigDto>.Failure(validationError);
        }

        var fee = new FeeConfig
        {
            Name = dto.Name,
            FeeType = dto.FeeType,
            Percentage = dto.Percentage,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await feeConfigRepository.AddFeeConfigAsync(fee, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Fee configuration {FeeConfigId} created.", fee.FeeConfigId);
        }

        return Result<FeeConfigDto>.Success(MapFeeConfigToDto(fee));
    }

    public async Task<Result<FeeConfigDto>> UpdateFeeConfigAsync(Guid feeConfigId, UpdateFeeConfigDto dto, CancellationToken cancellationToken)
    {
        if (feeConfigId == Guid.Empty)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.InvalidFeeConfigId);
        }

        dto = dto with
        {
            Name = dto.Name?.Trim()!
        };

        var validationError = ValidateFeeConfig(dto.Name, dto.FeeType, dto.Percentage, dto.EffectiveFrom, dto.EffectiveTo);

        if (validationError is not null)
        {
            return Result<FeeConfigDto>.Failure(validationError);
        }

        var fee = await feeConfigRepository.GetFeeConfigForUpdateAsync(feeConfigId, cancellationToken);

        if (fee is null)
        {
            return Result<FeeConfigDto>.Failure(FeeConfigErrors.NotFound(feeConfigId));
        }

        fee.Name = dto.Name;
        fee.FeeType = dto.FeeType;
        fee.Percentage = dto.Percentage;
        fee.EffectiveFrom = dto.EffectiveFrom;
        fee.EffectiveTo = dto.EffectiveTo;
        fee.IsActive = dto.IsActive;
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
