#!/usr/bin/env bash
# Configuração do GitHub para o deploy por serviço (D-56, AC 26 de deploy-servicos). Idempotente.
#
#   scripts/infra/configurar-github.sh                            aplica (escreve no GitHub e no SSM)
#   scripts/infra/configurar-github.sh --simular                  mostra o que aplicaria, só lendo
#   scripts/infra/configurar-github.sh --verificar                C31: confere o GitHub, só lendo; sai != 0 se faltar algo
#   scripts/infra/configurar-github.sh --verificar-idempotencia   C32: os segredos gerados continuam na versão 1
#   scripts/infra/configurar-github.sh --verificar-token-servico  C37: a Cobrança tem o TOKEN_SERVICO (D-58)
#
# Gera uma única vez, como SecureString no SSM (nunca sobrescreve), a chave JWT do Cadastros, a CHAVE_INTERNA
# (a mesma no pai, no Cadastros e no OS), a senha do usuário inicial e a senha do Grafana do pai. Lê o SendGrid
# de /autoreparos/production/cadastros/sendgrid-* sem pedir nada. Copia os valores para variáveis, segredos e
# Environments do repositório pai e dos 4 serviços, conforme .specs/features/fase4/deploy-servicos/contratos.md.
#
# Como segredos do GitHub não podem ser lidos de volta, quem recebe a CHAVE_INTERNA ganha também a variável
# CHAVE_INTERNA_IMPRESSAO (16 primeiros hex do SHA-256), que o --verificar compara com o parâmetro do SSM.
#
# Pré-requisitos: gh autenticado com admin nos repositórios, aws com o perfil autoreparos, openssl, python3.
set -euo pipefail

MODO=aplicar
case "${1:-}" in
  "") ;;
  --simular) MODO=simular ;;
  --verificar) MODO=verificar ;;
  --verificar-idempotencia) MODO=verificar-idempotencia ;;
  --verificar-token-servico) MODO=verificar-token-servico ;;
  -h | --help) sed -n '2,18p' "$0"; exit 0 ;;
  *) echo "uso: $0 [--simular | --verificar | --verificar-idempotencia | --verificar-token-servico]" >&2; exit 2 ;;
esac

PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
CONTA="198903849859"
REGIAO="us-east-1"
ORG="Grupo78-PosTech-15SOAT"
PAI="${ORG}/AutoReparos"
TF_STATE_BUCKET="autoreparos-tfstate-${CONTA}"
TF_LOCK_TABLE="autoreparos-terraform-locks"
ROLE_PAI="arn:aws:iam::${CONTA}:role/autoreparos-github-pai"
REVISORES=(JoseMD12 EnricoGollner)
SEED_USER_EMAIL="admin@autoreparos.com"

SSM="/autoreparos/production"
P_JWT="${SSM}/cadastros/jwt-chave-privada-pem"
P_CHAVE="${SSM}/cadastros/chave-interna"
P_SEED="${SSM}/cadastros/seed-user-password"
P_GRAFANA="${SSM}/plataforma/grafana-admin-password"
P_SENDGRID_KEY="${SSM}/cadastros/sendgrid-api-key"
P_SENDGRID_EMAIL="${SSM}/cadastros/sendgrid-from-email"
P_SENDGRID_NOME="${SSM}/cadastros/sendgrid-from-name"
GERADOS=("$P_JWT" "$P_CHAVE" "$P_SEED" "$P_GRAFANA")

# servico -> repositório AutoReparos.<Repo>
SERVICOS=(os cobranca execucao cadastros)
declare -A REPO=([os]=OS [cobranca]=Cobranca [execucao]=Execucao [cadastros]=Cadastros)

FALHAS=0
falha() { echo "FALHA $*"; FALHAS=$((FALHAS + 1)); }
ok() { echo "ok $*"; }
aws_() { aws --profile "$PERFIL" --region "$REGIAO" "$@"; }
repo_de() { echo "${ORG}/AutoReparos.${REPO[$1]}"; }
role_deploy() { echo "arn:aws:iam::${CONTA}:role/autoreparos-deploy-$1"; }

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
chmod 700 "$TMP"

# ------------------------------------------------------------------------------------------- leitura

