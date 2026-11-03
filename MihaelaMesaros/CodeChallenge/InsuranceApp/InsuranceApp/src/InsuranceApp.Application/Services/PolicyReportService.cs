using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Application.Settings;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Application.Services;

public sealed class PolicyReportService(IPolicyReportRepository policyReportRepository, IOptions<ApplicationSettings> options) : IPolicyReportService
{
    public async Task<Result<IReadOnlyList<PolicyReportDto>>> GetReportPoliciesAsync(
        PolicyReportGroupType groupType, 
        PolicyReportFiltersDto policyReportFiltersDto, 
        CancellationToken cancellationToken
    )
    {
        if (!Enum.IsDefined(groupType))
        {
            return Result<IReadOnlyList<PolicyReportDto>>.Failure(PolicyReportErrors.InvalidGroupType);
        }

        var configBaseCurrencyCode = options.Value.BaseCurrencyCode;

        var validationPolicyReportFilters = ValidatePolicyReportFilters(policyReportFiltersDto);

        if (validationPolicyReportFilters is not null)
        {
            return Result<IReadOnlyList<PolicyReportDto>>.Failure(validationPolicyReportFilters);
        }

        var result = await policyReportRepository.GetReportPoliciesAsync(groupType, policyReportFiltersDto, cancellationToken);

        foreach(var item in result)
        {
            item.BaseCurrencyCode = configBaseCurrencyCode;
            item.TotalFinalPremium = decimal.Round(item.TotalFinalPremium, CommonConstraints.PriceDecimalPlaces, CommonConstraints.MidpointRoundingMode);
            item.TotalFinalPremiumInBaseCurrency = decimal.Round(item.TotalFinalPremiumInBaseCurrency, CommonConstraints.PriceDecimalPlaces, CommonConstraints.MidpointRoundingMode);
        }

        return Result<IReadOnlyList<PolicyReportDto>>.Success(result);
    }

    private static Error? ValidatePolicyReportFilters(PolicyReportFiltersDto filters)
    {
        if (filters.From.HasValue && filters.To.HasValue && filters.From.Value.Date > filters.To.Value.Date)
        {
            return PolicyReportErrors.InvalidDateRange;
        }

        if (!string.IsNullOrWhiteSpace(filters.Status) && (!Enum.TryParse<PolicyStatus>(filters.Status, true, out var status) || !Enum.IsDefined(status)))
        {
            return PolicyReportErrors.InvalidStatus;
        }


        if (Guid.TryParse(filters.Currency, out var currencyId) && currencyId == Guid.Empty)
        {
            return PolicyReportErrors.InvalidCurrencyId;
        }

        if (Guid.TryParse(filters.BuildingType, out var buildingTypeId) && buildingTypeId == Guid.Empty)
        {
            return PolicyReportErrors.InvalidBuildingTypeId;
        }

        return null;
    }

}
