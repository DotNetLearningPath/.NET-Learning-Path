using InsuranceManagement.Domain.Entities;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<PagedResult<Client>> SearchAsync(
            string? name,
            string? identifier,
            int pageNumber,
            int pageSize);

        Task<bool> ExistsByIdentificationNumberAsync(
            string identificationNumber,
            Guid? excludeClientId = null);
    }
}
