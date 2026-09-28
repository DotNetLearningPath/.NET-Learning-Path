using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Clients;

public record CreateClientRequest(
    [Required]
    ClientTypeDto? Type,

    [Required]
    string? IdentificationNumber,

    [Required]
    string? Name,

    [Required]
    string? Email,

    [Required]
    string? Phone,

    string? PrimaryAddress
);
