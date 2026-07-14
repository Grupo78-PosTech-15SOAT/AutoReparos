param (
    [string]$Region = "",
    [string]$ClusterName = ""
)

$DefaultRegion = "us-east-1"
$DefaultCluster = "autoreparos-cluster"

# Caminho do arquivo variables.tf
$InfraDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$VariablesFile = Join-Path $InfraDir "variables.tf"

if (Test-Path $VariablesFile) {
    # Tenta ler a região
    $Content = Get-Content $VariablesFile -Raw
    if ($Content -match 'variable "aws_region"\s*\{[^}]*default\s*=\s*"([^"]+)"') {
        $DefaultRegion = $Matches[1]
    }
    # Tenta ler o nome do cluster
    if ($Content -match 'variable "cluster_name"\s*\{[^}]*default\s*=\s*"([^"]+)"') {
        $DefaultCluster = $Matches[1]
    }
}

if ([string]::IsNullOrEmpty($Region)) { $Region = $DefaultRegion }
if ([string]::IsNullOrEmpty($ClusterName)) { $ClusterName = $DefaultCluster }

Write-Host "Configurando contexto do kubectl para o EKS..." -ForegroundColor Cyan
Write-Host "Região: $Region" -ForegroundColor Cyan
Write-Host "Cluster: $ClusterName" -ForegroundColor Cyan

aws eks update-kubeconfig --region $Region --name $ClusterName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Sucesso! Testando conexão com o cluster..." -ForegroundColor Green
    kubectl get svc
} else {
    Write-Warning "Falha ao atualizar o contexto do kubectl. Verifique se a AWS CLI está configurada corretamente."
}
