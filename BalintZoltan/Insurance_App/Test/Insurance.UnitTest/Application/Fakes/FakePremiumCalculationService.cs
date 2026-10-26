using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Premiums;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakePremiumCalculationService : IPremiumCalculationService
{
    public decimal Result { get; set; } = 100m;
    public PremiumCalculationContext? LastContext { get; private set; }

    public Task<decimal> CalculateFinalPremiumAsync(
        decimal basePremium,
        DateTime effectiveAt,
        PremiumCalculationContext context,
        CancellationToken cancellationToken)
    {
        LastContext = context;
        return Task.FromResult(Result);
    }
}
