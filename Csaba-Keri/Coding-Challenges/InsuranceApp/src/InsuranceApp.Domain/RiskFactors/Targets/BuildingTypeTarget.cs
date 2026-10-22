using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Domain.RiskFactors.Targets;

public record BuildingTypeTarget : RiskTarget
{
    public BuildingType Type { get; }
    public override RiskFactorLevel Level => RiskFactorLevel.BuildingType;

    public BuildingTypeTarget(BuildingType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown building type."
            );
        }

        Type = type;
    }
}
