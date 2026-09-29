using Domain.Enums;

namespace Domain.Entities;

public class FeeConfiguration
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public FeeType Type { get; private set; }
    public decimal Percentage { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    private FeeConfiguration()
    {
        Name = null!;
    }

    public FeeConfiguration(string name, FeeType type, decimal percentage,
        DateTime effectiveFrom, DateTime? effectiveTo = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Fee name is required.", nameof(name));
        if (!Enum.IsDefined(type)) throw new ArgumentException("Fee type is not valid.", nameof(type));
        if (percentage < 0) throw new ArgumentOutOfRangeException(nameof(percentage));
        if (type == FeeType.Percentage && percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        if (effectiveTo.HasValue && effectiveTo < effectiveFrom) throw new ArgumentException("Effective end cannot precede effective start.", nameof(effectiveTo));

        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Percentage = percentage;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsActive = isActive;
    }

    public void Update(string name, FeeType type, decimal percentage,
        DateTime effectiveFrom, DateTime? effectiveTo)
    {
        Validate(name, type, percentage, effectiveFrom, effectiveTo);
        Name = name;
        Type = type;
        Percentage = percentage;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static void Validate(string name, FeeType type, decimal percentage,
        DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Fee name is required.", nameof(name));
        if (!Enum.IsDefined(type)) throw new ArgumentException("Fee type is not valid.", nameof(type));
        if (percentage < 0) throw new ArgumentOutOfRangeException(nameof(percentage));
        if (type == FeeType.Percentage && percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        if (effectiveTo.HasValue && effectiveTo < effectiveFrom)
            throw new ArgumentException("Effective end cannot precede effective start.", nameof(effectiveTo));
    }
}
