namespace InsuranceManagement.Domain.Entities.Geography
{
    public class City
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public Guid CountyId { get; set; }

        public County County { get; set; } = null!;

        public ICollection<Building> Buildings { get; set; }
            = new List<Building>();
    }
}
