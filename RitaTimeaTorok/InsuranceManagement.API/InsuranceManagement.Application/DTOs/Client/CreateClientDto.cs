using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Client
{
    public class CreateClientDto
    {
        [EnumDataType(typeof(ClientType))]
        public required ClientType Type { get; set; }
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }
        [Required]
        [MaxLength(20)]
        public required string IdentificationNumber { get; set; }
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
