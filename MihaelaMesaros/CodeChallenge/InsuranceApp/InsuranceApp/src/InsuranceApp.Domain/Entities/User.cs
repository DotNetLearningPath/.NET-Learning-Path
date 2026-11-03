using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class User
{
    public Guid UserId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public Role RoleId {get; set; }
    public Guid? BrokerId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
