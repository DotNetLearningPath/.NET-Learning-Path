namespace InsuranceApp.WebApi.Models.RiskFactors.Targets;

public record CountryTargetResponse(
    Guid CountryId
) : RiskTargetResponse;
