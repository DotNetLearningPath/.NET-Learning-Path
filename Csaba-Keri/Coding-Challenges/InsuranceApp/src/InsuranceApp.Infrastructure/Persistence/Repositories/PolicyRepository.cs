using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Policies;
using InsuranceApp.Domain.Policies;
using InsuranceApp.Infrastructure.Persistence.Entities;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class PolicyRepository(InsuranceDbContext context) : IPolicyRepository
{
    private readonly InsuranceDbContext _context = context;

    public async Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        var entity = await _context.Policies
            .AsNoTracking()
            .Include(policy => policy.AppliedAdjustments)
            .SingleOrDefaultAsync(policy => policy.Id == policyId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        _context.Policies.Add(policy.ToEntity());

        try
        {
            // The policy and its applied adjustments are saved in one transaction.
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
            DatabaseNames.PolicyClientForeignKey =>
                ValidationExceptionFactory.Create(nameof(PolicyEntityData.ClientId), "The selected client does not exist."),
            
            DatabaseNames.PolicyBuildingForeignKey =>
                ValidationExceptionFactory.Create(nameof(PolicyEntityData.BuildingId), "The selected building does not exist."),
            
            DatabaseNames.PolicyBrokerForeignKey =>
                ValidationExceptionFactory.Create(nameof(PolicyEntityData.BrokerId), "The selected broker does not exist."),
            
            DatabaseNames.PolicyCurrencyForeignKey =>
                ValidationExceptionFactory.Create(nameof(PolicyEntityData.CurrencyId), "The selected currency does not exist."),
            
            _ => null
        };

        return translatedException is not null;
    }
}
