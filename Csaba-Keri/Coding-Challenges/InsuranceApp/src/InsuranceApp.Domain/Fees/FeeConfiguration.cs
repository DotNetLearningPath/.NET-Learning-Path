namespace InsuranceApp.Domain.Fees;

public class FeeConfiguration
{
    public Guid Id { get; }

    public string Name { get; private set; }
    public FeeType Type { get; private set; }
    public decimal Percentage { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    public FeeConfiguration(
        Guid id,
        string name,
        FeeType type,
        decimal percentage,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isActive
    )
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Fee identifier must not be empty.",
                nameof(id)
            );
        }

        Id = id;
        Name = string.Empty;

        UpdateDetails(name, type, percentage, effectiveFrom, effectiveTo, isActive);
    }

    public void UpdateDetails(
        string name,
        FeeType type,
        decimal percentage,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isActive
    )
    {
        if (!FeeRules.IsValidName(name))
        {
            throw new ArgumentException(
                $"Fee name is required and must not exceed {FeeRules.MaxNameLength} characters.",
                nameof(name)
            );
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Fee type is invalid."
            );
        }

        if (!FeeRules.IsValidPercentage(percentage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                percentage,
                $"Percentage must be between {FeeRules.MinPercentage} and {FeeRules.MaxPercentage}" +
                $" with at most {FeeRules.PercentageScale} decimal places."
            );
        }

        if (!FeeRules.IsValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new ArgumentException(
                "A start date is required and the end date must not precede it.",
                nameof(effectiveFrom)
            );
        }

        Name = name.Trim();
        Type = type;
        Percentage = percentage;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsActive = isActive;
    }

    public bool IsApplicableOn(DateOnly date)
    {
        return IsActive
            && EffectiveFrom <= date
            && (!EffectiveTo.HasValue || EffectiveTo.Value >= date);
    }
}
