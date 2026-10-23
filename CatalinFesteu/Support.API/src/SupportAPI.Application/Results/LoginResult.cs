namespace SupportAPI.Application.Results
{
    public abstract record LoginResult;

    public sealed record LoginSucceeded(
        string AccessToken,
        DateTimeOffset ExpiresAtUtc) : LoginResult;

    public sealed record InvalidCredentials : LoginResult;
}