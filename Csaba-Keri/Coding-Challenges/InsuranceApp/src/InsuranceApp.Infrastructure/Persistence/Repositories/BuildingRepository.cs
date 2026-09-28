using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class BuildingRepository : IBuildingRepository
{
    private readonly InsuranceDbContext _context;

    public BuildingRepository(InsuranceDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<Building?> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        var entity = await _context.Buildings
            .AsNoTracking()
            .SingleOrDefaultAsync(building => building.Id == buildingId, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<PagedResult<Building>> GetBuildingsByClientIdAsync(Guid clientId, PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Buildings
            .AsNoTracking()
            .Where(building => building.ClientId == clientId)
            .OrderBy(building => building.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        _context.Buildings.Add(building.ToEntity());

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgresException
            && IsKnownForeignKeyViolation(postgresException)
        )
        {
            throw TranslateForeignKeyViolation(postgresException, building.ClientId);
        }
    }

    public async Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        var affected = 0;

        try
        {
            affected = await _context.Buildings
                .Where(entity => entity.Id == building.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(entity => entity.Type, building.Type)
                    .SetProperty(entity => entity.Street, building.Address.Street)
                    .SetProperty(entity => entity.Number, building.Address.Number)
                    .SetProperty(entity => entity.CityId, building.Address.CityId)
                    .SetProperty(entity => entity.ConstructionYear, building.ConstructionYear)
                    .SetProperty(entity => entity.NumberOfFloors, building.NumberOfFloors)
                    .SetProperty(entity => entity.SurfaceArea, building.SurfaceArea)
                    .SetProperty(entity => entity.InsuredValue, building.InsuredValue), cancellationToken);
        }
        catch (PostgresException exception) when (IsKnownForeignKeyViolation(exception))
        {
            throw TranslateForeignKeyViolation(exception, building.ClientId);
        }

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(Building), building.Id);
        }
    }

    private static bool IsKnownForeignKeyViolation(PostgresException exception)
    {
        return exception.SqlState == PostgresErrorCodes.ForeignKeyViolation
            && exception.ConstraintName is DatabaseNames.BuildingClientForeignKey or DatabaseNames.BuildingCityForeignKey;
    }

    private static Exception TranslateForeignKeyViolation(PostgresException exception, Guid clientId)
    {
        return exception.ConstraintName == DatabaseNames.BuildingClientForeignKey
            ? new EntityNotFoundException(nameof(Client), clientId)
            : ValidationExceptionFactory.Create("Address.CityId", "The selected city does not exist.");
    }
}
