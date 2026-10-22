using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees.Commands;

public record UpdateFeeCommand(
    Guid FeeId,
    string Name,
    FeeType Type,
    decimal Percentage,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive
) : IFeeDetailsCommand;
