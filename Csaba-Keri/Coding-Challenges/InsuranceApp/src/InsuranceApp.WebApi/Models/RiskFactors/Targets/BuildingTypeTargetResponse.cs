using InsuranceApp.WebApi.Models.Buildings;

namespace InsuranceApp.WebApi.Models.RiskFactors.Targets;

public record BuildingTypeTargetResponse(
    BuildingTypeDto BuildingType
) : RiskTargetResponse;
