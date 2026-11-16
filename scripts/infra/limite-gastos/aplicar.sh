#!/usr/bin/env bash
# Teto de gasto da conta AWS da Fase 4 (D-52). Idempotente: pode rodar de novo para atualizar.
#
#   ALERTA_EMAIL=<e-mail> scripts/infra/limite-gastos/aplicar.sh
#
# Cria o orçamento total `autoreparos-limite-total` (US$ 100 de 01/11/2026 a 31/10/2027, sem
# descontar créditos) com avisos por e-mail em US$ 50 e US$ 90 e, em US$ 80, o desligamento
# automático: Budgets -> SNS `autoreparos-limite-gastos` -> Lambda `autoreparos-limite-gastos`.
# Não fica no Terraform da plataforma de propósito: o `destroy-aws.yml` não pode apagar o freio.
set -euo pipefail

PERFIL="${AWS_PROFILE_FASE4:-autoreparos}"
CONTA="198903849859"
REGIAO="us-east-1"
ORCAMENTO="autoreparos-limite-total"
LIMITE_TOTAL="100"
LIMITE_DESLIGAR="${LIMITE_DESLIGAR:-80}"
NOME="autoreparos-limite-gastos"
ROLE="${NOME}-lambda"
POLITICA_CONGELADO="autoreparos-congelado"
: "${ALERTA_EMAIL:?defina ALERTA_EMAIL com o e-mail que recebe os avisos}"

aws_() { aws --profile "$PERFIL" --region "$REGIAO" "$@"; }
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TMP="$(mktemp -d)"; trap 'rm -rf "$TMP"' EXIT

conta_atual="$(aws_ sts get-caller-identity --query Account --output text)"
[ "$conta_atual" = "$CONTA" ] || { echo "Perfil $PERFIL aponta para $conta_atual, esperado $CONTA" >&2; exit 1; }

echo "1/6 política $POLITICA_CONGELADO"
cat > "$TMP/congelado.json" <<'JSON'
{"Version":"2012-10-17","Statement":[{"Sid":"TetoDeGastoAtingido","Effect":"Deny","Action":[
 "ec2:RunInstances","ec2:StartInstances","ec2:CreateNatGateway","ec2:AllocateAddress","ec2:CreateVpcEndpoint",
 "eks:CreateCluster","eks:CreateNodegroup","eks:UpdateNodegroupConfig","eks:CreateFargateProfile",
 "rds:CreateDBInstance","rds:StartDBInstance","rds:RestoreDBInstanceFromDBSnapshot","rds:RestoreDBInstanceToPointInTime",
 "elasticloadbalancing:CreateLoadBalancer","autoscaling:CreateAutoScalingGroup","autoscaling:UpdateAutoScalingGroup",
 "autoscaling:SetDesiredCapacity","lambda:CreateFunction","dynamodb:CreateTable","sqs:CreateQueue"],"Resource":"*"}]}
JSON
POLITICA_ARN="arn:aws:iam::${CONTA}:policy/${POLITICA_CONGELADO}"
aws_ iam get-policy --policy-arn "$POLITICA_ARN" >/dev/null 2>&1 || \
  aws_ iam create-policy --policy-name "$POLITICA_CONGELADO" --policy-document "file://$TMP/congelado.json" \
    --description "Anexada pelo desligamento automatico quando o gasto passa do teto (D-52)" >/dev/null

