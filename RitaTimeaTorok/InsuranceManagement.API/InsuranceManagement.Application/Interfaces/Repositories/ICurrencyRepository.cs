using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface ICurrencyRepository : IGenericRepository<Currency>
    {
        Task<bool> ExistsByCodeAsync(string code);
    }
}
