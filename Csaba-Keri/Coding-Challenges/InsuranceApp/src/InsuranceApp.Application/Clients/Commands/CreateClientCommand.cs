using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Application.Clients.Commands;

public record CreateClientCommand(
    ClientType Type,
    string? IdentificationNumber,
    string? Name,
    string? Email,
    string? Phone,
    string? PrimaryAddress
) : IClientDetailsCommand;
