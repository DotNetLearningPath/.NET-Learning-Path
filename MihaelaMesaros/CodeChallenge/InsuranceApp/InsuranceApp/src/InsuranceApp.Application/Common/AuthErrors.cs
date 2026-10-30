namespace InsuranceApp.Application.Common;

public static class AuthErrors
{
    public static readonly Error InvalidUsername = new("Auth.InvalidUsername", "Username is required.", ErrorType.Validation);
    public static readonly Error InvalidPassword = new("Auth.InvalidPassword", "Password is required.", ErrorType.Validation);
    public static readonly Error NotAuthenticated = new("Auth.NotAuthenticated", "The authentication failed.", ErrorType.NotAuthenticated);
    public static readonly Error NotAuthorized = new("Auth.NotAuthorized", "The user is not authorized for this action.", ErrorType.NotAuthorized);
}
