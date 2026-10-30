namespace InsuranceManagement.Application.DTOs.Geography;

public class UpdateCountyDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
}
