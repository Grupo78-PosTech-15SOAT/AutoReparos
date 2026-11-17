#!/usr/bin/env bash
# Credenciais da AWS pelo IRSA funcionando nos pods dos serviços.
# Uso: scripts/smoke/aws-credenciais.sh [janela]   (padrão: 10m)
#
# Para cada namespace (os, cobranca, execucao, cadastros), lê os logs dos pods na janela e conta as linhas com
# `AWSSDK.SecurityToken` (pacote do STS ausente no publish: o IRSA não troca o token pela credencial), `AccessDenied`
# ou `not authorized` (role do IRSA sem a permissão de SQS/DynamoDB). Falha se houver alguma, ou se o namespace não
# tiver pods.
# Requer kubectl com acesso ao cluster.
set -euo pipefail
JANELA="${1:-10m}"
PADRAO='AWSSDK\.SecurityToken|AccessDenied|not authorized'
FALHAS=0
falha() { echo "FALHA $*"; FALHAS=$((FALHAS + 1)); }

for ns in os cobranca execucao cadastros; do
  pods="$(kubectl get pods -n "$ns" -o name)"
  [ -n "$pods" ] || { falha "$ns: nenhum pod"; continue; }
  total=0
  for pod in $pods; do
    linhas="$(kubectl logs -n "$ns" "$pod" --all-containers --since="$JANELA" 2>/dev/null | grep -cE "$PADRAO" || true)"
    if [ "$linhas" -gt 0 ]; then
      echo "  $ns/${pod#pod/}: $linhas linha(s); exemplo:"
      kubectl logs -n "$ns" "$pod" --all-containers --since="$JANELA" 2>/dev/null | grep -m1 -E "$PADRAO" | cut -c1-240 | sed 's/^/    /' || true
    fi
    total=$((total + linhas))
  done
  if [ "$total" -eq 0 ]; then echo "ok $ns: nenhum erro de credencial da AWS em $JANELA"; else falha "$ns: $total linha(s) com erro de credencial da AWS em $JANELA"; fi
done

[ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
echo "OK smoke_aws_credenciais: os, cobranca, execucao e cadastros sem erro de credencial da AWS em $JANELA"
