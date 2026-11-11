using InsuranceManagement.Application.DTOs.Geography;

namespace InsuranceManagement.Application.DTOs.Building
{
    public class BuildingDetailsDto : BuildingDto
    {
        public CityDto City { get; set; } = null!;
        public CountyDto County { get; set; } = null!;
        public CountryDto Country { get; set; } = null!;
    }
}
