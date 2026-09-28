using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Buildings;

public record BuildingAddressRequest(
    [Required]
    Guid? CityId,

    [Required]
    string? Street,

    [Required]
    string? Number
);
