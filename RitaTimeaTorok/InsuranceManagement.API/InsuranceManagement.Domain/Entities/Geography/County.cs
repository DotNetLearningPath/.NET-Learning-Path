namespace InsuranceManagement.Domain.Entities.Geography
{
    public class County : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; } 
        public Guid CountryId { get; set; }

    }
}
