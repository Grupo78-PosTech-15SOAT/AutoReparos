namespace AutoReparos.IntegrationTests.Infrastructure.Config;

public record DbContainerSettings(
    string Image = "postgres:16-alpine",
    string Database = "autoreparos_test",
    string Username = "admin",
    string Password = "admin123"
);
