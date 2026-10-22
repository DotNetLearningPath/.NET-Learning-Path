using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Policies;
using InsuranceApp.Application.Policies.Exceptions;
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

    public async Task UpdatePolicyLifecycleAsync(Policy policy, PolicyStatus expectedCurrentStatus, CancellationToken cancellationToken)
    {
        var affected = await _context.Policies
            .Where(entity => entity.Id == policy.Id && entity.Status == expectedCurrentStatus)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Status, policy.Status)
                .SetProperty(entity => entity.UpdatedAt, policy.UpdatedAt)
                .SetProperty(entity => entity.ActivatedAt, policy.ActivatedAt)
                .SetProperty(entity => entity.CancelledAt, policy.CancelledAt)
                .SetProperty(entity => entity.CancellationEffectiveDate, policy.CancellationEffectiveDate)
                .SetProperty(entity => entity.CancellationReason, policy.CancellationReason)
                .SetProperty(entity => entity.ExpiredAt, policy.ExpiredAt), cancellationToken);

        if (affected == 0)
        {
            throw new PolicyConcurrencyException(policy.Id);
        }
    }

    public Task<int> ExpirePoliciesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        return _context.Policies
            .Where(entity => entity.Status == PolicyStatus.Active && entity.EndDate < today)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Status, PolicyStatus.Expired)
                .SetProperty(entity => entity.UpdatedAt, now)
                .SetProperty(entity => entity.ExpiredAt, now), cancellationToken);
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
