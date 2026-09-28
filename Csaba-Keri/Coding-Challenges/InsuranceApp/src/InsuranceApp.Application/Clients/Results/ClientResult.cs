using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Application.Clients.Results;

public record ClientResult(
    Guid Id,
    ClientType Type,
    string IdentificationNumber,
    string Name,
    string Email,
    string Phone,
    string? PrimaryAddress
)
{
    public Guid Id { get; } = Id;
    public ClientType Type { get; } = Type;
    public string IdentificationNumber { get; } = IdentificationNumber;
    public string Name { get; } = Name;
    public string Email { get; } = Email;
    public string Phone { get; } = Phone;
    public string? PrimaryAddress { get; } = PrimaryAddress;
}
