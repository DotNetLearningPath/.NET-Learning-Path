namespace InsuranceApp.Application.Brokers.Commands;

public interface IBrokerDetailsCommand
{
    string Name { get; }
    string Email { get; }
    string Phone { get; }
}
