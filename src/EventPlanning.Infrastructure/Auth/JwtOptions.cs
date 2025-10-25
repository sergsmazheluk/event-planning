namespace EventPlanning.Infrastructure.Auth
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string Key { get; set; } = default!; // симметричный ключ (HMAC)
        public int ExpiresMinutes { get; set; } = 60;
    }
}
