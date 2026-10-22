using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class FeeConfigurationEntity(
    Guid id,
    string name,
    FeeType type,
    decimal percentage,
    DateOnly effectiveFrom,
    DateOnly? effectiveTo,
    bool isActive
)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public FeeType Type { get; private set; } = type;
    public decimal Percentage { get; private set; } = percentage;
    public DateOnly EffectiveFrom { get; private set; } = effectiveFrom;
    public DateOnly? EffectiveTo { get; private set; } = effectiveTo;
    public bool IsActive { get; private set; } = isActive;
}
