using FluentValidation;
using InsuranceApp.Application.Authentication.Commands;

namespace InsuranceApp.Application.Authentication.Validation;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
