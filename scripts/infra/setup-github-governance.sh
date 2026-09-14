#!/usr/bin/env bash
# ==============================================================================
# Script: setup-github-governance.sh
# Objetivo: Aplicar regras de proteção na branch main nos 4 repositórios oficiais
#           do ecossistema AutoReparos (Fase 3 Tech Challenge FIAP SOAT)
# ==============================================================================

set -euo pipefail

# Definição de cores para terminal
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

ORG="Grupo78-PosTech-15SOAT"
REPOS=(
  "AutoReparos.App"
  "AutoReparos.AuthLambda"
  "AutoReparos.Infra.Database"
  "AutoReparos.Infra.K8s"
)

DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    -h|--help)
      echo "Uso: $0 [--dry-run]"
      echo "  --dry-run   Simula as chamadas de API sem aplicar alterações reais"
      exit 0
      ;;
  esac
done

if [ "$DRY_RUN" = true ]; then
  echo -e "${YELLOW}ℹ Modo DRY-RUN ativado. Nenhuma alteração real será enviada para a API do GitHub.${NC}\n"
fi

# 1. Verificar se a CLI gh está instalada
if ! command -v gh &> /dev/null; then
  echo -e "${RED}❌ Erro: GitHub CLI (gh) não está instalada ou não está no PATH.${NC}"
  echo "Instale através de https://cli.github.com/ antes de prosseguir."
  exit 1
fi

# 2. Verificar autenticação da CLI gh
if ! gh auth status &> /dev/null; then
  echo -e "${RED}❌ Erro: GitHub CLI não está autenticada. Execute 'gh auth login' primeiro.${NC}"
  exit 1
fi

echo -e "${BLUE}=== Iniciando Aplicação de Governança na Organização: ${ORG} ===${NC}\n"

# Payload da API GitHub v3 para proteção de branch
PAYLOAD=$(cat <<EOF
{
  "required_status_checks": {
    "strict": true,
    "contexts": []
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false,
    "required_approving_review_count": 1
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false
}
EOF
)

SUCCESS_COUNT=0
TOTAL_COUNT=${#REPOS[@]}

for REPO in "${REPOS[@]}"; do
  FULL_REPO="${ORG}/${REPO}"
  echo -e "${CYAN}▶ Processando repositório: ${FULL_REPO}${NC}"

  if [ "$DRY_RUN" = true ]; then
    echo -e "  ${YELLOW}[DRY-RUN] Enviaria PUT para /repos/${FULL_REPO}/branches/main/protection com as regras:${NC}"
    echo -e "    - Exigência de Pull Request (mínimo 1 aprovação)"
    echo -e "    - Dismiss de reviews obsoletas (dismiss_stale_reviews = true)"
    echo -e "    - Status checks obrigatórios e branch atualizada (strict = true)"
    echo -e "    - Regras aplicadas inclusive a administradores (enforce_admins = true)"
    echo -e "    - Bloqueio de Force Push (allow_force_pushes = false)"
    echo -e "    - Bloqueio de Exclusão de Branch (allow_deletions = false)"
    SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
  else
    echo -e "  Configurando proteção de branch na 'main'..."
    if echo "$PAYLOAD" | gh api \
      --method PUT \
      --header "Accept: application/vnd.github+json" \
      "/repos/${FULL_REPO}/branches/main/protection" \
      --input - > /dev/null 2>&1; then
      echo -e "  ${GREEN}✔ Proteção da branch 'main' configurada com sucesso em ${FULL_REPO}${NC}"
      SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
    else
      echo -e "  ${RED}✖ Falha ao aplicar proteção em ${FULL_REPO}:main.${NC}"
      echo -e "  ${YELLOW}Dica: Verifique se a branch 'main' existe e se seu token possui permissão de Admin no repositório.${NC}"
    fi
  fi
  echo ""
done

echo -e "${BLUE}=== Relatório de Governança ===${NC}"
echo -e "Repositórios configurados com sucesso: ${GREEN}${SUCCESS_COUNT}/${TOTAL_COUNT}${NC}"

if [ "$SUCCESS_COUNT" -eq "$TOTAL_COUNT" ]; then
  echo -e "${GREEN}✔ Todas as branches principais foram protegidas com sucesso!${NC}"
  exit 0
else
  echo -e "${YELLOW}⚠ Alguns repositórios não puderam ser atualizados. Verifique os logs acima.${NC}"
  exit 1
fi
