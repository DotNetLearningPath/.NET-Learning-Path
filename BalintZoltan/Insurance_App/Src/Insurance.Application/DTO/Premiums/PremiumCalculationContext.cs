using Insurance.Domain.Enums;

namespace Insurance.Application.DTO.Premiums;

public sealed class PremiumCalculationContext
{
    public Guid? CountryId { get; init; }
    public Guid? CountyId { get; init; }
    public Guid? CityId { get; init; }
    public BuildingType? BuildingType { get; init; }
}
