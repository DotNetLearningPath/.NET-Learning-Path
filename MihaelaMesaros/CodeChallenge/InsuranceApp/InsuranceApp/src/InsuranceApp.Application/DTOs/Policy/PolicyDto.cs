using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Policy;

public sealed record PolicyDto(
    Guid PolicyId,
    string PolicyNumber,
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    PolicyStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    decimal BasePremium,
    decimal FinalPremium);
