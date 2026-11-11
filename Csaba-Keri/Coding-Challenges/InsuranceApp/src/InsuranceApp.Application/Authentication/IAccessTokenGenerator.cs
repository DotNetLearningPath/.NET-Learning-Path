using InsuranceApp.Application.Authentication.Results;

namespace InsuranceApp.Application.Authentication;

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(AuthenticatedAccount account);
}
