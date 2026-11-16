#!/usr/bin/env bash
# Roda o gate do servico-os (provas, e mutacoes se passar --mutations <arquivo>) com o ambiente de prova.
# Uso: bash .specs/features/fase4/servico-os/proof-env/run_proofs.sh [--mutations .specs/features/fase4/servico-os/mutations.json]
# Importante: a API de prova roda de um publish; mate o processo na porta 5083 antes se o codigo mudou.
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(git -C "$HERE" rev-parse --show-toplevel)"
source "$HERE/harness.sh"
cd "$ROOT"
python .agents/skills/tlc-spec-lean/scripts/check_proofs.py fase4/servico-os "$@"
echo "check_proofs exit=$?"
