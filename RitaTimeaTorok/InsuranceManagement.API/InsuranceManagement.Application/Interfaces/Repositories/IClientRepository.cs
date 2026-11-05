using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<List<Client>> SearchAsync(string? name, string? identifier);

        Task<bool> ExistsByIdentificationNumberAsync(
            string identificationNumber,
            Guid? excludeClientId = null);
    }
}
