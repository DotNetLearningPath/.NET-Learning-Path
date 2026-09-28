using FluentValidation;

namespace InsuranceApp.Application.Common.Pagination;

public abstract class PaginationValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : PageQuery
{
    protected PaginationValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PaginationDefaults.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PaginationDefaults.MaxPageSize}.");
    }
}
