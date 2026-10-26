namespace Insurance.Domain.Entities
{
    public class Country
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<County> Counties { get; set; } = new List<County>();
    }
}
