using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.RiskFactorConfig;

public sealed record CreateRiskFactorConfigDto(
    RiskFactorLevel Level,
    Guid ReferenceId,
    decimal AdjustmentPercentage,
    bool IsActive
);