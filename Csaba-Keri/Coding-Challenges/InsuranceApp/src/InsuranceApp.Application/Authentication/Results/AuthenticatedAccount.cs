namespace InsuranceApp.Application.Authentication.Results;

public record AuthenticatedAccount(
    Guid UserId,
    string Role,
    Guid? BrokerId
);
