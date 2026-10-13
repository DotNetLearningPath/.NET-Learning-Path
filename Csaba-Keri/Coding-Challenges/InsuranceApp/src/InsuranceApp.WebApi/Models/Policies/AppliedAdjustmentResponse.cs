namespace InsuranceApp.WebApi.Models.Policies;

public record AppliedAdjustmentResponse(
    AdjustmentSourceDto Source,
    Guid ConfigurationId,
    decimal Percentage
);
