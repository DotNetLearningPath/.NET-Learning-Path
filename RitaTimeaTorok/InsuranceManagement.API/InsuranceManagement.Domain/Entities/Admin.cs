namespace InsuranceManagement.Domain.Entities
{
    public class Admin : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
