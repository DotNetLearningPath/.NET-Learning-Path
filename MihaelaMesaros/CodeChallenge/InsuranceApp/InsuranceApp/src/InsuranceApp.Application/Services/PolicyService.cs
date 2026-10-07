using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Services;

public sealed class PolicyService(IPolicyRepository policyRepository) : IPolicyService
{
    public async Task<Result<PagedResult<PolicyDto>>> SearchPoliciesAsync(PolicySearchDto policySearchDto, CancellationToken cancellationToken)
    {
        if (policySearchDto.PageNumber is < CommonConstraints.MinPageNumber or > CommonConstraints.MaxPageNumber)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidPageNumber);
        }

        if (policySearchDto.PageSize is < CommonConstraints.MinPageSize or > CommonConstraints.MaxPageSize)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidPageSize);
        }

        if (policySearchDto.Status.HasValue && !Enum.IsDefined(policySearchDto.Status.Value))
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidStatus);
        }

        if (policySearchDto.StartDate.HasValue 
            && policySearchDto.EndDate.HasValue 
            && policySearchDto.StartDate.Value > policySearchDto.EndDate.Value)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidDateRange);
        }

        var (policies, totalCount) = await policyRepository.SearchPoliciesAsync(policySearchDto, cancellationToken);

        var items = policies.Select(MapPolicyToDto).ToList();

        var pagedResult = new PagedResult<PolicyDto>(
            items,
            policySearchDto.PageNumber,
            policySearchDto.PageSize,
            totalCount);

        return Result<PagedResult<PolicyDto>>.Success(pagedResult);
    }

    public async Task<Result<PolicyDto>> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InvalidPolicyId);
        }

        var policy = await policyRepository.GetPolicyByIdAsync(policyId, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.NotFound(policyId));
        }

        return Result<PolicyDto>.Success(MapPolicyToDto(policy));
    }

    private static PolicyDto MapPolicyToDto(Policy policy)
    {
        return new PolicyDto(
            policy.PolicyId,
            policy.PolicyNumber,
            policy.ClientId,
            policy.BuildingId,
            policy.BrokerId,
            policy.CurrencyId,
            policy.Status,
            policy.StartDate,
            policy.EndDate,
            policy.BasePremium,
            policy.FinalPremium);
    }
}
