namespace InsuranceApp.WebApi.Models.Authentication;

public class LoginResponse(string accessToken, DateTimeOffset expiresAt)
{
    public string AccessToken { get; } = accessToken;
    public string TokenType { get; } = "Bearer";
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}
