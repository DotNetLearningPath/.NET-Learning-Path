using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.FeeConfig;

public sealed record FeeConfigDto(
    Guid FeeConfigId,
    string Name,
    FeeType FeeType,
    decimal Percentage,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive
);
