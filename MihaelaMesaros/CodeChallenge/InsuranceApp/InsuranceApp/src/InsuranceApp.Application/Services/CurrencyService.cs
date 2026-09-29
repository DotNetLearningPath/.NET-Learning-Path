using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class CurrencyService(ICurrencyRepository currencyRepository, ILogger<CurrencyService> logger) : ICurrencyService
{
    public async Task<Result<IReadOnlyList<CurrencyDto>>> GetCurrenciesAsync(CancellationToken cancellationToken)
    {
        var currencies = await currencyRepository.GetCurrenciesAsync(cancellationToken);

        var currencyDtos = currencies.Select(MapCurrencyToDto).ToList();

        return Result<IReadOnlyList<CurrencyDto>>.Success(currencyDtos);
    }

    public async Task<Result<CurrencyDto>> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        if (currencyId == Guid.Empty)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.InvalidCurrencyId);
        }

        var currency = await currencyRepository.GetCurrencyByIdAsync(currencyId, cancellationToken);

        if (currency is null)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.NotFound(currencyId));
        }

        return Result<CurrencyDto>.Success(MapCurrencyToDto(currency));
    }

    public async Task<Result<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto createCurrencyDto, CancellationToken cancellationToken)
    {
        createCurrencyDto = createCurrencyDto with
        {
            Code = createCurrencyDto.Code?.Trim().ToUpperInvariant()!,
            Name = createCurrencyDto.Name?.Trim()!
        };

        var validationError = ValidateCurrencyDetails(createCurrencyDto.Code, createCurrencyDto.Name, createCurrencyDto.ExchangeRateToBase);

        if (validationError is not null)
        {
            return Result<CurrencyDto>.Failure(validationError);
        }

        var codeExists = await currencyRepository.CurrencyCodeExistsAsync(createCurrencyDto.Code, null, cancellationToken);

        if (codeExists)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.DuplicateCode);
        }


        var currency = new Currency
        {
            Code = createCurrencyDto.Code,
            Name = createCurrencyDto.Name,
            ExchangeRateToBase = createCurrencyDto.ExchangeRateToBase,
            IsActive = createCurrencyDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await currencyRepository.AddCurrencyAsync(currency, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.DuplicateCode);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Currency {CurrencyId} created.", currency.CurrencyId);
        }

        return Result<CurrencyDto>.Success(MapCurrencyToDto(currency));
    }

    public async Task<Result<CurrencyDto>> UpdateCurrencyAsync(Guid currencyId, UpdateCurrencyDto updateCurrencyDto, CancellationToken cancellationToken)
    {
        if (currencyId == Guid.Empty)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.InvalidCurrencyId);
        }

        updateCurrencyDto = updateCurrencyDto with
        {
            Code = updateCurrencyDto.Code?.Trim().ToUpperInvariant()!,
            Name = updateCurrencyDto.Name?.Trim()!
        };

        var validationError = ValidateCurrencyDetails(updateCurrencyDto.Code, updateCurrencyDto.Name, updateCurrencyDto.ExchangeRateToBase);

        if (validationError is not null)
        {
            return Result<CurrencyDto>.Failure(validationError);
        }

        var currency = await currencyRepository.GetCurrencyForUpdateAsync(currencyId, cancellationToken);

        if (currency is null)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.NotFound(currencyId));
        }

        var codeExists = await currencyRepository.CurrencyCodeExistsAsync(updateCurrencyDto.Code, currencyId, cancellationToken);

        if (codeExists)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.DuplicateCode);
        }


        currency.Code = updateCurrencyDto.Code;
        currency.Name = updateCurrencyDto.Name;
        currency.ExchangeRateToBase = updateCurrencyDto.ExchangeRateToBase;
        currency.IsActive = updateCurrencyDto.IsActive;
        currency.ModifiedAt = DateTime.UtcNow;

        try
        {
            await currencyRepository.SaveCurrencyChangesAsync(cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<CurrencyDto>.Failure(CurrencyErrors.DuplicateCode);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Currency {CurrencyId} updated.", currencyId);
        }

        return Result<CurrencyDto>.Success(MapCurrencyToDto(currency));
    }

    private static Error? ValidateCurrencyDetails(string? code, string? name, decimal exchangeRate)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return CurrencyErrors.CodeRequired;
        }

        if (code.Length is < CurrencyConstraints.CodeMinLength or > CurrencyConstraints.CodeMaxLength)
        {
            return CurrencyErrors.InvalidCodeLength;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return CurrencyErrors.NameRequired;
        }

        if (name.Length is < CurrencyConstraints.NameMinLength or > CurrencyConstraints.NameMaxLength)
        {
            return CurrencyErrors.InvalidNameLength;
        }

        if (!DecimalValidation.HasValidScale(exchangeRate, CurrencyConstraints.ExchangeRateScale))
        {
            return CurrencyErrors.InvalidExchangeRateScale;
        }

        if (exchangeRate is < CurrencyConstraints.MinExchangeRate or > CurrencyConstraints.MaxExchangeRate)
        {
            return CurrencyErrors.InvalidExchangeRate;
        }

        return null;
    }

    private static CurrencyDto MapCurrencyToDto(Currency currency)
    {
        return new CurrencyDto(
            currency.CurrencyId,
            currency.Code,
            currency.Name,
            currency.ExchangeRateToBase,
            currency.IsActive
        );
    }

}