conferir_ferramentas() {
  local ferramenta
  for ferramenta in aws gh openssl python3 sha256sum; do
    command -v "$ferramenta" >/dev/null || { echo "$ferramenta não encontrado no PATH" >&2; exit 1; }
  done
  local conta
  conta="$(aws_ sts get-caller-identity --query Account --output text)"
  [ "$conta" = "$CONTA" ] || { echo "Perfil $PERFIL aponta para $conta, esperado $CONTA" >&2; exit 1; }
  gh auth status >/dev/null 2>&1 || { echo "gh não está autenticado" >&2; exit 1; }
}

parametro_existe() { aws_ ssm get-parameter --name "$1" --query Parameter.Name --output text >/dev/null 2>&1; }

# Valor exato do parâmetro (sem quebra de linha final).
valor_ssm() {
  local valor
  valor="$(aws_ ssm get-parameter --name "$1" --with-decryption --query Parameter.Value --output text)"
  printf '%s' "$valor"
}

# gh api imprime o corpo do erro mesmo com --jq: só devolve a saída quando a chamada deu certo.
ler_api() {
  local saida
  if saida="$(gh api "$@" 2>/dev/null)"; then printf '%s' "$saida"; else return 1; fi
}

impressao() { printf '%s' "$1" | sha256sum | cut -c1-16; }

# Variável de repositório (sem $3) ou de Environment ($3); vazio se não existe.
variavel() {
  local rota="repos/$1/actions/variables/$2"
  [ -n "${3:-}" ] && rota="repos/$1/environments/$3/variables/$2"
  ler_api "$rota" --jq .value || true
}

segredo_existe() {
  local rota="repos/$1/actions/secrets"
  [ -n "${3:-}" ] && rota="repos/$1/environments/$3/secrets"
  ler_api --paginate "$rota" --jq '.secrets[].name' | grep -qx "$2"
}

id_usuario() { gh api "users/$1" --jq .id; }

# ------------------------------------------------------------------------------------------- escrita

escrever() {
  if [ "$MODO" = simular ]; then
    echo "[simular] $*"
    return 0
  fi
  "$@"
}

