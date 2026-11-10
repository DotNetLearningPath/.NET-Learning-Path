using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class CurrencyRepository(InsuranceDbContext context)
    : GenericRepository<Currency>(context), ICurrencyRepository
    {
        public Task<bool> ExistsByCodeAsync(string code) =>
            _context.Currencies.AnyAsync(c => c.Code == code);
    }
}
