using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Broker;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace InsuranceApp.Application.Services;

public sealed class BrokerService(IBrokerRepository brokerRepository, ILogger<BrokerService> logger) : IBrokerService
{
    public async Task<Result<IReadOnlyList<BrokerDto>>> GetBrokersAsync(CancellationToken cancellationToken)
    {
        var brokers = await brokerRepository.GetBrokersAsync(cancellationToken);

        var brokerDtos = brokers.Select(MapBrokerToDto).ToList();

        return Result<IReadOnlyList<BrokerDto>>.Success(brokerDtos);
    }

    public async Task<Result<BrokerDto>> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        if (brokerId == Guid.Empty)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.InvalidBrokerId);
        }

        var broker = await brokerRepository.GetBrokerByIdAsync(brokerId, cancellationToken);

        if (broker is null)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.NotFound(brokerId));
        }

        return Result<BrokerDto>.Success(MapBrokerToDto(broker));
    }

    public async Task<Result<BrokerDto>> CreateBrokerAsync(CreateBrokerDto createBrokerDto, CancellationToken cancellationToken)
    {
        createBrokerDto = createBrokerDto with
        {
            BrokerCode = createBrokerDto.BrokerCode?.Trim().ToUpperInvariant()!,
            Name = createBrokerDto.Name?.Trim()!,
            Email = createBrokerDto.Email?.Trim(),
            Phone = createBrokerDto.Phone?.Trim()
        };

        var validationBrokerCode = ValidateBrokerCode(createBrokerDto.BrokerCode);

        if (validationBrokerCode is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerCode);
        }

        var validationBrokerName = ValidateBrokerName(createBrokerDto.Name);

        if (validationBrokerName is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerName);
        }

        var validationBrokerContactInfo = ValidateBrokerContactInfo(createBrokerDto.Email, createBrokerDto.Phone);

        if (validationBrokerContactInfo is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerContactInfo);
        }

        var validationCommissionPercentage = ValidateCommissionPercentage(createBrokerDto.CommissionPercentage);

        if (validationCommissionPercentage is not null)
        {
            return Result<BrokerDto>.Failure(validationCommissionPercentage);
        }

        var brokerCodeExists = await brokerRepository.BrokerCodeExistsAsync(createBrokerDto.BrokerCode, null, cancellationToken);

        if(brokerCodeExists)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.DuplicateBrokerCode(createBrokerDto.BrokerCode));
        }

        var broker = new Broker
        {
            BrokerCode = createBrokerDto.BrokerCode,
            Name = createBrokerDto.Name,
            Email = createBrokerDto.Email,
            Phone = createBrokerDto.Phone,
            CommissionPercentage = createBrokerDto.CommissionPercentage,
            IsActive = createBrokerDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await brokerRepository.AddBrokerAsync(broker, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.DuplicateBrokerCode(createBrokerDto.BrokerCode));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Broker {BrokerId} created.", broker.BrokerId);
        }

        return Result<BrokerDto>.Success(MapBrokerToDto(broker));
    }

    public async Task<Result<BrokerDto>> UpdateBrokerAsync(Guid brokerId, UpdateBrokerDto updateBrokerDto, CancellationToken cancellationToken)
    {
        if (brokerId == Guid.Empty)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.InvalidBrokerId);
        }

        updateBrokerDto = updateBrokerDto with
        {
            BrokerCode = updateBrokerDto.BrokerCode?.Trim().ToUpperInvariant()!,
            Name = updateBrokerDto.Name?.Trim()!,
            Email = updateBrokerDto.Email?.Trim(),
            Phone = updateBrokerDto.Phone?.Trim()
        };

        var validationBrokerCode = ValidateBrokerCode(updateBrokerDto.BrokerCode);

        if (validationBrokerCode is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerCode);
        }

        var validationBrokerName = ValidateBrokerName(updateBrokerDto.Name);

        if (validationBrokerName is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerName);
        }

        var validationBrokerContactInfo = ValidateBrokerContactInfo(updateBrokerDto.Email, updateBrokerDto.Phone);

        if (validationBrokerContactInfo is not null)
        {
            return Result<BrokerDto>.Failure(validationBrokerContactInfo);
        }

        var validationCommissionPercentage = ValidateCommissionPercentage(updateBrokerDto.CommissionPercentage);

        if (validationCommissionPercentage is not null)
        {
            return Result<BrokerDto>.Failure(validationCommissionPercentage);
        }

        var broker = await brokerRepository.GetBrokerForUpdateAsync(brokerId, cancellationToken);

        if (broker is null)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.NotFound(brokerId));
        }

        var brokerCodeExists = await brokerRepository.BrokerCodeExistsAsync(updateBrokerDto.BrokerCode, brokerId, cancellationToken);

        if (brokerCodeExists)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.DuplicateBrokerCode(updateBrokerDto.BrokerCode));
        }

        broker.BrokerCode = updateBrokerDto.BrokerCode;
        broker.Name = updateBrokerDto.Name;
        broker.Email = updateBrokerDto.Email;
        broker.Phone = updateBrokerDto.Phone;
        broker.CommissionPercentage = updateBrokerDto.CommissionPercentage;
        broker.ModifiedAt = DateTime.UtcNow;

        try
        {
            await brokerRepository.SaveBrokerChangesAsync(cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.DuplicateBrokerCode(updateBrokerDto.BrokerCode));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Broker {BrokerId} updated.", brokerId);
        }

        return Result<BrokerDto>.Success(MapBrokerToDto(broker));
    }

    public async Task<Result<BrokerDto>> ActivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        if (brokerId == Guid.Empty)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.InvalidBrokerId);
        }

        var broker = await brokerRepository.GetBrokerForUpdateAsync(brokerId, cancellationToken);

        if (broker is null)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.NotFound(brokerId));
        }

        broker.IsActive = true;
        broker.ModifiedAt = DateTime.UtcNow;

        await brokerRepository.SaveBrokerChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Broker {BrokerId} activated.", brokerId);
        }

        return Result<BrokerDto>.Success(MapBrokerToDto(broker));
    }

    public async Task<Result<BrokerDto>> DeactivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        if (brokerId == Guid.Empty)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.InvalidBrokerId);
        }

        var broker = await brokerRepository.GetBrokerForUpdateAsync(brokerId, cancellationToken);

        if (broker is null)
        {
            return Result<BrokerDto>.Failure(BrokerErrors.NotFound(brokerId));
        }

        broker.IsActive = false;
        broker.ModifiedAt = DateTime.UtcNow;

        await brokerRepository.SaveBrokerChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Broker {BrokerId} deactivated.", brokerId);
        }

        return Result<BrokerDto>.Success(MapBrokerToDto(broker));
    }

    private static BrokerDto MapBrokerToDto(Broker broker)
    {
        return new BrokerDto(
            broker.BrokerId,
            broker.BrokerCode,
            broker.Name,
            broker.Email,
            broker.Phone,
            broker.CommissionPercentage,
            broker.IsActive);
    }

    private static Error? ValidateBrokerCode(string? brokerCode)
    {
        if (string.IsNullOrWhiteSpace(brokerCode))
        {
            return BrokerErrors.BrokerCodeRequired;
        }

        if (brokerCode.Length is < BrokerConstraints.BrokerCodeMinLength or > BrokerConstraints.BrokerCodeMaxLength)
        {
            return BrokerErrors.InvalidBrokerCodeLength;
        }

        return null;
    }

    private static Error? ValidateBrokerName(string? brokerName)
    {
        if (string.IsNullOrWhiteSpace(brokerName))
        {
            return BrokerErrors.NameRequired;
        }

        if (brokerName.Length is < BrokerConstraints.NameMinLength or > BrokerConstraints.NameMaxLength)
        {
            return BrokerErrors.InvalidNameLength;
        }

        return null;
    }

    private static Error? ValidateBrokerContactInfo(string? brokerEmail, string? brokerPhone)
    {
        if (!string.IsNullOrWhiteSpace(brokerEmail)
            && (brokerEmail.Length > BrokerConstraints.EmailMaxLength || !MailAddress.TryCreate(brokerEmail, out _)))
        {
            return BrokerErrors.InvalidEmail;
        }

        if (!string.IsNullOrWhiteSpace(brokerPhone) && brokerPhone.Length > BrokerConstraints.PhoneMaxLength)
        {
            return BrokerErrors.InvalidPhoneLength;
        }

        return null;
    }

    private static Error? ValidateCommissionPercentage(decimal? commissionPercentage)
    {
        if (!commissionPercentage.HasValue)
        {
            return null;
        }

        if (!DecimalValidation.HasValidScale(commissionPercentage.Value, BrokerConstraints.CommissionPercentageScale))
        {
            return BrokerErrors.InvalidCommissionPercentageScale;
        }

        if (commissionPercentage.Value is < BrokerConstraints.MinCommissionPercentage or > BrokerConstraints.MaxCommissionPercentage)
        {
            return BrokerErrors.InvalidCommissionPercentage;
        }

        return null;
    }
}