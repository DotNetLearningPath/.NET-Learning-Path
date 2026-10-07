using InsuranceApp.Domain.RiskFactors.Targets;

namespace InsuranceApp.Application.RiskFactors.Results;

public record RiskFactorResult(
    Guid Id,
    RiskTarget Target,
    decimal AdjustmentPercentage,
    bool IsActive
);
