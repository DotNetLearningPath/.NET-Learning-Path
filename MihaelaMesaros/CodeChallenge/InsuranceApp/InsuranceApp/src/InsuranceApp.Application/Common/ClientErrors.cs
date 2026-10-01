using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class ClientErrors
{
    public static readonly Error InvalidClientId = new(
        "Client.InvalidClientId",
        "Client ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error NameRequired = new(
        "Client.NameRequired",
        "Client name is required.",
        ErrorType.Validation);

    public static readonly Error InvalidNameLength = new(
        "Client.InvalidNameLength",
        $"Client name must have between {ClientConstraints.NameMinLength} and {ClientConstraints.NameMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error IdentificationNumberRequired = new(
        "Client.IdentificationNumberRequired",
        "Identification number is required.",
        ErrorType.Validation);

    public static readonly Error InvalidIdentificationNumberLength = new(
        "Client.InvalidIdentificationNumberLength",
        $"Identification number must have between {ClientConstraints.IdentificationNumberMinLength} and {ClientConstraints.IdentificationNumberMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPhoneLength = new(
        "Client.InvalidPhoneLength",
        $"Phone number must not exceed {ClientConstraints.PhoneMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidAddressLength = new(
        "Client.InvalidAddressLength",
        $"Address must not exceed {ClientConstraints.AddressMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidClientType = new(
        "Client.InvalidClientType",
        "Client type is invalid.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "Client.InvalidEmail",
        $"Email address must be valid and must not exceed {ClientConstraints.EmailMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error DuplicateIdentificationNumber = new(
        "Client.DuplicateIdentificationNumber",
        "A client with this identification number already exists.",
        ErrorType.Conflict);

    public static Error NotFound(Guid clientId) => new(
        "Client.NotFound",
        $"Client with ID {clientId} was not found.",
        ErrorType.NotFound);

    public static readonly Error InvalidPageNumber = new(
        "Client.InvalidPageNumber",
        $"Page number must be in range {CommonConstraints.MinPageNumber} - {CommonConstraints.MaxPageNumber}.",
        ErrorType.Validation);

    public static readonly Error InvalidPageSize = new(
        "Client.InvalidPageSize",
        $"Page size must be in range {CommonConstraints.MinPageSize} - {CommonConstraints.MaxPageSize}.",
        ErrorType.Validation);
}