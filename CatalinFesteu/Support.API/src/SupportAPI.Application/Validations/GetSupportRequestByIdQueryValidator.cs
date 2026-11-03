using FluentValidation;
using SupportAPI.Application.Queries;

namespace SupportAPI.Application.Validations
{
    public class GetSupportRequestByIdQueryValidator : AbstractValidator<GetSupportRequestByIdQuery>
    {
        public GetSupportRequestByIdQueryValidator()
        {
            RuleFor(x => x.RequestId).NotEmpty().WithMessage("RequestId required");
        }
    }
}
