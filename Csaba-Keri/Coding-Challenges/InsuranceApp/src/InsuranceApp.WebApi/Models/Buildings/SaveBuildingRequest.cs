using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Buildings;

public record SaveBuildingRequest(
    [Required]
    BuildingTypeDto? Type,

    [Required]
    BuildingAddressRequest? Address,

    [Required]
    int? ConstructionYear,

    [Required]
    int? NumberOfFloors,

    [Required]
    decimal? SurfaceArea,

    [Required]
    decimal? InsuredValue
);
