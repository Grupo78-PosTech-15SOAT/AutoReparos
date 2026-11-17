#!/usr/bin/env bash
# Prova C34 de deploy-servicos: uma versão com /health quebrado é desfeita pelo --atomic.
# Uso: scripts/smoke/rollback-execucao.sh execucao
#
# Como quebra o /health sem trocar o código: o /health do serviço verifica a database, e o pod lê a connection
# string do Secret <servico>-database só ao iniciar. O script troca o Secret por uma connection string para um
# endereço que não responde, faz helm upgrade --atomic com uma mudança no template do pod (pods novos; --no-hooks
# porque o Job de migration já falharia antes, e aqui o alvo é a readiness), e devolve o Secret original sempre
# (trap). Os pods antigos continuam com a connection string boa e atendendo durante todo o teste.
#
# Passa se: a revisão nova fica `failed`, a última revisão é `deployed` com "Rollback to <anterior>" e
# GET /api/v1/servicos pelo API Gateway continua respondendo (401 sem token e 200 com token do Cadastros).
# Requer kubectl com acesso ao cluster, helm, aws (perfil autoreparos) e python3.
set -euo pipefail
cd "$(dirname "$0")/../.."
[ "${1:-}" = execucao ] || { echo "uso: $0 execucao" >&2; exit 2; }
SERVICO=execucao
CHART="submodules/AutoReparos.Execucao/deploy/helm/$SERVICO"
CHAVE=ConnectionStrings__Execucao
PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
ssm() { aws --profile "$PERFIL" --region us-east-1 ssm get-parameter --name "$1" --with-decryption --query Parameter.Value --output text; }

anterior="$(helm history "$SERVICO" -n "$SERVICO" -o json | python3 -c 'import json,sys
h=[r for r in json.load(sys.stdin) if r["status"]=="deployed"]; print(h[-1]["revision"] if h else "")')"
[ -n "$anterior" ] || { echo "FALHA nenhuma revisão deployed de $SERVICO"; exit 1; }
politica_atual="$(helm get values "$SERVICO" -n "$SERVICO" -o json | python3 -c 'import json,sys; print(json.load(sys.stdin).get("image",{}).get("pullPolicy","Always"))')"
nova_politica=IfNotPresent; [ "$politica_atual" = IfNotPresent ] && nova_politica=Always

original="$(kubectl get secret "$SERVICO-database" -n "$SERVICO" -o "jsonpath={.data.$CHAVE}")"
restaurar() {
  kubectl patch secret "$SERVICO-database" -n "$SERVICO" --type merge -p "{\"data\":{\"$CHAVE\":\"$original\"}}" >/dev/null
  echo "Secret $SERVICO-database restaurado"
}
trap restaurar EXIT
quebrada="$(printf 'Host=203.0.113.10;Port=5432;Database=execucao_db;Username=execucao_app;Password=x;Timeout=3' | base64 -w0)"
kubectl patch secret "$SERVICO-database" -n "$SERVICO" --type merge -p "{\"data\":{\"$CHAVE\":\"$quebrada\"}}" >/dev/null

echo "== helm upgrade --atomic da versão com /health quebrado (revisão anterior: $anterior)"
if helm upgrade "$SERVICO" "$CHART" -n "$SERVICO" --reuse-values --set-string image.pullPolicy="$nova_politica" \
     --no-hooks --atomic --wait --timeout 3m; then
  echo "FALHA o upgrade com /health quebrado terminou com sucesso"
  exit 1
fi
restaurar
trap - EXIT

FALHAS=0
helm history "$SERVICO" -n "$SERVICO" --max 3
helm history "$SERVICO" -n "$SERVICO" -o json | ANTERIOR="$anterior" python3 -c '
import json, os, sys
h = json.load(sys.stdin)
anterior = int(os.environ["ANTERIOR"])
quebrada = [r for r in h if r["revision"] == anterior + 1]
ultima = h[-1]
erros = []
if not quebrada or quebrada[0]["status"] != "failed":
    erros.append(f"revisão {anterior + 1} deveria estar failed: {quebrada}")
if ultima["status"] != "deployed" or f"Rollback to {anterior}" not in ultima.get("description", ""):
    erros.append(f"última revisão deveria ser deployed com Rollback to {anterior}: {ultima}")
for e in erros:
    print("FALHA", e)
sys.exit(1 if erros else 0)' || FALHAS=$((FALHAS + 1))

GATEWAY="$(ssm /autoreparos/production/apigateway/endpoint)" SENHA="$(ssm /autoreparos/production/cadastros/seed-user-password)" \
python3 - <<'PY' || FALHAS=$((FALHAS + 1))
import json, os, sys, urllib.error, urllib.request
gateway = os.environ["GATEWAY"].rstrip("/")
def chamar(metodo, rota, corpo=None, token=None):
    cab = {"Content-Type": "application/json", **({"Authorization": f"Bearer {token}"} if token else {})}
    pedido = urllib.request.Request(gateway + rota, data=json.dumps(corpo).encode() if corpo else None, headers=cab, method=metodo)
    try:
        with urllib.request.urlopen(pedido, timeout=30) as r:
            return r.status, r.read().decode()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()
sem, _ = chamar("GET", "/api/v1/servicos")
status, corpo = chamar("POST", "/api/auth/login", {"email": "admin@autoreparos.com", "password": os.environ["SENHA"]})
token = json.loads(corpo).get("token") if status == 200 else None
com, _ = chamar("GET", "/api/v1/servicos", token=token)
print(f"GET /api/v1/servicos depois do rollback: sem token {sem}, com token {com}")
sys.exit(0 if (sem, com) == (401, 200) else 1)
PY

[ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
echo "OK assert_rollback execucao: revisão com /health quebrado failed, anterior restaurada e a API continua respondendo"
