using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees.Results;

public record FeeResult(
    Guid Id,
    string Name,
    FeeType Type,
    decimal Percentage,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive
);
