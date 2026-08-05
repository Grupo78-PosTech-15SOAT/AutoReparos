#!/bin/bash

# Define valores padrões
DEFAULT_REGION="us-east-1"
DEFAULT_CLUSTER="autoreparos-cluster"

# Tenta ler do variables.tf se ele existir
INFRA_DIR="$(dirname "$0")"
VARIABLES_FILE="$INFRA_DIR/variables.tf"

if [[ -f "$VARIABLES_FILE" ]]; then
  # Tenta extrair a região padrão do variables.tf
  EXTRACTED_REGION=$(grep -A 3 'variable "aws_region"' "$VARIABLES_FILE" | grep 'default' | cut -d'"' -f2)
  if [[ ! -z "$EXTRACTED_REGION" ]]; then
    DEFAULT_REGION="$EXTRACTED_REGION"
  fi

  # Tenta extrair o nome do cluster padrão do variables.tf
  EXTRACTED_CLUSTER=$(grep -A 3 'variable "cluster_name"' "$VARIABLES_FILE" | grep 'default' | cut -d'"' -f2)
  if [[ ! -z "$EXTRACTED_CLUSTER" ]]; then
    DEFAULT_CLUSTER="$EXTRACTED_CLUSTER"
  fi
fi

REGION=${1:-$DEFAULT_REGION}
CLUSTER_NAME=${2:-$DEFAULT_CLUSTER}

echo "Configurando contexto do kubectl para o EKS..."
echo "Região: $REGION"
echo "Cluster: $CLUSTER_NAME"

aws eks update-kubeconfig --region "$REGION" --name "$CLUSTER_NAME"

if [[ $? -eq 0 ]]; then
  echo "Sucesso! Testando conexão com o cluster..."
  kubectl get svc
else
  echo "Falha ao atualizar o contexto do kubectl. Verifique se a AWS CLI está configurada corretamente."
  exit 1
fi
