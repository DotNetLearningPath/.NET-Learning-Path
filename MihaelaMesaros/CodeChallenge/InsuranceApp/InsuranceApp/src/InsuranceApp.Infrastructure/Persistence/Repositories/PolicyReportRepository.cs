using Dapper;
using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public sealed class PolicyReportRepository(InsuranceDbContext dbContext) : IPolicyReportRepository
{
    public async Task<IReadOnlyList<PolicyReportDto>> GetReportPoliciesAsync(
        PolicyReportGroupType groupType, 
        PolicyReportFiltersDto policyReportFiltersDto, 
        CancellationToken cancellationToken
    )
    {
        if (!Enum.IsDefined(groupType))
        {
            throw new ArgumentOutOfRangeException(nameof(groupType));
        }

        var queryParameters = new
        {
            @GroupType = groupType.ToString(),
            @From = policyReportFiltersDto.From,
            @To = policyReportFiltersDto.To,
            @Status = policyReportFiltersDto.Status,
            @CurrencyId = policyReportFiltersDto.Currency,
            @BuildingTypeId = policyReportFiltersDto.BuildingType
        };

        const string query = """
        ;WITH PolicyData AS
        (
            SELECT
                CASE @GroupType
                    WHEN 'Country' THEN co.CountryId
                    WHEN 'County' THEN cou.CountyId
                    WHEN 'City' THEN ci.CityId
                    WHEN 'Broker' THEN br.BrokerId
                END AS GroupId,

                CASE @GroupType
                    WHEN 'Country' THEN co.[Name]
                    WHEN 'County' THEN cou.[Name]
                    WHEN 'City' THEN ci.[Name]
                    WHEN 'Broker' THEN br.[Name]
                END AS GroupName,

                cu.CurrencyId,
                cu.Code AS CurrencyCode,
                p.FinalPremium,
                cu.ExchangeRateToBase
            FROM dbo.Policies p
                INNER JOIN dbo.Buildings b ON b.BuildingId = p.BuildingId
                INNER JOIN dbo.Cities ci ON ci.CityId = b.CityId
                INNER JOIN dbo.Counties cou ON cou.CountyId = ci.CountyId
                INNER JOIN dbo.Countries co ON co.CountryId = cou.CountryId
                INNER JOIN dbo.Currencies cu ON cu.CurrencyId = p.CurrencyId
                INNER JOIN dbo.Brokers br ON br.BrokerId = p.BrokerId
            WHERE
                (@From IS NULL OR p.StartDate >= @From)
                AND (@To IS NULL OR p.StartDate < DATEADD(DAY, 1, @To))
                AND (@Status IS NULL OR p.Status = @Status)
                AND (@CurrencyId IS NULL OR p.CurrencyId = @CurrencyId)
                AND (@BuildingTypeId IS NULL OR b.BuildingTypeId = @BuildingTypeId)
        )
        SELECT
            GroupId,
            @GroupType AS GroupType,
            GroupName,
            CurrencyId,
            CurrencyCode,
            COUNT(*) AS PolicyCount,
            SUM(FinalPremium) AS TotalFinalPremium,
            SUM(FinalPremium * ExchangeRateToBase) AS TotalFinalPremiumInBaseCurrency
        FROM PolicyData
        GROUP BY
            GroupId,
            GroupName,
            CurrencyId,
            CurrencyCode
        ORDER BY
            GroupName,
            CurrencyCode;
        """;

        var command = new CommandDefinition(
            query,
            queryParameters,
            cancellationToken: cancellationToken
        );

        var connection = dbContext.Database.GetDbConnection();

        var results = await connection.QueryAsync<PolicyReportDto>(command);

        return results.ToList();
    }
}
