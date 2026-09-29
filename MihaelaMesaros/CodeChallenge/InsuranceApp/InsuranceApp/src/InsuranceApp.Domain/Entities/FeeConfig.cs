using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class FeeConfig
{
    public Guid FeeConfigId { get; set; }

    public string Name { get; set; } = null!;

    public FeeType FeeType { get; set; }

    public decimal Percentage { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
}