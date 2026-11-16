#!/usr/bin/env bash
# Acesso do GitHub Actions à conta AWS da Fase 4 (D-50, AD-060). Idempotente.
#
#   scripts/infra/criar-acesso-github.sh
#
# Cria o provedor OIDC `token.actions.githubusercontent.com` e a role `autoreparos-github-pai`
# (administrador), que só aceita o repositório pai nos Environments `production` e `aws-bootstrap`.
# As roles `autoreparos-deploy-<servico>` dos serviços são criadas pelo Terraform da plataforma.
set -euo pipefail

PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
CONTA="198903849859"
REPO="Grupo78-PosTech-15SOAT/AutoReparos"
ROLE="autoreparos-github-pai"
PROVEDOR="token.actions.githubusercontent.com"
PROVEDOR_ARN="arn:aws:iam::${CONTA}:oidc-provider/${PROVEDOR}"

aws_() { aws --profile "$PERFIL" "$@"; }
conta_atual="$(aws_ sts get-caller-identity --query Account --output text)"
[ "$conta_atual" = "$CONTA" ] || { echo "Perfil $PERFIL aponta para $conta_atual, esperado $CONTA" >&2; exit 1; }

if ! aws_ iam get-open-id-connect-provider --open-id-connect-provider-arn "$PROVEDOR_ARN" >/dev/null 2>&1; then
  aws_ iam create-open-id-connect-provider --url "https://${PROVEDOR}" --client-id-list sts.amazonaws.com \
    --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1 >/dev/null
fi

TMP="$(mktemp)"; trap 'rm -f "$TMP"' EXIT
cat > "$TMP" <<JSON
{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Federated":"${PROVEDOR_ARN}"},
 "Action":"sts:AssumeRoleWithWebIdentity","Condition":{
  "StringEquals":{"${PROVEDOR}:aud":"sts.amazonaws.com",
   "${PROVEDOR}:sub":["repo:${REPO}:environment:production","repo:${REPO}:environment:aws-bootstrap"]}}}]}
JSON
if aws_ iam get-role --role-name "$ROLE" >/dev/null 2>&1; then
  aws_ iam update-assume-role-policy --role-name "$ROLE" --policy-document "file://$TMP"
else
  aws_ iam create-role --role-name "$ROLE" --assume-role-policy-document "file://$TMP" --max-session-duration 7200 \
    --description "GitHub Actions do repositorio pai (provisionamento da plataforma)" >/dev/null
fi
aws_ iam attach-role-policy --role-name "$ROLE" --policy-arn arn:aws:iam::aws:policy/AdministratorAccess
echo "AWS_ROLE_ARN=arn:aws:iam::${CONTA}:role/${ROLE}"
