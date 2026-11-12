using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IGenericRepository<Policy>
    {
        Task<PagedResult<Policy>> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateOnly? startDate,
            DateOnly? endDate,
            int pageNumber,
            int pageSize);
        Task<Policy?> GetByIdWithSnapshotsAsync(Guid id);
        Task SavePolicyChangesAsync();
    }
}
