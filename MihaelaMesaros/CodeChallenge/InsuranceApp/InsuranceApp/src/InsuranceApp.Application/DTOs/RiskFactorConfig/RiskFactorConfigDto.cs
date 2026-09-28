using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.RiskFactorConfig;

public sealed record RiskFactorConfigDto(
    Guid RiskFactorConfigId,
    RiskFactorLevel Level,
    Guid ReferenceId,
    decimal AdjustmentPercentage,
    bool IsActive
);
