using InsuranceApp.Application.Reports;
using InsuranceApp.Application.Reports.Queries;
using InsuranceApp.Application.Reports.Results;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class PolicyReportRepository(InsuranceDbContext context) : IPolicyReportRepository
{
    private readonly InsuranceDbContext _context = context;

    public async Task<IReadOnlyList<PolicyReportRowResult>> GetReportAsync(PolicyReportGrouping grouping, PolicyReportQuery query, CancellationToken cancellationToken)
    {
        var filteredPolicies = GetFilteredPolicies(query);

        var reportRows = grouping switch
        {
            PolicyReportGrouping.Country => SelectCountryReportRows(filteredPolicies),

            _ => throw new ArgumentOutOfRangeException(
                nameof(grouping),
                grouping,
                "Report grouping is invalid."
            )
        };

        return await reportRows
            .GroupBy(row => new
            {
                row.GroupId,
                row.GroupName,
                row.CurrencyId,
                row.CurrencyCode
            })
            .OrderBy(group => group.Key.GroupName)
            .ThenBy(group => group.Key.GroupId)
            .ThenBy(group => group.Key.CurrencyCode)
            .ThenBy(group => group.Key.CurrencyId)
            .Select(group => new PolicyReportRowResult(
                GroupId: group.Key.GroupId,
                GroupName: group.Key.GroupName,
                CurrencyId: group.Key.CurrencyId,
                CurrencyCode: group.Key.CurrencyCode,
                PolicyCount: group.LongCount(),
                TotalFinalPremium: group.Sum(row => row.FinalPremium),
                TotalFinalPremiumInBaseCurrency: group.Sum(row => row.FinalPremium * row.ExchangeRateToBase)
            ))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<PolicyReportData> GetFilteredPolicies(PolicyReportQuery query)
    {
        var policies = _context.Policies
            .AsNoTracking()
            .Where(policy => policy.StartDate >= query.From && policy.StartDate <= query.To);

        if (query.Status is not null)
        {
            policies = policies.Where(policy => policy.Status == query.Status);
        }

        if (query.CurrencyId is not null)
        {
            policies = policies.Where(policy => policy.CurrencyId == query.CurrencyId);
        }

        var buildings = _context.Buildings.AsNoTracking();

        if (query.BuildingType is not null)
        {
            buildings = buildings.Where(building => building.Type == query.BuildingType);
        }

        return (
            from policy in policies
            join building in buildings on policy.BuildingId equals building.Id
            join currency in _context.Currencies on policy.CurrencyId equals currency.Id
            select new PolicyReportData
            {
                CityId = building.CityId,
                BrokerId = policy.BrokerId,
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code,
                FinalPremium = policy.FinalPremium,
                ExchangeRateToBase = policy.CurrencyExchangeRateToBase
            }
        );
    }

    private IQueryable<PolicyReportGroupingData> SelectCountryReportRows(IQueryable<PolicyReportData> policies)
    {
        return (
            from policy in policies
            join city in _context.Cities on policy.CityId equals city.Id
            join county in _context.Counties on city.CountyId equals county.Id
            join country in _context.Countries on county.CountryId equals country.Id
            select new PolicyReportGroupingData
            {
                GroupId = country.Id,
                GroupName = country.Name,
                CurrencyId = policy.CurrencyId,
                CurrencyCode = policy.CurrencyCode,
                FinalPremium = policy.FinalPremium,
                ExchangeRateToBase = policy.ExchangeRateToBase
            }
        );
    }

    private sealed class PolicyReportData
    {
        public required Guid CityId { get; init; }
        public required Guid BrokerId { get; init; }
        public required Guid CurrencyId { get; init; }
        public required string CurrencyCode { get; init; }
        public required decimal FinalPremium { get; init; }
        public required decimal ExchangeRateToBase { get; init; }
    }

    private sealed class PolicyReportGroupingData
    {
        public required Guid GroupId { get; init; }
        public required string GroupName { get; init; }
        public required Guid CurrencyId { get; init; }
        public required string CurrencyCode { get; init; }
        public required decimal FinalPremium { get; init; }
        public required decimal ExchangeRateToBase { get; init; }
    }
}
