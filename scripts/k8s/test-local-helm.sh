#!/usr/bin/env bash
set -e

echo "🚀 1. Atualizando dependências do Helm Chart..."
helm dependency update k8s/

echo "📦 2. Construindo imagem Docker local da API..."
docker build -t autoreparos-api:latest -f AutoReparos.API/Dockerfile .

echo "🚢 3. Carregando imagem para o cluster Kind ('auto-reparos')..."
kind load docker-image autoreparos-api:latest --name auto-reparos

echo "☸️ 4. Instalando / Atualizando o Helm Chart no cluster..."
helm upgrade --install autoreparos k8s/ \
  -f k8s/values.yaml \
  --set api.secrets.jwtSecret="FBQOvEaUYAlmdilnGOk7vKzO9xUHiLgb8QCFUrk6af9" \
  --set api.secrets.seedUserPassword="Admin@123" \
  --set api.secrets.aprovacaoTokenSecret="another_super_secret_key_for_approval_tokens_with_enough_length" \
  --set api.secrets.sendGridApiKey="SG.dummy_key" \
  --set database.password="admin123"

echo "⏳ 5. Aguardando status dos Pods..."
kubectl get pods -w --timeout=30s || kubectl get pods
