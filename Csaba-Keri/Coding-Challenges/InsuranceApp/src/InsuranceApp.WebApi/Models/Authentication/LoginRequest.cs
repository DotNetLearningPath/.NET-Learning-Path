using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Authentication;

public class LoginRequest
{
    [Required]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
