namespace InsuranceManagement.Application.DTOs.Geography;

public class CountyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
}
