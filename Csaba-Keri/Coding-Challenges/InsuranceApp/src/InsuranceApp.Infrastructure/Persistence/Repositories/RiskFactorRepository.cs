using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.RiskFactors;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class RiskFactorRepository(InsuranceDbContext context) : IRiskFactorRepository
{
    private readonly InsuranceDbContext _context = context;

    public async Task<RiskFactorConfiguration?> GetRiskFactorByIdAsync(Guid riskFactorId, CancellationToken cancellationToken)
    {
        var entity = await _context.RiskFactorConfigurations
            .AsNoTracking()
            .SingleOrDefaultAsync(riskFactor => riskFactor.Id == riskFactorId, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<PagedResult<RiskFactorConfiguration>> GetRiskFactorsAsync(PageQuery query, CancellationToken cancellationToken)
    {
        return _context.RiskFactorConfigurations
            .AsNoTracking()
            .OrderBy(riskFactor => riskFactor.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public async Task<IReadOnlyList<RiskFactorConfiguration>> GetApplicableRiskFactorsAsync(Guid cityId, BuildingType buildingType, CancellationToken cancellationToken)
    {
        var query = (
            from city in _context.Cities
            join county in _context.Counties on city.CountyId equals county.Id
            from riskFactor in _context.RiskFactorConfigurations
            where riskFactor.IsActive
                && city.Id == cityId
                && ((riskFactor.Level == RiskFactorLevel.Country && riskFactor.CountryId == county.CountryId)
                    || (riskFactor.Level == RiskFactorLevel.County && riskFactor.CountyId == county.Id)
                    || (riskFactor.Level == RiskFactorLevel.City && riskFactor.CityId == city.Id)
                    || (riskFactor.Level == RiskFactorLevel.BuildingType && riskFactor.BuildingType == buildingType))
            orderby riskFactor.Id
            select riskFactor
        );

        var entities = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. entities.Select(entity => entity.ToDomain())];
    }

    public async Task AddRiskFactorAsync(RiskFactorConfiguration riskFactor, CancellationToken cancellationToken)
    {
        _context.RiskFactorConfigurations.Add(riskFactor.ToEntity());

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgresException
            && IsForeignKeyViolation(postgresException)
        )
        {
            if (TryTranslateForeignKeyViolation(postgresException, out var translatedException))
            {
                throw translatedException;
            }

            throw;
        }
    }

    public async Task UpdateRiskFactorAsync(RiskFactorConfiguration riskFactor, CancellationToken cancellationToken)
    {
        var updatedEntity = riskFactor.ToEntity();
        var affected = 0;

        try
        {
            affected = await _context.RiskFactorConfigurations
                .Where(entity => entity.Id == updatedEntity.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(entity => entity.Level, updatedEntity.Level)
                    .SetProperty(entity => entity.CountryId, updatedEntity.CountryId)
                    .SetProperty(entity => entity.CountyId, updatedEntity.CountyId)
                    .SetProperty(entity => entity.CityId, updatedEntity.CityId)
                    .SetProperty(entity => entity.BuildingType, updatedEntity.BuildingType)
                    .SetProperty(entity => entity.AdjustmentPercentage, updatedEntity.AdjustmentPercentage)
                    .SetProperty(entity => entity.IsActive, updatedEntity.IsActive), cancellationToken);
        }
        catch (PostgresException postgresException) when (IsForeignKeyViolation(postgresException))
        {
            if (TryTranslateForeignKeyViolation(postgresException, out var translatedException))
            {
                throw translatedException;
            }

            throw;
        }

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(RiskFactorConfiguration), updatedEntity.Id);
        }
    }

    private static bool IsForeignKeyViolation(PostgresException exception)
    {
        return exception.SqlState == PostgresErrorCodes.ForeignKeyViolation;
    }

    private static bool TryTranslateForeignKeyViolation(
        PostgresException exception,
        [NotNullWhen(true)] out Exception? translatedException
    )
    {
        translatedException = exception.ConstraintName switch
        {
            DatabaseNames.RiskFactorCountryForeignKey =>
                ValidationExceptionFactory.Create("CountryId", "The selected country does not exist."),

            DatabaseNames.RiskFactorCountyForeignKey =>
                ValidationExceptionFactory.Create("CountyId", "The selected county does not exist."),

            DatabaseNames.RiskFactorCityForeignKey =>
                ValidationExceptionFactory.Create("CityId", "The selected city does not exist."),

            _ => null
        };

        return translatedException is not null;
    }
}
