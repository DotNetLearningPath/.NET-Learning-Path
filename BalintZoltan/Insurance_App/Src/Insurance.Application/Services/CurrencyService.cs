using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Currencies;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public sealed class CurrencyService(ICurrencyRepository repository) : ICurrencyService
{
    public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request.Code, request.Name, request.ExchangeRateToBase);
        var code = request.Code.Trim().ToUpperInvariant();
        await EnsureCurrencyCodeIsUniqueAsync(code, null, cancellationToken);

        var currency = new Currency(code, request.Name, request.ExchangeRateToBase, request.IsActive);
        await repository.AddCurrencyAsync(currency, cancellationToken);
        return MapToCurrencyDto(currency);
    }

    public async Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var currency = await repository.GetCurrencyByIdAsync(id, cancellationToken);
        return currency is null ? null : MapToCurrencyDto(currency);
    }

    public async Task<PagedResult<CurrencyDto>> ListCurrenciesAsync(PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await repository.ListCurrenciesAsync(pagination, cancellationToken);
        return new PagedResult<CurrencyDto>
        {
            Items = [.. result.Items.Select(MapToCurrencyDto)],
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request.Code, request.Name, request.ExchangeRateToBase);
        var currency = await GetRequiredCurrencyAsync(id, cancellationToken);
        var code = request.Code.Trim().ToUpperInvariant();
        await EnsureCurrencyCodeIsUniqueAsync(code, id, cancellationToken);

        currency.Update(code, request.Name, request.ExchangeRateToBase);
        await repository.UpdateCurrencyAsync(currency, cancellationToken);
        return MapToCurrencyDto(currency);
    }

    public Task<CurrencyDto> ActivateCurrencyAsync(Guid id, CancellationToken cancellationToken) =>
        SetActiveCurrencyAsync(id, true, cancellationToken);

    public Task<CurrencyDto> DeactivateCurrencyAsync(Guid id, CancellationToken cancellationToken) =>
        SetActiveCurrencyAsync(id, false, cancellationToken);

    private async Task<CurrencyDto> SetActiveCurrencyAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        var currency = await GetRequiredCurrencyAsync(id, cancellationToken);
        if (active)
        {
            currency.Activate();
        }
        else
        {
            currency.Deactivate();
        }
        await repository.UpdateCurrencyAsync(currency, cancellationToken);
        return MapToCurrencyDto(currency);
    }

    private async Task<Currency> GetRequiredCurrencyAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetCurrencyByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Currency was not found.");

    private async Task EnsureCurrencyCodeIsUniqueAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await repository.ExistsCurrencyByCodeAsync(code, excludedId, cancellationToken))
        {
            throw new InvalidOperationException("A currency with this code already exists.");
        }
    }

    private static void ValidateRequest(string code, string name, decimal exchangeRateToBase)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != Currency.CodeLength)
        {
            throw new ArgumentException($"Currency code must contain exactly {Currency.CodeLength} characters.", nameof(code));
        }
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > Currency.NameMaxLength)
        {
            throw new ArgumentException($"Currency name is required and cannot exceed {Currency.NameMaxLength} characters.", nameof(name));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exchangeRateToBase);
    }

    private static CurrencyDto MapToCurrencyDto(Currency currency) => new()
    {
        Id = currency.Id,
        Code = currency.Code,
        Name = currency.Name,
        ExchangeRateToBase = currency.ExchangeRateToBase,
        IsActive = currency.IsActive
    };
}
