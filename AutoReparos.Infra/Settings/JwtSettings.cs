namespace AutoReparos.Infra.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 2;
}
