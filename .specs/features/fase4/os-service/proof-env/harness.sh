#!/usr/bin/env bash
# Sobe o ambiente de prova local do os-service (C5 - ServiceDatabaseIsolation) e exporta OS_PROOF_*.
# Uso: source .specs/features/fase4/os-service/proof-env/harness.sh
# Requisitos: Git Bash, Docker, .NET 10 SDK, dotnet-ef (dotnet tool install --global dotnet-ef),
#             Python 3 com `pip install pyjwt cryptography`. Credenciais abaixo sao descartaveis (somente local).
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(git -C "$HERE" rev-parse --show-toplevel)"
OS="$ROOT/submodules/AutoReparos.OS"
WORK="${PROOF_WORK:-${TMPDIR:-/tmp}/os-proof}"; mkdir -p "$WORK"
PGPW=proofpw; DB=os_proof; PGPORT=55432

# 1. Postgres de prova (cria o container na primeira vez)
if ! docker ps -a --format '{{.Names}}' | grep -qx os-proof-pg; then
  docker run -d --name os-proof-pg -e POSTGRES_PASSWORD=$PGPW -e POSTGRES_DB=$DB -p $PGPORT:5432 postgres:16-alpine >/dev/null
else
  docker start os-proof-pg >/dev/null 2>&1
fi
for i in $(seq 1 30); do docker exec os-proof-pg pg_isready -U postgres >/dev/null 2>&1 && break; sleep 1; done

# 2. Esquema (script idempotente gerado das migracoes) e papeis sem privilegio nas tabelas
dotnet ef migrations script --idempotent --project "$OS/AutoReparos.OS.Infrastructure" \
  --startup-project "$OS/AutoReparos.OS.Infrastructure" -o "$WORK/os_migrations.sql" >/dev/null 2>&1
docker exec -i os-proof-pg psql -U postgres -d $DB -q < "$WORK/os_migrations.sql" >/dev/null 2>&1
for r in os_billing_proof os_execucao_proof; do
  docker exec os-proof-pg psql -U postgres -d $DB -q -c "DO \$\$ BEGIN IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname='$r') THEN CREATE ROLE $r LOGIN PASSWORD '$PGPW'; END IF; END \$\$;" >/dev/null 2>&1
done

# 3. Emissor JWT local (porta 8099)
curl -s -o /dev/null http://localhost:8099/tokens || (cd "$HERE" && nohup python mock_issuer.py > "$WORK/issuer.log" 2>&1 < /dev/null & sleep 3)

# 4. API publicada fora de bin/ (nao trava os builds dos testes), porta 5083
if ! curl -s -o /dev/null http://localhost:5083/; then
  rm -rf "$WORK/api-out"
  dotnet publish "$OS/AutoReparos.OS.Api/AutoReparos.Os.Api.csproj" -o "$WORK/api-out" --nologo -v q >/dev/null 2>&1
  ( export ConnectionStrings__OrdemServico="Host=localhost;Port=$PGPORT;Database=$DB;Username=postgres;Password=$PGPW" \
           Authentication__Authority="http://localhost:8099" Authentication__Audience="autoreparos-os" \
           Authentication__RequireHttpsMetadata=false ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5083"
    cd "$WORK/api-out" && nohup dotnet AutoReparos.Os.Api.dll > "$WORK/api.log" 2>&1 < /dev/null & )
  for i in $(seq 1 30); do curl -s -o /dev/null http://localhost:5083/ && break; sleep 1; done
fi

# 5. OS de teste e variaveis de prova
T=$(curl -s http://localhost:8099/tokens)
tok() { echo "$T" | python -c "import sys,json;print(json.load(sys.stdin)['$1'])"; }
OID=$(curl -s -X POST http://localhost:5083/api/v1/ordens-servico -H "Authorization: Bearer $(tok admin)" -H "Content-Type: application/json" \
  -d '{"clienteId":"11111111-1111-1111-1111-111111111111","veiculoId":"22222222-2222-2222-2222-222222222222"}' | python -c "import sys,json;print(json.load(sys.stdin)['id'])")
export OS_PROOF_DB_CONNECTION="Host=localhost;Port=$PGPORT;Database=$DB;Username=postgres;Password=$PGPW"
export OS_PROOF_API_URL="http://localhost:5083/" OS_PROOF_ORDER_ID="$OID"
export OS_PROOF_BILLING_DB_ROLE=os_billing_proof OS_PROOF_EXECUTION_DB_ROLE=os_execucao_proof
export OS_PROOF_BILLING_TOKEN="$(tok billing)" OS_PROOF_EXECUTION_TOKEN="$(tok execucao)"
echo "ambiente de prova pronto: ordem $OID"
