namespace SupportAPI.Application.Abstractions
{
    public interface IUserProvisioningRepository
    {
        Task<Guid> FindOrCreateUserAsync(string name,
            string issuer, 
            string subject,
            CancellationToken cancellationToken);
    }
}
