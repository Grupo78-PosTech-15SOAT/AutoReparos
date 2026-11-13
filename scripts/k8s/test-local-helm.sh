#!/usr/bin/env bash
set -e

echo "🚀 1. Atualizando dependências do Helm Chart..."
helm dependency update submodules/AutoReparos.Infra.K8s/k8s/

echo "📦 2. Construindo imagem Docker local da API..."
docker build -t autoreparos-api:latest -f submodules/AutoReparos.Cadastros/AutoReparos.API/Dockerfile submodules/AutoReparos.Cadastros

echo "🚢 3. Carregando imagem para o cluster Kind ('auto-reparos')..."
kind load docker-image autoreparos-api:latest --name auto-reparos

echo "☸️ 4. Instalando / Atualizando o Helm Chart no cluster..."
helm upgrade --install autoreparos submodules/AutoReparos.Infra.K8s/k8s/ \
  -f submodules/AutoReparos.Infra.K8s/k8s/values.yaml \
  --set api.secrets.jwtChavePrivadaPem="$JWT_CHAVE_PRIVADA_PEM" \
  --set api.secrets.chaveInterna="${CHAVE_INTERNA:-chave_interna_de_desenvolvimento}" \
  --set api.config.jwtIssuer="${JWT_ISSUER:-http://localhost:8080}" \
  --set api.secrets.seedUserPassword="Admin@123" \
  --set api.secrets.sendGridApiKey="SG.dummy_key" \
  --set database.password="admin123"

echo "⏳ 5. Aguardando status dos Pods..."
kubectl get pods -w --timeout=30s || kubectl get pods
