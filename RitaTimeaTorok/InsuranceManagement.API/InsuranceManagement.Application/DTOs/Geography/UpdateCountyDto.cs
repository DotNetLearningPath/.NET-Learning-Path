using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography
{
    public class UpdateCountyDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public required Guid CountryId { get; set; }
    }
}
