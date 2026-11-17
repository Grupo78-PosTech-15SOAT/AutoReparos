#!/usr/bin/env bash
# Prova C33 de deploy-servicos: um serviço implantado de verdade pelo próprio pipeline.
# Uso: scripts/smoke/deploy-execucao.sh execucao
#
# 1. O último ci-cd.yml da main de AutoReparos.Execucao terminou com o job deploy verde (depois da aprovação
#    no Environment production).
# 2. Pelo API Gateway (URL em /autoreparos/production/apigateway/endpoint), com um token emitido pelo Cadastros
#    do mesmo ambiente (login do usuário inicial), GET /api/v1/servicos responde 200 com os 5 serviços semeados.
# 3. Sem token, a mesma rota responde 401.
# Requer gh autenticado, aws com o perfil autoreparos e python3.
set -euo pipefail
[ "${1:-}" = execucao ] || { echo "uso: $0 execucao" >&2; exit 2; }

PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
ssm() { aws --profile "$PERFIL" --region us-east-1 ssm get-parameter --name "$1" --with-decryption --query Parameter.Value --output text; }

FALHAS=0
falha() { echo "FALHA $*"; FALHAS=$((FALHAS + 1)); }

execucao_id="$(gh run list -R Grupo78-PosTech-15SOAT/AutoReparos.Execucao --workflow ci-cd.yml --branch main -L 1 --json databaseId --jq '.[0].databaseId')"
deploy="$(gh run view "$execucao_id" -R Grupo78-PosTech-15SOAT/AutoReparos.Execucao --json jobs --jq '[.jobs[]|select(.name=="deploy")|.conclusion][0]')"
if [ "$deploy" = success ]; then echo "ok workflow $execucao_id da main: deploy success"; else falha "workflow $execucao_id da main: deploy $deploy"; fi

GATEWAY="$(ssm /autoreparos/production/apigateway/endpoint)" SENHA="$(ssm /autoreparos/production/cadastros/seed-user-password)" \
python3 - <<'PY' || FALHAS=$((FALHAS + 1))
import json, os, sys, urllib.error, urllib.request

gateway = os.environ["GATEWAY"].rstrip("/")

def chamar(metodo, rota, corpo=None, token=None):
    cabecalhos = {"Content-Type": "application/json"}
    if token:
        cabecalhos["Authorization"] = f"Bearer {token}"
    dados = json.dumps(corpo).encode() if corpo is not None else None
    pedido = urllib.request.Request(gateway + rota, data=dados, headers=cabecalhos, method=metodo)
    try:
        with urllib.request.urlopen(pedido, timeout=30) as resposta:
            return resposta.status, resposta.read().decode()
    except urllib.error.HTTPError as erro:
        return erro.code, erro.read().decode()

falhas = []
status, corpo = chamar("POST", "/api/auth/login", {"email": "admin@autoreparos.com", "password": os.environ["SENHA"]})
token = json.loads(corpo).get("token") if status == 200 else None
if not token:
    print(f"FALHA login no Cadastros respondeu {status}")
    sys.exit(1)
print("ok token emitido pelo Cadastros pelo gateway")

status, corpo = chamar("GET", "/api/v1/servicos", token=token)
itens = json.loads(corpo).get("itens", []) if status == 200 else []
if status == 200 and len(itens) == 5:
    print(f"ok GET /api/v1/servicos com token: 200 e {len(itens)} serviços semeados")
else:
    falhas.append(f"GET /api/v1/servicos com token: {status}, {len(itens)} itens (esperado 200 e 5)")

status, _ = chamar("GET", "/api/v1/servicos")
if status == 401:
    print("ok GET /api/v1/servicos sem token: 401")
else:
    falhas.append(f"GET /api/v1/servicos sem token: {status} (esperado 401)")

for item in falhas:
    print(f"FALHA {item}")
sys.exit(1 if falhas else 0)
PY

[ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
echo "OK smoke_deploy execucao: pipeline verde, 200 com token do Cadastros e 401 sem token"
