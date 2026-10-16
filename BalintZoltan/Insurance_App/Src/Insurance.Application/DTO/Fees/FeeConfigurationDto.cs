using Insurance.Domain.Enums;
namespace Insurance.Application.DTO.Fees;

public sealed class FeeConfigurationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public FeeType Type { get; init; }
    public decimal Percentage { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}
