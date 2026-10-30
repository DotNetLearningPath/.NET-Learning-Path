namespace InsuranceManagement.Application.DTOs.Geography;

public class CreateCountyDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
}
