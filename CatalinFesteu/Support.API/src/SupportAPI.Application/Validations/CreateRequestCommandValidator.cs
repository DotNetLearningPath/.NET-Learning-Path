using FluentValidation;
using SupportAPI.Application.Commands;

namespace SupportAPI.Application.Validations
{
    public class CreateRequestCommandValidator : AbstractValidator<CreateUserRequestCommand>
    {
        public CreateRequestCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Request title is required and must not surpass 200 characters");
            
            RuleFor(x => x.RequestContent)
                .NotEmpty()
                .MaximumLength(3000)
                .WithMessage("Request content is required and has a maximum length of 3000 characters");
        }
    }
}
