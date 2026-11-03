namespace InsuranceManagement.Domain.Entities.Geography
{
    public class Country : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }
    }
}
