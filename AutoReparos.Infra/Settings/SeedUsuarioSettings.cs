namespace AutoReparos.Infra.Settings
{
    public class SeedUsuarioSettings
    {
        public const string SectionName = "SeedUser";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
