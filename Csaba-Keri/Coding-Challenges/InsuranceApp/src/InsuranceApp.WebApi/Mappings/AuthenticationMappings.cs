using InsuranceApp.Application.Authentication.Commands;
using InsuranceApp.Application.Authentication.Results;
using InsuranceApp.WebApi.Models.Authentication;

namespace InsuranceApp.WebApi.Mappings;

internal static class AuthenticationMappings
{
    public static LoginCommand ToCommand(this LoginRequest request)
    {
        return new(
            email: request.Email,
            password: request.Password
        );
    }

    public static LoginResponse ToResponse(this AccessTokenResult result)
    {
        return new(
            accessToken: result.AccessToken,
            expiresAt: result.ExpiresAt
        );
    }
}
