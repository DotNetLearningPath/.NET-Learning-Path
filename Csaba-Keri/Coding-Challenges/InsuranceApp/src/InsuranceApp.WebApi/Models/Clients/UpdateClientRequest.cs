using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Clients;

public record UpdateClientRequest(
    [Required]
    string? Name,

    [Required]
    string? Email,

    [Required]
    string? Phone,

    string? PrimaryAddress
);
