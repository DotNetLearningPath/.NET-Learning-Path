using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class Client
{
    public int ClientId { get; set; }
    public ClientType ClientType { get; set; }
    public string Name { get; set; } = null!;
    public string IdentificationNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}