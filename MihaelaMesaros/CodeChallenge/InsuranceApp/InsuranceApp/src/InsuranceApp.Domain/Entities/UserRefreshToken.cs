namespace InsuranceApp.Domain.Entities;

public sealed class UserRefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
