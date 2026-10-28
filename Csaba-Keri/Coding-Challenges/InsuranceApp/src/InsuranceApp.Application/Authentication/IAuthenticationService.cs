using InsuranceApp.Application.Authentication.Commands;
using InsuranceApp.Application.Authentication.Results;

namespace InsuranceApp.Application.Authentication;

public interface IAuthenticationService
{
    Task<AccessTokenResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}