# Gera o parâmetro SecureString só se ele não existe; put-parameter sem --overwrite recusa sobrescrever.
gerar_parametro() {
  local nome="$1" gerador="$2"
  if parametro_existe "$nome"; then
    echo "mantido $nome (já existe)"
    return 0
  fi
  if [ "$MODO" = simular ]; then
    echo "[simular] geraria $nome com $gerador"
    return 0
  fi
  local valor
  valor="$("$gerador")"
  (umask 077 && NOME="$nome" VALOR="$valor" python3 -c 'import json, os
print(json.dumps({"Name": os.environ["NOME"], "Value": os.environ["VALOR"], "Type": "SecureString",
                  "Tier": "Standard", "Description": "Gerado por scripts/infra/configurar-github.sh (D-56)"}))' \
    > "$TMP/parametro.json")
  unset valor
  aws_ ssm put-parameter --cli-input-json "file://$TMP/parametro.json" >/dev/null
  rm -f "$TMP/parametro.json"
  echo "gerado $nome"
}

gerar_chave_jwt() { openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 2>/dev/null; }
gerar_chave_interna() { openssl rand -hex 32; }
# Atende à política padrão do ASP.NET Identity (maiúscula, minúscula, dígito e símbolo).
gerar_senha_inicial() { printf 'Ar%s!9' "$(openssl rand -hex 16)"; }
gerar_senha_grafana() { openssl rand -hex 24; }

definir_variavel() { # repo nome valor [environment]
  local args=(variable set "$2" --body "$3" -R "$1")
  [ -n "${4:-}" ] && args+=(--env "$4")
  escrever gh "${args[@]}"
}

# O valor vai pela entrada padrão do gh, nunca pela linha de comando.
definir_segredo_do_ssm() { # repo nome parametro [environment]
  local args=(secret set "$2" -R "$1")
  [ -n "${4:-}" ] && args+=(--env "$4")
  if [ "$MODO" = simular ]; then
    echo "[simular] gh ${args[*]} < $3"
    return 0
  fi
  valor_ssm "$3" | gh "${args[@]}"
}

definir_segredo_literal() { # repo nome valor [environment]
  local args=(secret set "$2" -R "$1")
  [ -n "${4:-}" ] && args+=(--env "$4")
  if [ "$MODO" = simular ]; then
    echo "[simular] gh ${args[*]}"
    return 0
  fi
  printf '%s' "$3" | gh "${args[@]}"
}

# Environment com política de branch só na main; com revisores quando pedidos.
garantir_environment() { # repo environment com-revisores(sim|nao)
  local repo="$1" ambiente="$2" revisores="$3" corpo
  if [ "$revisores" = sim ]; then
    local usuario lista=""
    for usuario in "${REVISORES[@]}"; do lista+="${lista:+,}{\"type\":\"User\",\"id\":$(id_usuario "$usuario")}"; done
    corpo="{\"reviewers\":[${lista}],"
    corpo+='"deployment_branch_policy":{"protected_branches":false,"custom_branch_policies":true}}'
  else
    corpo='{"deployment_branch_policy":{"protected_branches":false,"custom_branch_policies":true}}'
  fi
  if [ "$MODO" = simular ]; then
    echo "[simular] PUT repos/$repo/environments/$ambiente $corpo"
  else
    gh api -X PUT "repos/$repo/environments/$ambiente" --input - <<< "$corpo" >/dev/null
  fi
  local politicas id tipo nome
  politicas="$(ler_api "repos/$repo/environments/$ambiente/deployment-branch-policies" \
    --jq '.branch_policies[] | "\(.id) \(.type) \(.name)"' || true)"
  if ! grep -qx '[0-9]* branch main' <<< "$politicas"; then
    escrever gh api -X POST "repos/$repo/environments/$ambiente/deployment-branch-policies" \
      -f name=main -f type=branch --silent
  fi
  while read -r id tipo nome; do
    [ -z "$id" ] && continue
    if [ "$tipo $nome" != "branch main" ]; then
      escrever gh api -X DELETE "repos/$repo/environments/$ambiente/deployment-branch-policies/$id" --silent
    fi
  done <<< "$politicas"
}

aplicar() {
  echo "== SSM: segredos gerados uma vez"
  gerar_parametro "$P_JWT" gerar_chave_jwt
  gerar_parametro "$P_CHAVE" gerar_chave_interna
  gerar_parametro "$P_SEED" gerar_senha_inicial
  gerar_parametro "$P_GRAFANA" gerar_senha_grafana
  local p
  for p in "$P_SENDGRID_KEY" "$P_SENDGRID_EMAIL" "$P_SENDGRID_NOME"; do
    parametro_existe "$p" || { echo "Parâmetro $p ausente: grave o SendGrid no SSM antes" >&2; exit 1; }
  done
  local impressao_chave=""
  if [ "$MODO" = aplicar ] || parametro_existe "$P_CHAVE"; then
    impressao_chave="$(impressao "$(valor_ssm "$P_CHAVE")")"
  fi

  echo "== $PAI"
  definir_variavel "$PAI" AWS_ACCOUNT_ID "$CONTA"
  definir_variavel "$PAI" AWS_REGION "$REGIAO"
  definir_variavel "$PAI" TF_STATE_BUCKET "$TF_STATE_BUCKET"
  definir_variavel "$PAI" TF_LOCK_TABLE "$TF_LOCK_TABLE"
  local ambiente
  for ambiente in production aws-bootstrap; do
    garantir_environment "$PAI" "$ambiente" nao
    # Variável para os workflows do pai e para o --verificar; o segredo continua para o bootstrap-aws.yml.
    definir_variavel "$PAI" AWS_ROLE_ARN "$ROLE_PAI" "$ambiente"
    definir_segredo_literal "$PAI" AWS_ROLE_ARN "$ROLE_PAI" "$ambiente"
  done
  definir_segredo_do_ssm "$PAI" CHAVE_INTERNA "$P_CHAVE" production
  definir_variavel "$PAI" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production
  definir_segredo_do_ssm "$PAI" GRAFANA_ADMIN_PASSWORD "$P_GRAFANA" production

  local servico repo
  for servico in "${SERVICOS[@]}"; do
    repo="$(repo_de "$servico")"
    echo "== $repo"
    definir_variavel "$repo" AWS_REGION "$REGIAO"
    definir_variavel "$repo" TF_STATE_BUCKET "$TF_STATE_BUCKET"
    definir_variavel "$repo" TF_LOCK_TABLE "$TF_LOCK_TABLE"
    garantir_environment "$repo" production sim
    definir_variavel "$repo" AWS_ROLE_ARN "$(role_deploy "$servico")" production
  done

  repo="$(repo_de cadastros)"
  definir_segredo_do_ssm "$repo" JWT_CHAVE_PRIVADA_PEM "$P_JWT" production
  definir_segredo_do_ssm "$repo" CHAVE_INTERNA "$P_CHAVE" production
  definir_variavel "$repo" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production
  definir_segredo_do_ssm "$repo" SEED_USER_PASSWORD "$P_SEED" production
  definir_segredo_do_ssm "$repo" SENDGRID_API_KEY "$P_SENDGRID_KEY" production
  definir_variavel "$repo" SENDGRID_FROM_EMAIL "$(valor_ssm "$P_SENDGRID_EMAIL")" production
  definir_variavel "$repo" SENDGRID_FROM_NAME "$(valor_ssm "$P_SENDGRID_NOME")" production
  definir_variavel "$repo" SEED_USER_EMAIL "$SEED_USER_EMAIL" production

  repo="$(repo_de os)"
  definir_segredo_do_ssm "$repo" CHAVE_INTERNA "$P_CHAVE" production
  definir_variavel "$repo" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production

  gerar_token_servico
  echo "Concluído ($MODO). Confira com: $0 --verificar"
}

# D-58: JWT de serviço da Cobrança, assinado com a chave do Cadastros. O emissor é a URL do API Gateway,
# que só existe depois do provision-aws.yml; antes disso o passo é pulado. Rodar de novo renova os 30 dias.
gerar_token_servico() {
  local repo emissor
  repo="$(repo_de cobranca)"
  if ! parametro_existe "${SSM}/apigateway/endpoint"; then
    echo "pulado TOKEN_SERVICO da Cobrança: ${SSM}/apigateway/endpoint ainda não existe (rode de novo depois do provision-aws.yml)"
    return 0
  fi
  if [ "$MODO" = simular ]; then
    echo "[simular] gh secret set TOKEN_SERVICO -R $repo --env production < token assinado com $P_JWT"
    return 0
  fi
  emissor="$(valor_ssm "${SSM}/apigateway/endpoint")"
  (umask 077 && valor_ssm "$P_JWT" > "$TMP/chave.pem")
  python3 "$(dirname "$0")/token_servico.py" --chave "$TMP/chave.pem" --emissor "$emissor" \
    | gh secret set TOKEN_SERVICO -R "$repo" --env production
  rm -f "$TMP/chave.pem"
  echo "gerado TOKEN_SERVICO da Cobrança (emissor $emissor, 30 dias)"
}

# ------------------------------------------------------------------------------------------- C31

conferir_variavel() { # repo nome esperado [environment]
  local atual
  atual="$(variavel "$1" "$2" "${4:-}")"
  if [ "$atual" = "$3" ]; then ok "$1 ${4:+[$4] }$2"; else falha "$1 ${4:+[$4] }$2 = '${atual}', esperado '$3'"; fi
}

conferir_segredo() { # repo nome [environment]
  if segredo_existe "$1" "$2" "${3:-}"; then ok "$1 ${3:+[$3] }segredo $2"; else falha "$1 ${3:+[$3] }segredo $2 ausente"; fi
}

conferir_environment() { # repo environment com-revisores(sim|nao)
  local repo="$1" ambiente="$2" revisores="$3" politica
  if ! politica="$(ler_api "repos/$repo/environments/$ambiente" --jq '.deployment_branch_policy | "\(.protected_branches) \(.custom_branch_policies)"')"; then
    falha "$repo Environment $ambiente ausente"
    return 0
  fi
  if [ "$politica" = "false true" ] &&
    [ "$(ler_api "repos/$repo/environments/$ambiente/deployment-branch-policies" \
      --jq '[.branch_policies[] | "\(.type) \(.name)"] | join(",")')" = "branch main" ]; then
    ok "$repo [$ambiente] só a branch main"
  else
    falha "$repo [$ambiente] política de branch diferente de só main"
  fi
  if [ "$revisores" = sim ]; then
    local atuais esperados
    atuais="$(ler_api "repos/$repo/environments/$ambiente" \
      --jq '[.protection_rules[]? | select(.type == "required_reviewers") | .reviewers[].reviewer.login] | sort | join(",")')"
    esperados="$(printf '%s\n' "${REVISORES[@]}" | sort | paste -sd, -)"
    if [ "$atuais" = "$esperados" ]; then ok "$repo [$ambiente] revisores $atuais"; else falha "$repo [$ambiente] revisores '$atuais', esperado '$esperados'"; fi
  fi
}

verificar() {
  local impressao_chave="" p
  for p in "${GERADOS[@]}"; do
    if parametro_existe "$p"; then ok "SSM $p"; else falha "SSM $p ausente"; fi
  done
  parametro_existe "$P_CHAVE" && impressao_chave="$(impressao "$(valor_ssm "$P_CHAVE")")"

  conferir_variavel "$PAI" AWS_ACCOUNT_ID "$CONTA"
  conferir_variavel "$PAI" TF_STATE_BUCKET "$TF_STATE_BUCKET"
  conferir_variavel "$PAI" TF_LOCK_TABLE "$TF_LOCK_TABLE"
  conferir_variavel "$PAI" AWS_REGION "$REGIAO"
  local ambiente nome atual
  for ambiente in production aws-bootstrap; do
    conferir_environment "$PAI" "$ambiente" nao
    conferir_variavel "$PAI" AWS_ROLE_ARN "$ROLE_PAI" "$ambiente"
    conferir_segredo "$PAI" AWS_ROLE_ARN "$ambiente"
    # Variável de Environment com o mesmo nome venceria a do repositório.
    for nome in AWS_ACCOUNT_ID TF_STATE_BUCKET; do
      atual="$(variavel "$PAI" "$nome" "$ambiente")"
      [ -z "$atual" ] || [ "$atual" = "$(variavel "$PAI" "$nome")" ] || falha "$PAI [$ambiente] $nome = '$atual' sobrepõe o do repositório"
    done
  done
  conferir_segredo "$PAI" CHAVE_INTERNA production
  conferir_variavel "$PAI" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production
  conferir_segredo "$PAI" GRAFANA_ADMIN_PASSWORD production

  local servico repo
  for servico in "${SERVICOS[@]}"; do
    repo="$(repo_de "$servico")"
    conferir_variavel "$repo" AWS_REGION "$REGIAO"
    conferir_variavel "$repo" TF_STATE_BUCKET "$TF_STATE_BUCKET"
    conferir_variavel "$repo" TF_LOCK_TABLE "$TF_LOCK_TABLE"
    conferir_environment "$repo" production sim
    conferir_variavel "$repo" AWS_ROLE_ARN "$(role_deploy "$servico")" production
  done

  repo="$(repo_de cadastros)"
  for nome in JWT_CHAVE_PRIVADA_PEM CHAVE_INTERNA SEED_USER_PASSWORD SENDGRID_API_KEY; do
    conferir_segredo "$repo" "$nome" production
  done
  conferir_variavel "$repo" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production
  conferir_variavel "$repo" SENDGRID_FROM_EMAIL "$(valor_ssm "$P_SENDGRID_EMAIL" 2>/dev/null || true)" production
  conferir_variavel "$repo" SENDGRID_FROM_NAME "$(valor_ssm "$P_SENDGRID_NOME" 2>/dev/null || true)" production
  conferir_variavel "$repo" SEED_USER_EMAIL "$SEED_USER_EMAIL" production

  repo="$(repo_de os)"
  conferir_segredo "$repo" CHAVE_INTERNA production
  conferir_variavel "$repo" CHAVE_INTERNA_IMPRESSAO "$impressao_chave" production

  if [ -z "$impressao_chave" ]; then falha "sem $P_CHAVE não há como conferir a CHAVE_INTERNA"; fi
  [ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
  echo "OK configurar-github --verificar: pai e 4 serviços configurados"
}

# ------------------------------------------------------------------------------------------- C32

verificar_idempotencia() {
  local p dados
  for p in "${GERADOS[@]}"; do
    if ! dados="$(aws_ ssm get-parameter --name "$p" --query 'Parameter.[Type,Version]' --output text 2>/dev/null)"; then
      falha "SSM $p ausente"
      continue
    fi
    if [ "$dados" = "$(printf 'SecureString\t1')" ]; then ok "SSM $p SecureString versão 1"; else falha "SSM $p: $dados, esperado SecureString versão 1"; fi
  done
  [ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
  echo "OK configurar-github --verificar-idempotencia: ${#GERADOS[@]} segredos gerados uma vez só"
}

# ------------------------------------------------------------------------------------------- C37

verificar_token_servico() {
  conferir_segredo "$(repo_de cobranca)" TOKEN_SERVICO production
  [ "$FALHAS" -eq 0 ] || { echo "$FALHAS falha(s)"; exit 1; }
  echo "OK configurar-github --verificar-token-servico: Cobrança com TOKEN_SERVICO no Environment production"
}

conferir_ferramentas
case "$MODO" in
  aplicar | simular) aplicar ;;
  verificar) verificar ;;
  verificar-idempotencia) verificar_idempotencia ;;
  verificar-token-servico) verificar_token_servico ;;
esac
