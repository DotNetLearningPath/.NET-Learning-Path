namespace InsuranceApp.Application.Authentication.Results;

public class AccessTokenResult(string accessToken, DateTimeOffset expiresAt)
{
    public string AccessToken { get; } = accessToken;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}
