namespace InsuranceApp.WebApi.Models.Fees;

public record FeeResponse(
    Guid Id,
    string Name,
    FeeTypeDto Type,
    decimal Percentage,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive
);
