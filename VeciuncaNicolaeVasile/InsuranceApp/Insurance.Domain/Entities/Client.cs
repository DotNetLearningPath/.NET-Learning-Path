using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public ClientType ClientType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
