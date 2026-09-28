using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace InsuranceApp.UnitTests.Common;

internal static class ValidatorMocks
{
    public static IValidator<T> CreatePassing<T>()
    {
        var validator = Substitute.For<IValidator<T>>();

        // ValidateAndThrowAsync calls this overload.
        validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        validator.ClearReceivedCalls();

        return validator;
    }
}
