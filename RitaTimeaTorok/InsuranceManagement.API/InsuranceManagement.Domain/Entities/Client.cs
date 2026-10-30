using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public ClientType Type { get; set; }

        public required string Name { get; set; }

        public required string IdentificationNumber { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public ICollection<Building> Buildings { get; set; }
            = new List<Building>();
    }
}
