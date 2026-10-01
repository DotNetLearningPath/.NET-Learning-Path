using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.RiskFactorConfig;

public sealed record UpdateRiskFactorConfigDto(
    RiskFactorLevel Level,
    Guid ReferenceId,
    decimal AdjustmentPercentage,
    bool IsActive
);