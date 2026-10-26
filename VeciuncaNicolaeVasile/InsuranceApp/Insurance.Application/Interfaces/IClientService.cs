using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IClientService
    {
        Task<Client> CreateAsync(Client client, CancellationToken cancellationToken);

        Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<Client>> SearchAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken);

        Task<Client?> UpdateAsync(Client client, CancellationToken cancellationToken);
    }
}
