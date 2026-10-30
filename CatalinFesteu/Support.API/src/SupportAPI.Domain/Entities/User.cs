using Microsoft.AspNetCore.Identity;
using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public DepartmentType Department { get; private set; } = DepartmentType.Unassigned;
        public string IdentityIssuer { get; private set; } = string.Empty;
        public string IdentitySubject { get; private set; } = string.Empty;
        private User()
        {
            
        }

        public static User Create(string name, string identityIssuer, string identitySubject)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name)); 
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(identityIssuer));
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(identitySubject));

            return new User
            {
                Name = name.Trim(),
                IdentityIssuer = identityIssuer,
                IdentitySubject = identitySubject
            };
        }
    }
}
