using Microsoft.AspNetCore.Identity;
using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public DepartmentType Department { get; set; }
    }
}
