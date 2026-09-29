using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Client;

public sealed record ClientDto(
    Guid ClientId,
    ClientType ClientType,
    string Name,
    string IdentificationNumber,
    string? Email,
    string? Phone,
    string? Address
);