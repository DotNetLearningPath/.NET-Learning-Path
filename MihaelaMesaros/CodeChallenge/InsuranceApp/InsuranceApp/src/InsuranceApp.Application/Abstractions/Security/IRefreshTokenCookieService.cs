namespace InsuranceApp.Application.Abstractions.Security;

public interface IRefreshTokenCookieService
{
    void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt);
    string? GetRefreshTokenCookie();
    void DeleteRefreshTokenCookie();
}
