namespace InsuranceApp.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    NotAuthenticated,
    NotAuthorized
}

public sealed record Error(string Code, string Description, ErrorType Type);
