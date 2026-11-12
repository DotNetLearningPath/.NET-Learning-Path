using InsuranceManagement.Application.DTOs.Policy;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IPolicyService
    {
        Task<PolicyDto> GetPolicyByIdAsync(Guid id);
        Task<PagedResult<PolicyListItemDto>> GetAllPoliciesAsync(
            int pageNumber,
            int pageSize,
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateOnly? startDate,
            DateOnly? endDate);
        Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto request);
        Task<PolicyDto> ActivatePolicyAsync(Guid id);
        Task<PolicyDto> CancelPolicyAsync(Guid id, CancelPolicyDto request);
        Task<PolicyDto> ExpirePolicyAsync(Guid id);
    }
}
