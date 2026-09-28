using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class ClientEntity(
    Guid id,
    ClientType type,
    string identificationNumber,
    string name,
    string email,
    string phone,
    string? primaryAddress
)
{
    public Guid Id { get; private set; } = id;
    public ClientType Type { get; private set; } = type;
    public string IdentificationNumber { get; private set; } = identificationNumber;
    public string Name { get; private set; } = name;
    public string Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;
    public string? PrimaryAddress { get; private set; } = primaryAddress;
}
