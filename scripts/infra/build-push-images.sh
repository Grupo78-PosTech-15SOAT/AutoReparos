#!/usr/bin/env bash
# build-push-images.sh — Build, tag e push da imagem Docker do Web do AutoReparos
# Uso: ./scripts/infra/build-push-images.sh [REGISTRY_USER] [TAG]
#
# OS, Cobrança, Execução e Cadastros publicam a própria imagem no pipeline de cada repositório (ADR-005);
# o repositório pai só constrói o Web.

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

WEB_IMAGE="${REGISTRY_USER}/autoreparos-web"

echo "=========================================================="
echo "🐳 Iniciando Build, Tag e Push da imagem do Web"
echo "=========================================================="
echo "📦 Registry: $REGISTRY_USER"
echo "🏷️  Tag:      $TAG"
echo "=========================================================="

# 1. Build do Frontend Web (Angular)
echo ""
echo "🏗️  1/2 Building Frontend Web image ($WEB_IMAGE:$TAG)..."
docker build -t "$WEB_IMAGE:$TAG" -t "$WEB_IMAGE:latest" -f submodules/AutoReparos.Web/Dockerfile submodules/AutoReparos.Web

# 2. Push do Frontend Web
echo ""
echo "🚀 2/2 Pushing Frontend Web image to Docker Registry..."
docker push "$WEB_IMAGE:$TAG"
docker push "$WEB_IMAGE:latest"

echo ""
echo "=========================================================="
echo "✅ Sucesso! Imagem publicada: $WEB_IMAGE:$TAG e $WEB_IMAGE:latest"
echo "=========================================================="
