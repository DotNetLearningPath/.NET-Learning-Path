using FluentValidation;
using SupportAPI.Application.Queries;

namespace SupportAPI.Application.Validations
{
    public class GetUserRequestByIdQueryValidator : AbstractValidator<GetUserRequestByIdQuery>
    {
        public GetUserRequestByIdQueryValidator()
        {
            RuleFor(x => x.RequestId).NotEmpty().WithMessage("RequestId required");
        }
    }
}
