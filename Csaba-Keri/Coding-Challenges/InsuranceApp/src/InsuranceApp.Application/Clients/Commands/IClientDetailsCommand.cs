namespace InsuranceApp.Application.Clients.Commands;

public interface IClientDetailsCommand
{
    string? Name { get; }
    string? Email { get; }
    string? Phone { get; }
    string? PrimaryAddress { get; }
}
