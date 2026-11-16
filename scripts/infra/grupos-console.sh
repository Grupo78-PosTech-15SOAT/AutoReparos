#!/usr/bin/env bash
# Cria (ou atualiza) os Resource Groups que mostram no console da AWS os recursos de cada serviço.
# Idempotente e sem custo. Os grupos filtram por tag: Servico=<servico> (Terraform de cada serviço)
# e Project=AutoReparos (plataforma: VPC, EKS, RDS, ECR do Web, Lambdas).
#
#   scripts/infra/grupos-console.sh
#
# Console: Resource Groups & Tag Editor -> Saved resource groups (região us-east-1).
set -euo pipefail

PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
REGIAO="us-east-1"
aws_() { aws --profile "$PERFIL" --region "$REGIAO" "$@"; }

consulta() { # chave valor
  printf '{"Type":"TAG_FILTERS_1_0","Query":"{\\"ResourceTypeFilters\\":[\\"AWS::AllSupported\\"],\\"TagFilters\\":[{\\"Key\\":\\"%s\\",\\"Values\\":[\\"%s\\"]}]}"}' "$1" "$2"
}

garantir_grupo() { # nome descrição chave valor
  local nome="$1" descricao="$2" query
  query="$(consulta "$3" "$4")"
  if aws_ resource-groups get-group --group-name "$nome" >/dev/null 2>&1; then
    aws_ resource-groups update-group-query --group-name "$nome" --resource-query "$query" >/dev/null
    echo "atualizado $nome"
  else
    aws_ resource-groups create-group --name "$nome" --description "$descricao" --resource-query "$query" >/dev/null
    echo "criado $nome"
  fi
}

garantir_grupo autoreparos-plataforma "Plataforma compartilhada - VPC EKS RDS API Gateway Lambdas e Web" Project AutoReparos
for servico in os cobranca execucao cadastros; do
  garantir_grupo "autoreparos-$servico" "Recursos exclusivos do servico $servico - infra terraform do repositorio" Servico "$servico"
done

echo
echo "Abra: https://${REGIAO}.console.aws.amazon.com/resource-groups/groups?region=${REGIAO}"
