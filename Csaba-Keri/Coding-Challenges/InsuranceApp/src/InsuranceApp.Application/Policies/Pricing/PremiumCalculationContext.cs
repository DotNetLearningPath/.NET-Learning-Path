using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Policies.Pricing;

public record PremiumCalculationContext(
    Guid CityId,
    BuildingType BuildingType,
    DateOnly CalculationDate
);
