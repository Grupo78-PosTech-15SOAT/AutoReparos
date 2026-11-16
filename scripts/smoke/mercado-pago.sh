#!/usr/bin/env bash
# Prova C41 de deploy-servicos (AC 29, D-62): credenciais do Mercado Pago chegam ao pod da Cobrança.
# Uso: scripts/smoke/mercado-pago.sh
#
# 1. O Deployment `cobranca` entrega MercadoPago__AccessToken e MercadoPago__WebhookSecret a partir do Secret
#    `cobranca-segredos` (chaves MercadoPagoAccessToken e MercadoPagoWebhookSecret), com optional: true.
# 2. O Secret `cobranca-segredos` tem as duas chaves (os segredos existem no Environment production).
# 3. Pelo API Gateway, POST /api/v1/webhooks/mercado-pago com corpo válido e sem assinatura, ou com assinatura
#    falsa, responde 401 (a Cobrança validou a assinatura com o segredo recebido).
# Requer kubectl com acesso ao cluster, aws (perfil autoreparos), curl e python3.
set -euo pipefail
PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
FALHAS=0
falha() { echo "FALHA $*"; FALHAS=$((FALHAS + 1)); }

kubectl get deployment cobranca -n cobranca -o json | python3 -c '
import json, sys
env = json.load(sys.stdin)["spec"]["template"]["spec"]["containers"][0].get("env", [])
esperado = {"MercadoPago__AccessToken": "MercadoPagoAccessToken", "MercadoPago__WebhookSecret": "MercadoPagoWebhookSecret"}
erros = []
for nome, chave in esperado.items():
    ref = next((((e.get("valueFrom") or {}).get("secretKeyRef") or {}) for e in env if e["name"] == nome), None)
    if not ref or ref.get("name") != "cobranca-segredos" or ref.get("key") != chave or ref.get("optional") is not True:
        erros.append(f"{nome}: secretKeyRef {ref}, esperado cobranca-segredos/{chave} optional")
for e in erros:
    print("FALHA", e)
print("ok Deployment entrega as duas variáveis do Mercado Pago" if not erros else "")
sys.exit(1 if erros else 0)' || FALHAS=$((FALHAS + 1))

chaves="$(kubectl get secret cobranca-segredos -n cobranca -o jsonpath='{.data}' | python3 -c 'import json,sys; print(" ".join(sorted(json.load(sys.stdin))))')"
for chave in MercadoPagoAccessToken MercadoPagoWebhookSecret; do
  case " $chaves " in *" $chave "*) echo "ok Secret cobranca-segredos tem $chave" ;; *) falha "Secret cobranca-segredos sem $chave" ;; esac
done

gateway="$(aws --profile "$PERFIL" --region us-east-1 ssm get-parameter --name /autoreparos/production/apigateway/endpoint --query Parameter.Value --output text)"
corpo='{"type":"payment","data":{"id":"123"}}'
sem="$(curl -s -o /dev/null -w '%{http_code}' -X POST -H 'Content-Type: application/json' -d "$corpo" "$gateway/api/v1/webhooks/mercado-pago")"
falsa="$(curl -s -o /dev/null -w '%{http_code}' -X POST -H 'Content-Type: application/json' -H 'x-signature: ts=1,v1=00' -H 'x-request-id: prova' -d "$corpo" "$gateway/api/v1/webhooks/mercado-pago")"
if [ "$sem" = 401 ] && [ "$falsa" = 401 ]; then echo "ok webhook sem assinatura e com assinatura falsa: 401"; else falha "webhook: sem assinatura $sem, assinatura falsa $falsa (esperado 401 e 401)"; fi

[ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
echo "OK smoke_mercado_pago: credenciais no pod da Cobrança e webhook recusando assinatura inválida"
