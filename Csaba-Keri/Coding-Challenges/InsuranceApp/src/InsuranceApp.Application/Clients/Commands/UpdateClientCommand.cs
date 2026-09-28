namespace InsuranceApp.Application.Clients.Commands;

public record UpdateClientCommand(
    Guid ClientId,
    string? Name,
    string? Email,
    string? Phone,
    string? PrimaryAddress
) : IClientDetailsCommand;
