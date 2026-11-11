using InsuranceApp.Application.Abstractions.Security;

namespace InsuranceApp.Api.Security;

internal sealed class RefreshTokenCookieService(IHttpContextAccessor httpContextAccessor) : IRefreshTokenCookieService
{
    private const string CookieName = "refreshToken";

    public string? GetRefreshTokenCookie()
    {
        return httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
    }

    public void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is unavailable.");

        context.Response.Cookies.Append(
            CookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(expiresAt),
                Path = "/api/auth"
            });
    }

    public void DeleteRefreshTokenCookie()
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is unavailable.");

        context.Response.Cookies.Delete(
            CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            });
    }
}