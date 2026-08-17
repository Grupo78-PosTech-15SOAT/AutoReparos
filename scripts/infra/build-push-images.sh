#!/usr/bin/env bash
# build-push-images.sh — Build, tag e push automatizado das imagens Docker do AutoReparos
# Uso: ./scripts/infra/build-push-images.sh [REGISTRY_USER] [TAG]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AUTOREPAROS_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

cd "$AUTOREPAROS_DIR"

REGISTRY_USER="${1:-josemd12}"

# Se a tag não for informada, tenta pegar o commit curto do Git, ou usa 'latest'
if [[ -n "$2" ]]; then
  TAG="$2"
else
  GIT_TAG=$(git rev-parse --short HEAD 2>/dev/null || echo "latest")
  TAG="$GIT_TAG"
fi

API_IMAGE="${REGISTRY_USER}/autoreparos-api"
WEB_IMAGE="${REGISTRY_USER}/autoreparos-web"

echo "=========================================================="
echo "🐳 Iniciando Build, Tag e Push das Imagens Docker"
echo "=========================================================="
echo "📦 Registry: $REGISTRY_USER"
echo "🏷️  Tag:      $TAG"
echo "=========================================================="

# 1. Build da API .NET
echo ""
echo "🏗️  1/4 Building API .NET image ($API_IMAGE:$TAG)..."
docker build -t "$API_IMAGE:$TAG" -t "$API_IMAGE:latest" -f AutoReparos.API/Dockerfile .

# 2. Build do Frontend Web (Angular)
echo ""
echo "🏗️  2/4 Building Frontend Web image ($WEB_IMAGE:$TAG)..."
docker build -t "$WEB_IMAGE:$TAG" -t "$WEB_IMAGE:latest" -f AutoReparos.Web/Dockerfile ./AutoReparos.Web

# 3. Push da API
echo ""
echo "🚀 3/4 Pushing API image to Docker Registry..."
docker push "$API_IMAGE:$TAG"
docker push "$API_IMAGE:latest"

# 4. Push do Frontend Web
echo ""
echo "🚀 4/4 Pushing Frontend Web image to Docker Registry..."
docker push "$WEB_IMAGE:$TAG"
docker push "$WEB_IMAGE:latest"

echo ""
echo "=========================================================="
echo "✅ Sucesso! Imagens publicadas com sucesso:"
echo "   🔹 API: $API_IMAGE:$TAG e $API_IMAGE:latest"
echo "   🔹 Web: $WEB_IMAGE:$TAG e $WEB_IMAGE:latest"
echo "=========================================================="
