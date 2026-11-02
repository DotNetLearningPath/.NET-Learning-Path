using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.Abstractions;

public interface IUserProvisioningRepository
{
    Task<Guid?> FindUserIdAsync(string issuer, string subject, CancellationToken cancellationToken);
    Task<bool> TryAddUserAsync(User user, CancellationToken cancellationToken);
}
