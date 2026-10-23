using SupportAPI.Application;
using System.ComponentModel.DataAnnotations;

namespace SupportAPI.Api.Requests
{
    public sealed record RegisterUserRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; init; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required, MaxLength(20)]
        public string Password { get; init; } = string.Empty;
    }
}