namespace InsuranceApp.WebApi.Models.RiskFactors.Targets;

public record CountyTargetResponse(
    Guid CountyId
) : RiskTargetResponse;
