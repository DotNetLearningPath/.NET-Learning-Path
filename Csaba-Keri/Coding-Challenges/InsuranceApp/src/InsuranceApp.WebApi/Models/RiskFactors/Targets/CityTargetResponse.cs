namespace InsuranceApp.WebApi.Models.RiskFactors.Targets;

public record CityTargetResponse(
    Guid CityId
) : RiskTargetResponse;
