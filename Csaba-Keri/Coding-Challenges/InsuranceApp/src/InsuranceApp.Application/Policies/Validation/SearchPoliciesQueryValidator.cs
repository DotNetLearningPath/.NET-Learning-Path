using FluentValidation;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Policies.Queries;

namespace InsuranceApp.Application.Policies.Validation;

public class SearchPoliciesQueryValidator : PaginationValidator<SearchPoliciesQuery>
{
    public SearchPoliciesQueryValidator()
    {
        RuleFor(query => query.ClientId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Client identifier must not be empty when specified.");
        
        RuleFor(query => query.BrokerId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Broker identifier must not be empty when specified.");
        
        RuleFor(query => query.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value))
            .WithMessage("Policy status is invalid.");
        
        RuleFor(query => query.StartDate)
            .Must(date => date is null || date != default(DateOnly))
            .WithMessage("Start date must be a valid non-default date.");
        
        RuleFor(query => query.EndDate)
            .Must(date => date is null || date != default(DateOnly))
            .WithMessage("End date must be a valid non-default date.");
        
        RuleFor(query => query.EndDate)
            .Must((query, end) => query.StartDate is null || end is null || end >= query.StartDate)
            .WithMessage("End date must be on or after start date.");
    }
}
