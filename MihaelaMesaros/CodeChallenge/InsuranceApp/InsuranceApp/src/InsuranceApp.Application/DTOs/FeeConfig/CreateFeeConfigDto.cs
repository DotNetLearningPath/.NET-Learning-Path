using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.FeeConfig;

public sealed record CreateFeeConfigDto(
    string Name,
    FeeType FeeType,
    decimal Percentage,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive
);
