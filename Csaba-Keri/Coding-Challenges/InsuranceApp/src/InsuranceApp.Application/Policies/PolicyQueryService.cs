using FluentValidation;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Policies.Queries;
using InsuranceApp.Application.Policies.Results;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies;

public class PolicyQueryService(
    IPolicyQueryRepository policyQueryRepository,
    IValidator<SearchPoliciesQuery> searchValidator
) : IPolicyQueryService
{
    private readonly IPolicyQueryRepository _policyQueryRepository = policyQueryRepository;
    private readonly IValidator<SearchPoliciesQuery> _searchValidator = searchValidator;

    public async Task<PagedResult<PolicyListItemResult>> SearchPoliciesAsync(SearchPoliciesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _searchValidator.ValidateAndThrowAsync(query, cancellationToken);

        return await _policyQueryRepository.SearchPoliciesAsync(query, cancellationToken);
    }

    public async Task<PolicyDetailsResult> GetPolicyDetailsAsync(Guid policyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (policyId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create(nameof(policyId), "Policy identifier must not be empty.");
        }

        return await _policyQueryRepository.GetPolicyDetailsAsync(policyId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Policy), policyId);
    }
}
