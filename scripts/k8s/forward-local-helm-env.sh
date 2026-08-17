#!/usr/bin/env bash
set -e

# Descobrir o diretório do próprio script e determinar a raiz do projeto AutoReparos
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AUTOREPAROS_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

cd "$AUTOREPAROS_DIR"

if [[ ! -f .env ]]; then
  echo "❌ Erro: Arquivo .env não encontrado no diretório $AUTOREPAROS_DIR!"
  echo "Por favor, crie o arquivo .env com as configurações necessárias."
  exit 1
fi

echo "📄 Carregando variáveis de ambiente do .env..."
export $(grep -v '^#' .env | xargs)

GIT_TAG=$(git rev-parse --short HEAD 2>/dev/null || echo "latest")

echo "🚀 1. Sincronizando dependências do Helm..."
helm dependency update k8s/

echo "☸️ 2. Executando Helm Upgrade/Install..."
helm upgrade --install autoreparos k8s/ \
  -f k8s/values.yaml \
  --set api.image.repository="josemd12/autoreparos-api" \
  --set api.image.tag="$GIT_TAG" \
  --set web.image.repository="josemd12/autoreparos-web" \
  --set web.image.tag="$GIT_TAG" \
  --set api.config.aspnetcoreEnvironment="$ASPNETCORE_ENVIRONMENT" \
  --set api.config.jwtExpiryHours="$JWT_EXPIRY_HOURS" \
  --set api.config.seedUserEmail="$SEED_USER_EMAIL" \
  --set api.config.sendGridFromEmail="$SENDGRID_FROM_EMAIL" \
  --set api.config.sendGridFromName="$SENDGRID_FROM_NAME" \
  --set api.secrets.jwtSecret="$JWT_SECRET" \
  --set api.secrets.seedUserPassword="$SEED_USER_PASSWORD" \
  --set api.secrets.aprovacaoTokenSecret="$APROVACAO_TOKEN_SECRET" \
  --set api.secrets.sendGridApiKey="$SENDGRID_API_KEY" \
  --set database.password="$DB_PASSWORD"

echo "⏳ 3. Aguardando 10 segundos..."
sleep 10
kubectl get pods

echo "🔌 4. Redirecionando portas..."
./scripts/k8s/dev-forward.sh start

echo "✅ Concluído!"
