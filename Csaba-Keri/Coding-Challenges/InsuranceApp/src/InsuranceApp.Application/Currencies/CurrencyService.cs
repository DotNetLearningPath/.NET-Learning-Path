using FluentValidation;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Application.Currencies.Mappings;
using InsuranceApp.Application.Currencies.Results;
using InsuranceApp.Domain.Currencies;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Currencies;

public class CurrencyService(
    ICurrencyRepository currencyRepository,
    IValidator<CreateCurrencyCommand> createValidator,
    IValidator<UpdateCurrencyCommand> updateValidator,
    IValidator<PageQuery> pageValidator,
    ILogger<CurrencyService> logger
) : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository = currencyRepository;
    private readonly IValidator<CreateCurrencyCommand> _createValidator = createValidator;
    private readonly IValidator<UpdateCurrencyCommand> _updateValidator = updateValidator;
    private readonly IValidator<PageQuery> _pageValidator = pageValidator;
    private readonly ILogger<CurrencyService> _logger = logger;

    public async Task<CurrencyResult> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currency = await GetCurrencyByIdOrThrowAsync(currencyId, cancellationToken);

        return currency.ToResult();
    }

    public async Task<PagedResult<CurrencyResult>> GetCurrenciesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);
        
        var page = await _currencyRepository.GetCurrenciesAsync(query, cancellationToken);
        
        return page.Map(currency => currency.ToResult());
    }

    public async Task<CurrencyResult> CreateCurrencyAsync(CreateCurrencyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var currency = new Currency(
            id: Guid.NewGuid(),
            code: command.Code,
            name: command.Name,
            exchangeRateToBase: command.ExchangeRateToBase,
            isActive: command.IsActive
        );

        await _currencyRepository.AddCurrencyAsync(currency, cancellationToken);
        _logger.LogInformation("Currency {CurrencyId} created with code {Code}.", currency.Id, currency.Code);
        
        return currency.ToResult();
    }

    public async Task<CurrencyResult> UpdateCurrencyAsync(UpdateCurrencyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var currency = await GetCurrencyByIdOrThrowAsync(command.CurrencyId, cancellationToken);

        if (!CurrencyRules.IsBaseCurrencyRateValid(currency.Code, command.ExchangeRateToBase))
        {
            throw ValidationExceptionFactory.Create(
                "ExchangeRateToBase",
                $"The {CurrencyRules.BaseCurrencyCode} exchange rate must be 1."
            );
        }

        currency.UpdateDetails(
            name: command.Name,
            exchangeRateToBase: command.ExchangeRateToBase,
            isActive: command.IsActive
        );

        await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);
        _logger.LogInformation("Currency {CurrencyId} updated. IsActive: {IsActive}.", currency.Id, currency.IsActive);
        
        return currency.ToResult();
    }

    private async Task<Currency> GetCurrencyByIdOrThrowAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        if (currencyId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("CurrencyId", "Currency identifier must not be empty.");
        }

        return await _currencyRepository.GetCurrencyByIdAsync(currencyId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Currency), currencyId);
    }
}
