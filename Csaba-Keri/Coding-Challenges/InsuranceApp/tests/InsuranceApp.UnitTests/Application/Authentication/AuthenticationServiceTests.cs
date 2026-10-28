using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Authentication;
using InsuranceApp.Application.Authentication.Commands;
using InsuranceApp.Application.Authentication.Exceptions;
using InsuranceApp.Application.Authentication.Results;
using InsuranceApp.Application.Common.Security;
using InsuranceApp.UnitTests.Common;
using NSubstitute;

namespace InsuranceApp.UnitTests.Application.Authentication;

public sealed class AuthenticationServiceTests
{
    private readonly IAccountAuthenticator _accountAuthenticator;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IValidator<LoginCommand> _loginValidator;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _accountAuthenticator = Substitute.For<IAccountAuthenticator>();
        _accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();
        _loginValidator = ValidatorMocks.CreatePassing<LoginCommand>();
        _authenticationService = new(_accountAuthenticator, _accessTokenGenerator, _loginValidator);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenForAuthenticatedAccount()
    {
        // Arrange
        var email = "admin@example.test";
        var command = new LoginCommand($" {email} ", "Password123! ");
        var account = new AuthenticatedAccount(Guid.NewGuid(), ApplicationRoles.Admin, null);
        var expected = new AccessTokenResult("test-token", DateTimeOffset.UtcNow.AddMinutes(15));

        _accountAuthenticator.AuthenticateAsync(email, command.Password, CancellationToken.None)
            .Returns(account);
        
        _accessTokenGenerator.Generate(account)
            .Returns(expected);

        // Act
        var result = await _authenticationService.LoginAsync(command, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);

        await _loginValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _accountAuthenticator.Received(1).AuthenticateAsync(email, command.Password, CancellationToken.None);

        _accessTokenGenerator.Received(1).Generate(account);
    }

    [Fact]
    public async Task LoginAsync_ValidationFails_DoesNotAuthenticate()
    {
        // Arrange
        var command = new LoginCommand("", "");
        var exception = new ValidationException("Rejected by the validator mock.");

        _loginValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.LoginAsync(command, CancellationToken.None)
        );
        
        await _accountAuthenticator.DidNotReceive().AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        
        _accessTokenGenerator.DidNotReceive().Generate(Arg.Any<AuthenticatedAccount>());
    }

    [Fact]
    public async Task LoginAsync_RejectedAccount_DoesNotGenerateToken()
    {
        // Arrange
        var command = new LoginCommand("admin@example.test", "wrong-password");
        var exception = new InvalidCredentialsException();

        _accountAuthenticator.AuthenticateAsync(command.Email, command.Password, CancellationToken.None)
            .Returns(Task.FromException<AuthenticatedAccount>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _authenticationService.LoginAsync(command, CancellationToken.None)
        );

        _accessTokenGenerator.DidNotReceive().Generate(Arg.Any<AuthenticatedAccount>());
    }
}
