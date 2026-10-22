using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class BrokerErrors
{
    public static readonly Error InvalidBrokerId = new(
        "Broker.InvalidBrokerId",
        "Broker ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error BrokerCodeRequired = new(
        "Broker.BrokerCodeRequired",
        "Broker code is required.",
        ErrorType.Validation);

    public static readonly Error InvalidBrokerCodeLength = new(
        "Broker.InvalidBrokerCodeLength",
        $"Broker code must have between {BrokerConstraints.BrokerCodeMinLength} and {BrokerConstraints.BrokerCodeMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error NameRequired = new(
        "Broker.NameRequired",
        "Broker name is required.",
        ErrorType.Validation);

    public static readonly Error InvalidNameLength = new(
        "Broker.InvalidNameLength",
        $"Broker name must have between {BrokerConstraints.NameMinLength} and {BrokerConstraints.NameMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "Broker.InvalidEmail",
        $"Email address must be valid and must not exceed {BrokerConstraints.EmailMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPhoneLength = new(
        "Broker.InvalidPhoneLength",
        $"Phone number must not exceed {BrokerConstraints.PhoneMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCommissionPercentage = new(
        "Broker.InvalidCommissionPercentage",
        $"Commission percentage must be between {BrokerConstraints.MinCommissionPercentage} and {BrokerConstraints.MaxCommissionPercentage}.",
        ErrorType.Validation);

    public static readonly Error InvalidCommissionPercentageScale = new(
        "Broker.InvalidCommissionPercentageScale",
        $"Commission percentage must have at most {BrokerConstraints.CommissionPercentageScale} decimal places.",
        ErrorType.Validation);

    public static Error DuplicateBrokerCode(string brokerCode) => new(
        "Broker.DuplicateBrokerCode",
        $"A broker with broker code '{brokerCode}' already exists.",
        ErrorType.Conflict);

    public static Error NotFound(Guid brokerId) => new(
        "Broker.NotFound",
        $"Broker with ID {brokerId} was not found.",
        ErrorType.NotFound);
}
