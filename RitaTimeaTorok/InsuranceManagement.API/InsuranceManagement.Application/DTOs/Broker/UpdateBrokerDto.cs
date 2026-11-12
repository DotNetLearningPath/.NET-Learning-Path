using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Broker
{
    public class UpdateBrokerDto
    {
        [Required]
        [MaxLength(30)]
        public required string BrokerCode { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public required string Email { get; set; }

        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EnumDataType(typeof(BrokerStatus))]
        public BrokerStatus Status { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal? CommissionPercentage { get; set; }
    }
}
