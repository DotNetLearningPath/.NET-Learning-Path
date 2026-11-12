using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Enums;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class PolicyRepository(InsuranceDbContext context)
        : GenericRepository<Policy>(context), IPolicyRepository
    {
        public async Task<PagedResult<Policy>> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateOnly? startDate,
            DateOnly? endDate,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Policies.AsNoTracking();

            if (clientId.HasValue)
                query = query.Where(policy => policy.ClientId == clientId.Value);
            if (brokerId.HasValue)
                query = query.Where(policy => policy.BrokerId == brokerId.Value);
            if (status.HasValue)
                query = query.Where(policy => policy.Status == status.Value);
            if (startDate.HasValue)
                query = query.Where(policy => policy.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(policy => policy.EndDate <= endDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(policy => policy.CreatedAt)
                .ThenBy(policy => policy.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Policy>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<Policy?> GetByIdWithSnapshotsAsync(Guid id) =>
            _context.Policies
                .Include(policy => policy.CalculationSnapshotEntries)
                .SingleOrDefaultAsync(policy => policy.Id == id);

        public async Task SavePolicyChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
