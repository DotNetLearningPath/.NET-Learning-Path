using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Policies;
using Insurance.Domain.Enums;

namespace Insurance.Application.Abstractions;

public interface IPolicyService
{
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken);
    Task<PolicyDto?> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<PolicyDto>> SearchPolicyAsync(
        string? policyNumber,
        Guid? clientId,
        Guid? brokerId,
        PolicyStatus? status,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        PaginationRequest pagination,
        CancellationToken cancellationToken);
    Task<PolicyDto> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken);
    Task<PolicyDto> CancelPolicyAsync(Guid id, CancelPolicyRequest request, CancellationToken cancellationToken);
}
