namespace Application.DTO.Geography;

public class CityDto
{
    public Guid Id { get; set; }
    public Guid CountyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}