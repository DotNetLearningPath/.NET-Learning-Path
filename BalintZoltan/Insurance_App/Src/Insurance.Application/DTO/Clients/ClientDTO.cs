using Domain.Enums;

namespace Application.DTO.Clients;

public class ClientDto
{
    public Guid Id { get; set; }
    public ClientType ClientType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
}