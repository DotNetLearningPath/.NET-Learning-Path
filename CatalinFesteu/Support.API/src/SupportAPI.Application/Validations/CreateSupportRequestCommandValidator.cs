using FluentValidation;
using SupportAPI.Application.Commands;

namespace SupportAPI.Application.Validations
{
    public class CreateSupportRequestCommandValidator : AbstractValidator<CreateSupportRequestCommand>
    {
        public CreateSupportRequestCommandValidator()
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
