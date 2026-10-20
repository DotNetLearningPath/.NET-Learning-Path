using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Policies;
using InsuranceApp.Application.Policies.Queries;
using InsuranceApp.Application.Policies.Results;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class PolicyQueryRepository(InsuranceDbContext context) : IPolicyQueryRepository
{
    private readonly InsuranceDbContext _context = context;

    public Task<PagedResult<PolicyListItemResult>> SearchPoliciesAsync(SearchPoliciesQuery query, CancellationToken cancellationToken)
    {
        var policies = _context.Policies.AsNoTracking();

        if (query.ClientId is not null)
        {
            policies = policies.Where(policy => policy.ClientId == query.ClientId);
        }

        if (query.BrokerId is not null)
        {
            policies = policies.Where(policy => policy.BrokerId == query.BrokerId);
        }

        if (query.Status is not null)
        {
            policies = policies.Where(policy => policy.Status == query.Status);
        }

        if (query.StartDate is not null)
        {
            policies = policies.Where(policy => policy.StartDate >= query.StartDate);
        }

        if (query.EndDate is not null)
        {
            policies = policies.Where(policy => policy.EndDate <= query.EndDate);
        }

        var policiesWithCurrency = policies.Join(
            _context.Currencies,
            policy => policy.CurrencyId,
            currency => currency.Id,
            (policy, currency) => new
            {
                Policy = policy,
                CurrencyCode = currency.Code
            }
        );

        return policiesWithCurrency
            .OrderByDescending(item => item.Policy.StartDate)
            .ThenBy(item => item.Policy.Id)
            .ToPageAsync(
                pageNumber: query.PageNumber,
                pageSize: query.PageSize,
                mapItem: item => item.Policy.ToPolicyListItemResult(item.CurrencyCode),
                cancellationToken: cancellationToken
            );
    }

    public async Task<PolicyDetailsResult?> GetPolicyDetailsAsync(Guid policyId, CancellationToken cancellationToken)
    {
        var policies = _context.Policies
            .Where(policy => policy.Id == policyId)
            .Include(policy => policy.AppliedAdjustments);

        var query = (
            from policy in policies
            join currency in _context.Currencies on policy.CurrencyId equals currency.Id
            join broker in _context.Brokers on policy.BrokerId equals broker.Id
            join client in _context.Clients on policy.ClientId equals client.Id
            join building in _context.Buildings on policy.BuildingId equals building.Id
            join city in _context.Cities on building.CityId equals city.Id
            join county in _context.Counties on city.CountyId equals county.Id
            join country in _context.Countries on county.CountryId equals country.Id
            select new
            {
                Policy = policy,
                CurrencyCode = currency.Code,
                Broker = broker,
                Client = client,
                Building = building,
                City = city,
                County = county,
                Country = country
            }
        );

        var item = await query
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return item?.Policy.ToPolicyDetailsResult(
            currencyCode: item.CurrencyCode,
            clientResult: item.Client.ToClientResult(),
            buildingDetailsResult: item.Building.ToBuildingDetailsResult(
                cityEntity: item.City,
                countyEntity: item.County,
                countryEntity: item.Country
            ),
            brokerResult: item.Broker.ToBrokerResult()
        );
    }
}
