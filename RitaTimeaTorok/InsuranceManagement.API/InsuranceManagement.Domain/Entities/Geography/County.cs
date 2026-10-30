namespace InsuranceManagement.Domain.Entities.Geography
{
    public class County
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; } 

        public Guid CountryId { get; set; }

        public Country Country { get; set; } = null!;

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
