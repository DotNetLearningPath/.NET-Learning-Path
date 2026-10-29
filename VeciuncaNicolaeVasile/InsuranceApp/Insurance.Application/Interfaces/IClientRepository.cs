using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Client?> GetClientByIdWithBuildingsAsync(Guid id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Client>> SearchClientsAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken);

        Task<bool> ExistsClientByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken);

        Task AddAsync(Client client, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
