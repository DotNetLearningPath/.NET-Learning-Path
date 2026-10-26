using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<Client>> SearchAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken);

        Task<bool> ExistsByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken);

        Task AddAsync(Client client, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
