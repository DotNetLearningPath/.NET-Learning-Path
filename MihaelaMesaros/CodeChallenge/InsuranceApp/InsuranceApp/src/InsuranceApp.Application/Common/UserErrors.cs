namespace InsuranceApp.Application.Common;

public static class UserErrors
{
    public static readonly Error UsernameRequired = new("User.UsernameRequired", "Username is required.", ErrorType.Validation);

    public static readonly Error InvalidUsernameLength = new("User.InvalidUsernameLength", "Username must contain between 3 and 50 characters.", ErrorType.Validation);

    public static readonly Error PasswordRequired = new("User.PasswordRequired", "Password is required.", ErrorType.Validation);

    public static readonly Error InvalidPasswordLength = new("User.InvalidPasswordLength", "Password must contain between 8 and 128 characters.", ErrorType.Validation);

    public static readonly Error InvalidRole = new("User.InvalidRole", "Invalid user role.", ErrorType.Validation);

    public static readonly Error BrokerIdRequired = new("User.BrokerIdRequired", "BrokerId is required for users with the Broker role.", ErrorType.Validation);

    public static readonly Error BrokerIdNotAllowed = new("User.BrokerIdNotAllowed", "BrokerId must be null for users with the Admin role.", ErrorType.Validation);

    public static readonly Error DuplicateUsername = new("User.DuplicateUsername", "Username already exists.", ErrorType.Conflict);

    public static readonly Error BrokerNotFound = new("User.BrokerNotFound", "Broker was not found.", ErrorType.NotFound);

    public static readonly Error BrokerInactive = new("User.BrokerInactive", "Cannot create a user for an inactive broker.", ErrorType.Validation);
}
