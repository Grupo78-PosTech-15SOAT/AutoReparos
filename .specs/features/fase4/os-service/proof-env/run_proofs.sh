#!/usr/bin/env bash
# Roda o gate do os-service (provas, e mutacoes se passar --mutations <arquivo>) com o ambiente de prova.
# Uso: bash .specs/features/fase4/os-service/proof-env/run_proofs.sh [--mutations .specs/features/fase4/os-service/mutations.json]
# Importante: a API de prova roda de um publish; mate o processo na porta 5083 antes se o codigo mudou.
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(git -C "$HERE" rev-parse --show-toplevel)"
source "$HERE/harness.sh"
cd "$ROOT"
python .agents/skills/tlc-spec-lean/scripts/check_proofs.py fase4/os-service "$@"
echo "check_proofs exit=$?"
