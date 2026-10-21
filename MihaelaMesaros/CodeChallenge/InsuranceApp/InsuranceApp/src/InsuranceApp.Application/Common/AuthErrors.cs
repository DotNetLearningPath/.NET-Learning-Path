namespace InsuranceApp.Application.Common;

public static class AuthErrors
{
    public static readonly Error InvalidUsername = new("Auth.InvalidUsername", "Username is required.", ErrorType.Validation);
    public static readonly Error InvalidPassword = new("Auth.InvalidPassword", "Password is required.", ErrorType.Validation);
    public static readonly Error UserNotFoundByUsername = new("Auth.UserNotFoundByUsername", "The user with this username doesn't exist.", ErrorType.Validation);
}
