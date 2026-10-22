namespace InsuranceApp.Domain.Policies;

public static class PolicyPricing
{
    public static decimal CalculateFinalPremium(decimal basePremium, IEnumerable<AppliedAdjustment> adjustments)
    {
        ArgumentNullException.ThrowIfNull(adjustments);

        var totalPercentage = adjustments.Sum(adjustment => adjustment.Percentage);

        var finalPremium = basePremium * (1m + totalPercentage / 100m);

        var roundedPremium = decimal.Round(finalPremium, PolicyRules.MoneyScale, MidpointRounding.AwayFromZero);

        return Math.Max(roundedPremium, PolicyRules.MinFinalPremium);
    }
}
