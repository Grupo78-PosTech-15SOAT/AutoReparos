#!/usr/bin/env bash
# Roda os smoke tests do ambiente de produção e registra cada execução no log da feature.
# Uso: scripts/smoke/rodar.sh [deploy-execucao|rollback-execucao|mercado-pago ...]   (sem argumentos: todos)
#
# Cada execução acrescenta uma linha (data UTC, commit, teste, resultado, última linha da saída) em
# .specs/features/fase4/deploy-servicos/smoke.md. O rollback-execucao quebra o /health da Execução por alguns
# minutos e restaura sozinho: rode-o de propósito, não em meio a uma demonstração.
set -uo pipefail
cd "$(dirname "$0")/../.."
LOG=".specs/features/fase4/deploy-servicos/smoke.md"
declare -A ARGS=([deploy-execucao]=execucao [rollback-execucao]=execucao [mercado-pago]=)
TESTES=("$@")
[ "${#TESTES[@]}" -gt 0 ] || TESTES=(deploy-execucao mercado-pago rollback-execucao)

commit="$(git rev-parse --short HEAD)"
falhas=0
for teste in "${TESTES[@]}"; do
  [ -n "${ARGS[$teste]+x}" ] || { echo "teste desconhecido: $teste" >&2; exit 2; }
  echo "== $teste"
  saida="$(scripts/smoke/"$teste".sh ${ARGS[$teste]} 2>&1)"; codigo=$?
  echo "$saida"
  resultado=PASS; [ "$codigo" -eq 0 ] || { resultado=FAIL; falhas=$((falhas + 1)); }
  ultima="$(printf '%s\n' "$saida" | tail -1 | tr '|' '/')"
  printf '| %s | `%s` | `%s` | %s | %s |\n' "$(date -u +%Y-%m-%dT%H:%MZ)" "$commit" "$teste" "$resultado" "$ultima" >> "$LOG"
done
echo "Registrado em $LOG"
[ "$falhas" -eq 0 ]
