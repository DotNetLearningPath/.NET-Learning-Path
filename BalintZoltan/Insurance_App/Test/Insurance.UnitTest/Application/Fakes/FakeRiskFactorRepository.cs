using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeRiskFactorRepository : IRiskFactorRepository
{
    public readonly List<RiskFactorConfiguration> Storage = [];

    public Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken)
    {
        Storage.Add(configuration);
        return Task.CompletedTask;
    }

    public Task<RiskFactorConfiguration?> GetByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken)
    {
        var match = Storage.FirstOrDefault(configuration =>
            configuration.IsActive
            && configuration.Level == level
            && configuration.Reference == reference);
        return Task.FromResult(match);
    }

    public Task<RiskFactorConfiguration?> GetByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken)
    {
        var match = Storage.FirstOrDefault(configuration =>
            configuration.IsActive
            && configuration.Level == RiskFactorLevel.BuildingType
            && configuration.Reference == buildingType.ToString());
        return Task.FromResult(match);
    }

    public Task<IReadOnlyCollection<RiskFactorConfiguration>> GetApplicableRiskFactorsAsync(
        Guid? countryId,
        Guid? countyId,
        Guid? cityId,
        BuildingType? buildingType,
        CancellationToken cancellationToken)
    {
        var countryReference = countryId?.ToString();
        var countyReference = countyId?.ToString();
        var cityReference = cityId?.ToString();
        var buildingTypeReference = buildingType?.ToString();

        IReadOnlyCollection<RiskFactorConfiguration> matches = [.. Storage
            .Where(configuration => configuration.IsActive
                && ((countryReference != null
                        && configuration.Level == RiskFactorLevel.Country
                        && configuration.Reference == countryReference)
                    || (countyReference != null
                        && configuration.Level == RiskFactorLevel.County
                        && configuration.Reference == countyReference)
                    || (cityReference != null
                        && configuration.Level == RiskFactorLevel.City
                        && configuration.Reference == cityReference)
                    || (buildingTypeReference != null
                        && configuration.Level == RiskFactorLevel.BuildingType
                        && configuration.Reference == buildingTypeReference)))];

        return Task.FromResult(matches);
    }

    public Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var all = Storage
            .OrderBy(configuration => configuration.Level)
            .ThenBy(configuration => configuration.Reference)
            .ThenBy(configuration => configuration.Id)
            .ToList();
        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);

        return Task.FromResult(new PagedResult<RiskFactorConfiguration>
        {
            Items = [.. all
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }
}
