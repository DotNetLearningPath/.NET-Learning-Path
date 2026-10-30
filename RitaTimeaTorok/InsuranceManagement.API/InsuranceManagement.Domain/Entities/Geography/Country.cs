namespace InsuranceManagement.Domain.Entities.Geography
{
    public class Country
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public ICollection<County> Counties { get; set; } = new List<County>();
    }
}
