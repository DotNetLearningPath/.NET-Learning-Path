using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Client
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public ClientType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
