namespace InsuranceApp.Domain.Entities
{
    public sealed class County
    {
        public Guid CountyId { get; set; }
        public required string Name { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; } = null!;
    }
}
