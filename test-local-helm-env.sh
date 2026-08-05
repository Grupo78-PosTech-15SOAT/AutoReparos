#!/usr/bin/env bash
set -e

# Carregar variáveis do .env
export $(grep -v '^#' .env | xargs)

echo "🚀 1. Sincronizando dependências do Helm..."
helm dependency update k8s/

echo "☸️ 2. Executando Helm Upgrade/Install..."
helm upgrade --install autoreparos k8s/ \
  -f k8s/values.yaml \
  --set api.image.repository="josemd12/autoreparos-api" \
  --set api.image.tag="a987a22" \
  --set web.image.repository="josemd12/autoreparos-web" \
  --set web.image.tag="a987a22" \
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
pkill -f "kubectl port-forward" || true

kubectl port-forward svc/autoreparos-api-service 8080:8080 > /dev/null 2>&1 &
kubectl port-forward svc/autoreparos-web-service 80:80 > /dev/null 2>&1 &
kubectl port-forward svc/autoreparos-grafana 3000:3000 > /dev/null 2>&1 &
kubectl port-forward svc/autoreparos-jaeger 16686:16686 > /dev/null 2>&1 &
kubectl port-forward svc/autoreparos-prometheus 9090:9090 > /dev/null 2>&1 &

echo "✅ Concluído!"
