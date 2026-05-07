param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("update-tool", "run", "watch", "db-update", "mig-add", "restore")]
    $Action,

    [Parameter(Mandatory=$false)]
    $Name = "InitialMigration"
)

switch ($Action) {
    "update-tool" {
        Write-Host "--- Atualizando dotnet-ef globalmente ---" -ForegroundColor Cyan
        dotnet tool update --global dotnet-ef
    }
    "run" {
        Write-Host "--- Iniciando API AutoReparos ---" -ForegroundColor Green
        dotnet run --project .\AutoReparos.API\
    }
    "watch" {
        Write-Host "--- Iniciando Hot Reload (Watch) ---" -ForegroundColor Green
        dotnet watch --project .\AutoReparos.API\
    }
    "db-update" {
        Write-Host "--- Aplicando Migrations no Banco de Dados ---" -ForegroundColor Yellow
        dotnet ef database update --project .\AutoReparos.Infra\ --startup-project .\AutoReparos.API\
    }
    "mig-add" {
        Write-Host "--- Criando nova Migration: $Name ---" -ForegroundColor Yellow
        dotnet ef migrations add $Name --project .\AutoReparos.Infra\ --startup-project .\AutoReparos.API\
    }
    "restore" {
        Write-Host "--- Restaurando Pacotes ---" -ForegroundColor Cyan
        dotnet restore
    }
}