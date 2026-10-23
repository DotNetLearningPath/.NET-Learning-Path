using System.ComponentModel.DataAnnotations;

namespace SupportAPI.Api.Requests
{
    public sealed record LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
