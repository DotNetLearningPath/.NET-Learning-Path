namespace SupportAPI.Application.Abstractions;

public interface IUserProvisioningService
{
    Task<Guid> FindOrCreateUserAsync(string name, string issuer, string subject, CancellationToken cancellationToken);
}
