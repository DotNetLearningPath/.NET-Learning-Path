namespace InsuranceApp.Application.Common;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Username and password are required.", ErrorType.Validation);
    public static readonly Error NotAuthenticated = new("Auth.NotAuthenticated", "The authentication failed.", ErrorType.NotAuthenticated);
    public static readonly Error NotAuthorized = new("Auth.NotAuthorized", "The user is not authorized for this action.", ErrorType.NotAuthorized);
}
