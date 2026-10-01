namespace InsuranceApp.Application.Brokers.Commands;

public record UpdateBrokerCommand(
    Guid BrokerId,
    string Name,
    string Email,
    string Phone
) : IBrokerDetailsCommand;
