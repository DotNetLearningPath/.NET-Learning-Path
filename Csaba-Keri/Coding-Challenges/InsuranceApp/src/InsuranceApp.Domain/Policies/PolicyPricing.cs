namespace InsuranceApp.Domain.Policies;

public static class PolicyPricing
{
    public static decimal CalculateFinalPremium(
        decimal basePremium,
        IEnumerable<AppliedAdjustment> adjustments
    )
    {
        ArgumentNullException.ThrowIfNull(adjustments);

        if (!PolicyRules.IsValidPremium(basePremium))
        {
            throw new ArgumentOutOfRangeException(
                nameof(basePremium),
                basePremium,
                $"Base premium must be greater than 0, at most {PolicyRules.MaxPremium}" +
                $", and have at most {PolicyRules.MoneyScale} decimal places."
            );
        }

        var totalPercentage = adjustments.Sum(adjustment => adjustment.Percentage);

        var finalPremium = basePremium * (1m + totalPercentage / 100m);

        return decimal.Round(finalPremium, PolicyRules.MoneyScale, MidpointRounding.AwayFromZero);
    }
}
