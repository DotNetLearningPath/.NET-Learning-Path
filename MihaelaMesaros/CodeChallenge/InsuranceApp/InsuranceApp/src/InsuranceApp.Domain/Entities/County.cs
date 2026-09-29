namespace InsuranceApp.Domain.Entities
{
    public sealed class County
    {
        public int CountyId { get; set; }
        public required string Name { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;
        public ICollection<City> Cities { get; set; } = [];
    }
}
