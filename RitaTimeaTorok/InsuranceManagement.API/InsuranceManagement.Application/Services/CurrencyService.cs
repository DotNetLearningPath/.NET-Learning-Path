using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class CurrencyService(
        ICurrencyRepository currencyRepository,
        IMapper mapper,
        ILogger<CurrencyService> logger) : ICurrencyService
    {
        public async Task<PagedResult<CurrencyDto>> GetAllCurrenciesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await currencyRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved currencies page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} currencies",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<CurrencyDto>
            {
                Items = mapper.Map<List<CurrencyDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto request)
        {
            ValidateCreateCurrency(request.Code, request.Name, request.ExchangeRateToBase);

            var code = request.Code.Trim().ToUpperInvariant();
            if (await currencyRepository.ExistsByCodeAsync(code))
                throw new ConflictException($"Currency code '{code}' is already in use.");

            var currency = mapper.Map<Currency>(request);
            currency.Code = code;
            currency.Name = request.Name.Trim();
            currency.IsActive = request.IsActive ?? true;
            var createdCurrency = await currencyRepository.AddAsync(currency);

            logger.LogInformation("Created currency {CurrencyId} with code {CurrencyCode}",
                createdCurrency.Id, createdCurrency.Code);
            return mapper.Map<CurrencyDto>(createdCurrency);
        }

        public async Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto request)
        {
            var currency = await currencyRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Currency '{id}' was not found.");

            ValidateUpdateCurrency(currency, request);

            if (request.ExchangeRateToBase.HasValue)
                currency.ExchangeRateToBase = request.ExchangeRateToBase.Value;

            if (request.IsActive.HasValue)
                currency.IsActive = request.IsActive.Value;

            await currencyRepository.UpdateAsync(currency);

            logger.LogInformation(
                "Updated currency {CurrencyId}; active: {IsActive}, exchange rate: {ExchangeRate}",
                currency.Id,
                currency.IsActive,
                currency.ExchangeRateToBase);

            return mapper.Map<CurrencyDto>(currency);
        }

        private static void ValidateCreateCurrency(string code, string name, decimal exchangeRateToBase)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ValidationException("Currency code is required.");
            if (code.Trim().Length != 3)
                throw new ValidationException("Currency code must contain exactly 3 characters.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Currency name is required.");
            if (name.Trim().Length > 100)
                throw new ValidationException("Currency name cannot exceed 100 characters.");
            if (exchangeRateToBase <= 0)
                throw new ValidationException("Exchange rate to base must be greater than zero.");
        }

        private static void ValidateUpdateCurrency(Currency currency, UpdateCurrencyDto request)
            {
                if (currency.Code == "RON")
                    throw new ConflictException("The base currency cannot be changed.");

                if (!request.IsActive.HasValue && !request.ExchangeRateToBase.HasValue)
                    throw new ValidationException("At least one field must be provided.");

                if (request.ExchangeRateToBase.HasValue &&
                    request.ExchangeRateToBase.Value <= 0)
                    throw new ValidationException("Exchange rate to base must be greater than zero.");

                var isActiveUnchanged = !request.IsActive.HasValue ||
                    currency.IsActive == request.IsActive.Value;
                var exchangeRateUnchanged = !request.ExchangeRateToBase.HasValue ||
                    currency.ExchangeRateToBase == request.ExchangeRateToBase.Value;

                if (isActiveUnchanged && exchangeRateUnchanged)
                {
                    throw new ConflictException($"Currency '{currency.Code}' already has the requested values.");
                }
            }
    }
}
