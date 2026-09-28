using FluentValidation;
using FluentValidation.Results;

namespace InsuranceApp.Application.Common.Validation;

public static class ValidationExceptionFactory
{
    public static ValidationException Create(string propertyName, string message)
    {
        return new ValidationException([
            new ValidationFailure(propertyName, message)
        ]);
    }
}
