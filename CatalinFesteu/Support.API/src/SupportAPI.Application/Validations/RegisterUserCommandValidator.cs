using FluentValidation;
using SupportAPI.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportAPI.Application.Validations
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email required");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MaximumLength(20).WithMessage("Password required and under 20 characters");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200).WithMessage("Name required and under 200 characters");
        }
    }
}
