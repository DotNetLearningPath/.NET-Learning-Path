using InsuranceApp.WebApi.Models.RiskFactors.Targets;

namespace InsuranceApp.WebApi.Models.RiskFactors;

public record RiskFactorResponse(
    Guid Id,
    RiskTargetResponse Target,
    decimal AdjustmentPercentage,
    bool IsActive
);
