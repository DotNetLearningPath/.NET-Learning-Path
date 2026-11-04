using InsuranceManagement.Application.DTOs.Building;

namespace InsuranceManagement.Application.DTOs.Client
{
    public class ClientDetailsDto : ClientDto
    {
        public List<BuildingDto> Buildings { get; set; } = [];
    }
}
