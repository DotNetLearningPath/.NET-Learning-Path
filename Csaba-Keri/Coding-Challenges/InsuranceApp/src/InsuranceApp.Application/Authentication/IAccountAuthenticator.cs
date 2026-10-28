using InsuranceApp.Application.Authentication.Results;

namespace InsuranceApp.Application.Authentication;

public interface IAccountAuthenticator
{
    Task<AuthenticatedAccount> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
}
