namespace SupportAPI.Application.Results
{
    public abstract record RegisterUserResult;

    public sealed record RegistrationSucceeded(Guid UserId) : RegisterUserResult;

    public sealed record RegistrationFailed(IEnumerable<string> Errors) : RegisterUserResult;

}