using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees.Commands;

public interface IFeeDetailsCommand
{
    string Name { get; }
    FeeType Type { get; }
    decimal Percentage { get; }
    DateOnly EffectiveFrom { get; }
    DateOnly? EffectiveTo { get; }
    bool IsActive { get; }
}
