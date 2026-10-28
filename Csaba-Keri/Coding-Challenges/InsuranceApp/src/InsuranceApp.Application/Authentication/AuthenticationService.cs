using FluentValidation;
using InsuranceApp.Application.Authentication.Commands;
using InsuranceApp.Application.Authentication.Results;

namespace InsuranceApp.Application.Authentication;

public class AuthenticationService(
    IAccountAuthenticator accountAuthenticator,
    IAccessTokenGenerator accessTokenGenerator,
    IValidator<LoginCommand> loginValidator
) : IAuthenticationService
{
    private readonly IAccountAuthenticator _accountAuthenticator = accountAuthenticator;
    private readonly IAccessTokenGenerator _accessTokenGenerator = accessTokenGenerator;
    private readonly IValidator<LoginCommand> _loginValidator = loginValidator;

    public async Task<AccessTokenResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _loginValidator.ValidateAndThrowAsync(command, cancellationToken);

        var authenticatedAccount = await _accountAuthenticator.AuthenticateAsync(
            email: command.Email.Trim(),
            password: command.Password,
            cancellationToken: cancellationToken
        );

        cancellationToken.ThrowIfCancellationRequested();

        return _accessTokenGenerator.Generate(authenticatedAccount);
    }
}
