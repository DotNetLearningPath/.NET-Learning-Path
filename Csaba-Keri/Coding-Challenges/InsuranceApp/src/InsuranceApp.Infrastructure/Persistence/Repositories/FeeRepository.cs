using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Fees;
using InsuranceApp.Domain.Fees;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class FeeRepository(InsuranceDbContext context) : IFeeRepository
{
    private readonly InsuranceDbContext _context = context;

    public async Task<FeeConfiguration?> GetFeeByIdAsync(Guid feeId, CancellationToken cancellationToken)
    {
        var entity = await _context.FeeConfigurations
            .AsNoTracking()
            .SingleOrDefaultAsync(fee => fee.Id == feeId, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<PagedResult<FeeConfiguration>> GetFeesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        return _context.FeeConfigurations
            .AsNoTracking()
            .OrderBy(fee => fee.Name)
            .ThenBy(fee => fee.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public async Task<IReadOnlyList<FeeConfiguration>> GetApplicableFeesAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var entities = await _context.FeeConfigurations
            .AsNoTracking()
            .Where(fee =>
                fee.IsActive
                && fee.EffectiveFrom <= date
                && (fee.EffectiveTo == null || fee.EffectiveTo >= date)
            )
            .OrderBy(fee => fee.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(entity => entity.ToDomain())];
    }

    public async Task AddFeeAsync(FeeConfiguration fee, CancellationToken cancellationToken)
    {
        _context.FeeConfigurations.Add(fee.ToEntity());

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateFeeAsync(FeeConfiguration fee, CancellationToken cancellationToken)
    {
        var affected = await _context.FeeConfigurations
            .Where(entity => entity.Id == fee.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Name, fee.Name)
                .SetProperty(entity => entity.Type, fee.Type)
                .SetProperty(entity => entity.Percentage, fee.Percentage)
                .SetProperty(entity => entity.EffectiveFrom, fee.EffectiveFrom)
                .SetProperty(entity => entity.EffectiveTo, fee.EffectiveTo)
                .SetProperty(entity => entity.IsActive, fee.IsActive), cancellationToken);

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(FeeConfiguration), fee.Id);
        }
    }
}
