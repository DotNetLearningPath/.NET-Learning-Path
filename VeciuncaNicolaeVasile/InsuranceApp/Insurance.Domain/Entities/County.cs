namespace Insurance.Domain.Entities
{
    public class County
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public Country Country { get; set; } = new Country();
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
