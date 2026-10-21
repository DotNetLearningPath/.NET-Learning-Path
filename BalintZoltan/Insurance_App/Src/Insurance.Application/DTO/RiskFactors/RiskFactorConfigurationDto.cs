using Insurance.Domain.Enums;

namespace Insurance.Application.DTO.RiskFactors;

public sealed class RiskFactorConfigurationDto
{
    public Guid Id { get; init; }
    public RiskFactorLevel Level { get; init; }
    public string Reference { get; init; } = string.Empty;
    public decimal AdjustmentPercentage { get; init; }
    public bool IsActive { get; init; }
}
