namespace Insurance.Domain.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CountyId { get; set; }
        public County County { get; set; } = new County();
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}
