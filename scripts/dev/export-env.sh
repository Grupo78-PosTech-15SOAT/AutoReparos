#!/usr/bin/env bash
# export-env.sh — Exporta as variáveis do .env ou .env.example mapeadas para os testes/sessão .NET
# Uso: source ./scripts/dev/export-env.sh (ou . ./scripts/dev/export-env.sh)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AUTOREPAROS_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

ENV_FILE="$AUTOREPAROS_DIR/.env"
if [[ ! -f "$ENV_FILE" ]]; then
  ENV_FILE="$AUTOREPAROS_DIR/.env.example"
fi

if [[ ! -f "$ENV_FILE" ]]; then
  echo "❌ Erro: Nem .env nem .env.example foram encontrados em $AUTOREPAROS_DIR"
  return 1 2>/dev/null || exit 1
fi

echo "📄 Exportando variáveis de ambiente de: $ENV_FILE"

# 1. Carregar variáveis cruas do arquivo
while IFS='=' read -r key value || [[ -n "$key" ]]; do
  # Ignorar linhas vazias e comentários
  [[ -z "$key" || "$key" =~ ^[[:space:]]*# ]] && continue
  
  # Remover espaços em branco no início e fim das chaves/valores
  key=$(echo "$key" | xargs)
  value=$(echo "$value" | xargs)
  
  # Exportar a variável original
  export "$key=$value"
done < "$ENV_FILE"

# 2. Mapear para o formato aceito pelos testes unitários e de integração (.NET Configuration Provider)
export ConnectionStrings__DbConnection="${ConnectionStrings__DbConnection:-Host=localhost;Database=autoreparos_test;Username=admin;Password=${DB_PASSWORD:-admin123}}"
export Jwt__Secret="${JWT_SECRET:-FBQOvEaUYAlmdilnGOk7vKzO9xUHiLgb8QCFUrk6af9}"
export Jwt__ExpiryHours="${JWT_EXPIRY_HOURS:-2}"
export SeedUser__Email="${SEED_USER_EMAIL:-admin@autoreparos.com}"
export SeedUser__Password="${SEED_USER_PASSWORD:-Admin@123}"
export AprovacaoToken__Secret="${APROVACAO_TOKEN_SECRET:-another_super_secret_key_for_approval_tokens_with_enough_length}"
export SendGrid__ApiKey="${SENDGRID_API_KEY:-SG.dummy_key}"
export SendGrid__FromEmail="${SENDGRID_FROM_EMAIL:-noreply@autoreparos.com}"
export SendGrid__FromName="${SENDGRID_FROM_NAME:-AutoReparos}"
export App__BaseUrl="${APP_BASE_URL:-http://localhost:8080}"

echo "✅ Variáveis da sessão exportadas com sucesso!"
