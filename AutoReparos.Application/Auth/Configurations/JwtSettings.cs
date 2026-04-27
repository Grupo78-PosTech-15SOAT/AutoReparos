namespace AutoReparos.Application.Auth.Configurations
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int ExpiryHours { get; set; } = 2;
    }
}