echo "2/6 role $ROLE"
cat > "$TMP/confianca.json" <<'JSON'
{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Service":"lambda.amazonaws.com"},"Action":"sts:AssumeRole"}]}
JSON
aws_ iam get-role --role-name "$ROLE" >/dev/null 2>&1 || \
  aws_ iam create-role --role-name "$ROLE" --assume-role-policy-document "file://$TMP/confianca.json" >/dev/null
aws_ iam attach-role-policy --role-name "$ROLE" --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
cat > "$TMP/permissoes.json" <<JSON
{"Version":"2012-10-17","Statement":[
 {"Effect":"Allow","Action":["eks:ListClusters","eks:DescribeCluster","eks:DeleteCluster","eks:ListNodegroups","eks:DescribeNodegroup",
   "eks:DeleteNodegroup","eks:ListFargateProfiles","eks:DeleteFargateProfile",
   "elasticloadbalancing:DescribeLoadBalancers","elasticloadbalancing:DeleteLoadBalancer",
   "ec2:DescribeNatGateways","ec2:DeleteNatGateway","ec2:DescribeAddresses","ec2:ReleaseAddress","ec2:DescribeInstances","ec2:TerminateInstances",
   "rds:DescribeDBInstances","rds:ModifyDBInstance","rds:DeleteDBInstance","rds:CreateDBSnapshot","rds:AddTagsToResource",
   "iam:ListRoles"],"Resource":"*"},
 {"Effect":"Allow","Action":"iam:AttachRolePolicy","Resource":"arn:aws:iam::${CONTA}:role/*",
   "Condition":{"ArnEquals":{"iam:PolicyARN":"${POLITICA_ARN}"}}},
 {"Effect":"Allow","Action":"lambda:InvokeFunction","Resource":"arn:aws:lambda:${REGIAO}:${CONTA}:function:${NOME}"}]}
JSON
aws_ iam put-role-policy --role-name "$ROLE" --policy-name desligar-ambiente --policy-document "file://$TMP/permissoes.json"

echo "3/6 lambda $NOME"
(cd "$DIR" && zip -q "$TMP/funcao.zip" desligar_ambiente.py)
AMBIENTE="Variables={POLITICA_CONGELADO_ARN=${POLITICA_ARN},ROLE_PROPRIA=${ROLE},REGIOES=${REGIAO},RDS_SNAPSHOT_FINAL=${RDS_SNAPSHOT_FINAL:-true}}"
if aws_ lambda get-function --function-name "$NOME" >/dev/null 2>&1; then
  aws_ lambda update-function-code --function-name "$NOME" --zip-file "fileb://$TMP/funcao.zip" >/dev/null
  aws_ lambda wait function-updated --function-name "$NOME"
  aws_ lambda update-function-configuration --function-name "$NOME" --environment "$AMBIENTE" --timeout 900 >/dev/null
else
  for _ in 1 2 3 4 5 6; do  # a role recém-criada leva alguns segundos para valer no Lambda
    aws_ lambda create-function --function-name "$NOME" --runtime python3.12 --handler desligar_ambiente.handler \
      --role "arn:aws:iam::${CONTA}:role/${ROLE}" --zip-file "fileb://$TMP/funcao.zip" --timeout 900 --memory-size 256 \
      --environment "$AMBIENTE" --description "Desliga o ambiente quando o gasto passa do teto (D-52)" >/dev/null 2>&1 && break
    sleep 10
  done
  aws_ lambda get-function --function-name "$NOME" >/dev/null
fi
aws_ lambda wait function-updated --function-name "$NOME"

echo "4/6 tópico SNS $NOME"
TOPICO="$(aws_ sns create-topic --name "$NOME" --query TopicArn --output text)"
cat > "$TMP/topico.json" <<JSON
{"Version":"2012-10-17","Statement":[
 {"Sid":"Budgets","Effect":"Allow","Principal":{"Service":"budgets.amazonaws.com"},"Action":"SNS:Publish","Resource":"${TOPICO}",
  "Condition":{"StringEquals":{"aws:SourceAccount":"${CONTA}"}}},
 {"Sid":"Conta","Effect":"Allow","Principal":{"AWS":"arn:aws:iam::${CONTA}:root"},"Action":["SNS:Publish","SNS:Subscribe","SNS:GetTopicAttributes"],"Resource":"${TOPICO}"}]}
JSON
aws_ sns set-topic-attributes --topic-arn "$TOPICO" --attribute-name Policy --attribute-value "file://$TMP/topico.json"
aws_ lambda add-permission --function-name "$NOME" --statement-id sns-limite-gastos --action lambda:InvokeFunction \
  --principal sns.amazonaws.com --source-arn "$TOPICO" >/dev/null 2>&1 || true
aws_ sns subscribe --topic-arn "$TOPICO" --protocol lambda \
  --notification-endpoint "arn:aws:lambda:${REGIAO}:${CONTA}:function:${NOME}" >/dev/null

echo "5/6 orçamento $ORCAMENTO"
cat > "$TMP/orcamento.json" <<JSON
{"BudgetName":"${ORCAMENTO}","BudgetLimit":{"Amount":"${LIMITE_TOTAL}","Unit":"USD"},"TimeUnit":"ANNUALLY","BudgetType":"COST",
 "TimePeriod":{"Start":"2026-11-01T00:00:00Z","End":"2027-10-31T23:59:59Z"},
 "CostTypes":{"IncludeCredit":false,"IncludeRefund":false,"IncludeTax":true,"IncludeSubscription":true,"IncludeSupport":true,
  "IncludeOtherSubscription":true,"IncludeRecurring":true,"IncludeUpfront":true,"IncludeDiscount":true,"UseBlended":false,"UseAmortized":false}}
JSON
email="{\"SubscriptionType\":\"EMAIL\",\"Address\":\"${ALERTA_EMAIL}\"}"
sns="{\"SubscriptionType\":\"SNS\",\"Address\":\"${TOPICO}\"}"
aviso() { echo "{\"Notification\":{\"NotificationType\":\"ACTUAL\",\"ComparisonOperator\":\"GREATER_THAN\",\"Threshold\":$1,\"ThresholdType\":\"ABSOLUTE_VALUE\"},\"Subscribers\":[$2]}"; }
echo "[$(aviso 50 "$email"),$(aviso "$LIMITE_DESLIGAR" "$email,$sns"),$(aviso 90 "$email")]" > "$TMP/avisos.json"
if aws_ budgets describe-budget --account-id "$CONTA" --budget-name "$ORCAMENTO" >/dev/null 2>&1; then
  aws_ budgets update-budget --account-id "$CONTA" --new-budget "file://$TMP/orcamento.json"
else
  aws_ budgets create-budget --account-id "$CONTA" --budget "file://$TMP/orcamento.json" \
    --notifications-with-subscribers "file://$TMP/avisos.json"
fi

echo "6/6 conferência"
aws_ budgets describe-notifications-for-budget --account-id "$CONTA" --budget-name "$ORCAMENTO" \
  --query 'Notifications[].[NotificationType,Threshold]' --output text
echo "Simulação: aws --profile $PERFIL lambda invoke --function-name $NOME --payload '{\"dryRun\":true}' --cli-binary-format raw-in-base64-out /dev/stdout"
