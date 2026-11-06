namespace SupportAPI.Api
{
    public class KeycloakConfiguration
    {
        public const string SectionName = "Keycloak";

        public string MetadataAddress { get; set; } = string.Empty;
        public string RealmUrl { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
    }
}
