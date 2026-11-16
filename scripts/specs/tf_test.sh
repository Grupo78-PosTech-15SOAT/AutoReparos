#!/usr/bin/env bash
# Provas de deploy-servicos que rodam `terraform test` com mock_provider, sem AWS.
# Uso: scripts/specs/tf_test.sh <diretório do Terraform, relativo à raiz do pai> <nome do teste>
# Roda <diretório>/tests/<nome>.tftest.hcl; código de saída 0 só se o arquivo existe e passa.
set -euo pipefail
cd "$(dirname "$0")/../.."
if [ "$#" -ne 2 ]; then
  echo "uso: $0 <diretório terraform> <nome do teste>" >&2
  exit 2
fi
diretorio="$1"
arquivo="tests/$2.tftest.hcl"
if [ ! -f "$diretorio/$arquivo" ]; then
  echo "FALHA $diretorio/$arquivo ausente"
  exit 1
fi
terraform -chdir="$diretorio" init -backend=false -input=false -no-color >/dev/null
terraform -chdir="$diretorio" test -no-color -filter="$arquivo"
