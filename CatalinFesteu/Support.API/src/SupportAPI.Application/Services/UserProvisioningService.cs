using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.Services;

public sealed class UserProvisioningService(IUserProvisioningRepository repository) : IUserProvisioningService
{
    public async Task<Guid> FindOrCreateUserAsync(
        string name, string issuer, string subject, CancellationToken cancellationToken)
    {
        var existingUserId = await repository.FindUserIdAsync(issuer, subject, cancellationToken);
        if (existingUserId.HasValue)
        {
            return existingUserId.Value;
        }

        var user = User.Create(name, issuer, subject);
        if (await repository.TryAddUserAsync(user, cancellationToken))
        {
            return user.Id;
        }

        // Another request may have inserted this identity after the lookup.
        return await repository.FindUserIdAsync(issuer, subject, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found after a concurrent insert.");
    }
}
