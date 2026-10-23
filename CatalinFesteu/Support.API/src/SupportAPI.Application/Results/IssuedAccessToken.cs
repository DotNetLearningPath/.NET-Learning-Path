namespace SupportAPI.Application.Results
{
    public sealed record IssuedAccessToken(string AccessToken, DateTimeOffset ExpiresAt);
}