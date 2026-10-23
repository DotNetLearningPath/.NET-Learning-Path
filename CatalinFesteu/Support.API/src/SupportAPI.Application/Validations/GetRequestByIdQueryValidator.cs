using FluentValidation;
using SupportAPI.Application.Queries;

namespace SupportAPI.Application.Validations
{
    public class GetRequestByIdQueryValidator : AbstractValidator<GetRequestByIdQuery>
    {
        public GetRequestByIdQueryValidator()
        {
            RuleFor(x => x.RequestId).NotEmpty().WithMessage("RequestId required");
        }
    }
}
