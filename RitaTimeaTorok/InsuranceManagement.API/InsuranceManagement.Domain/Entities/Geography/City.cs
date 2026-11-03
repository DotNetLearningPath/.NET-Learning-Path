namespace InsuranceManagement.Domain.Entities.Geography
{
    public class City : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public Guid CountyId { get; set; }

        public County County { get; set; } = null!;
    }
}
