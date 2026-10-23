namespace SupportAPI.Infrastructure
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public required string Issuer { get; init; } = string.Empty;
        public required string Audience { get; init; } = string.Empty;
        public required string SigningKey { get; init; } = string.Empty;
        public required int AccessTokenMinutes { get; init; }
    }
}
