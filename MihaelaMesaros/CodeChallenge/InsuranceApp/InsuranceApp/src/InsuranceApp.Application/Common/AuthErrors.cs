namespace InsuranceApp.Application.Common;

public static class AuthErrors
{
    public static readonly Error InvalidUsername = new("Auth.InvalidUsername", "Username is required.", ErrorType.Validation);
    public static readonly Error InvalidPassword = new("Auth.InvalidPassword", "Password is required.", ErrorType.Validation);
    public static readonly Error NotAuthenticated = new("Auth.NotAuthenticated", "The authentication failed.", ErrorType.NotAuthenticated);
    public static readonly Error UserNotFoundByUsername = new("Auth.UserNotFoundByUsername", "The user with this username doesn't exist.", ErrorType.NotAuthenticated);
    public static readonly Error UserInactive = new("Auth.UserInactive", "The user is not active.", ErrorType.NotAuthorized);
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "The username or password is incorrect.", ErrorType.NotAuthorized);
}
