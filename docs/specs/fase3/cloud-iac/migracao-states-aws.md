# Inventário e migração dos Terraform states AWS

## Estado conhecido no checkout

Não há sessão AWS nem variáveis/roles de produção configuradas nesta sessão. Assim, **nenhum recurso da conta foi consultado** e a conta não foi tratada como vazia. O inventário remoto precisa ser coletado pelo workflow central, com credenciais OIDC, e revisado antes de liberar a primeira execução.

| State atual no código | Proprietário atual | Destino lógico | Situação antes da migração |
|---|---|---|---|
| `k8s/terraform.tfstate` | `AutoReparos.Infra.K8s/terraform` | Rede/EKS/ECR/SG em `network/production/terraform.tfstate`; add-ons em `platform/production/terraform.tfstate`; API Gateway em `gateway/production/terraform.tfstate` | Não consultado; migração bloqueada |
| `database/terraform.tfstate` | `AutoReparos.Infra.Database/terraform` | Continua Database/RDS | Não consultado |
| `lambda-auth/terraform.tfstate` | `AutoReparos.AuthLambda/infra` | Continua AuthLambda | Não consultado; o SG Lambda precisa sair deste state e ser transferido para `network` |
| `bootstrap/terraform.tfstate` | `AutoReparos.Infra.K8s/bootstrap` | Continua bootstrap | Não consultado; recuperado/migrado pelo workflow Bootstrap AWS |

Os states novos são separados por domínio. A API Gateway tem state próprio. RDS continua sendo propriedade exclusiva do repositório Database, e a função Lambda continua sendo propriedade exclusiva do repositório AuthLambda. A role de execução da Lambda pertence ao state da Lambda. O SG compartilhado é criado na rede e referenciado por RDS/Lambda; as regras de entrada do RDS continuam no state Database.

## Inventário obrigatório na AWS

O workflow `Provision AWS` registra, em modo somente leitura, VPCs, SGs, clusters EKS, repositórios ECR, instâncias RDS, funções Lambda e APIs Gateway visíveis na região/conta configuradas. Antes da migração, completar uma cópia desta tabela com IDs/ARNs reais, tags, state/key e endereço Terraform atual:

| Recurso real | ID/ARN | State e endereço atual | Novo state/endereço | Ação (manter/importar/mover) | Plano revisado por/data |
|---|---|---|---|---|---|
| VPC e subnets |  |  | `network/production` |  |  |
| EKS, node groups, IAM/KMS/OIDC |  |  | `network/production` |  |  |
| ECR API/Web |  |  | `network/production` |  |  |
| SG Lambda compartilhado |  |  | `network/production` |  |  |
| Add-ons, storage class, LBC |  |  | `platform/production` |  |  |
| RDS, subnet group, SG e secret |  |  | `database` |  |  |
| Lambda, role e log group |  |  | `lambda-auth` |  |  |
| API Gateway, VPC Link e integrações |  |  | `gateway/production` |  |  |
| Parâmetros SSM |  |  | Rede (IDs de rede) / Gateway (endpoint) |  |  |

Inventariar ainda recursos existentes fora do state, snapshots, volumes/PVCs, balanceadores e ENIs. Verificar account ID, região, owners e tags; um nome coincidente não comprova que o recurso pertence a este projeto. Exportar uma cópia de cada state, preservar versões S3 e restringir acesso aos arquivos: state e planos podem conter credenciais.

## Sequência segura de transferência

1. Desativar todos os writers antigos (os workflows filhos foram removidos/convertidos em CI nesta branch; aplicar a mesma mudança nas branches remotas após revisão).
2. Confirmar que nenhum `apply`/`destroy` está ativo e bloquear novas execuções.
3. Baixar cada state original sem imprimir seu conteúdo; guardar cópia cifrada e confirmar a versão S3.
4. Registrar o mapeamento acima. A transferência planejada do state misto usa os módulos já equivalentes: `module.vpc`, `module.eks` e `module.ecr` passam para `network`; `module.addons` para `platform`; `module.apigateway` para `gateway`. Os parâmetros SSM de rede vão para `network`; o parâmetro do endpoint vai para `gateway`.
5. Transferir o recurso `aws_security_group.lambda_sg[0]` do state Lambda para `aws_security_group.lambda` em `network` **antes** de qualquer apply. Atualizar o input de RDS para permitir o SG Lambda e o SG dos nós EKS. A função deve continuar usando o ID transferido.
6. Usar `terraform state mv` entre cópias locais dos states (ou importação por ID quando os endereços não coincidirem). Não editar JSON de state manualmente. `module.ecr` mantém o endereço API; o ECR Web é recurso novo e precisa ser verificado antes da criação/importação.
7. Empurrar cada state de destino com `terraform state push` apenas após validar serial/lineage, backend key, IDs e plano. Manter o state legado vazio e sua versão S3 de backup; não apagar o objeto nem a versão de backup.
8. Executar `terraform plan` em cada state e comparar todos os endereços/IDs com o inventário. Não aceitar substituições, destruições, mudanças de conta/região ou recriação de recursos de dados. Resolver qualquer drift manualmente e repetir o plano.
9. Só então executar `Provision AWS` com Environment `production` aprovado. Fazer deploy sem migration automática no arranque: a migration Job precisa terminar antes do rollout.

Não executar `state mv`, `state push`, `apply` ou `destroy` até a tabela real estar preenchida, as cópias dos states confirmadas e os planos de destino revisados. O workflow central falha se ainda houver recursos no state misto `k8s/terraform.tfstate`.

## Backend e lock

O bootstrap mantém state em `bootstrap/terraform.tfstate`, bucket versionado e DynamoDB lock table. As camadas usam chaves distintas: `network/production/terraform.tfstate`, `platform/production/terraform.tfstate`, `database/terraform.tfstate`, `lambda-auth/terraform.tfstate` e `gateway/production/terraform.tfstate`. A tabela DynamoDB é mantida durante esta migração para não misturar mudança de locking com mudança de ownership. O locking S3 nativo (`use_lockfile`) fica para uma migração coordenada posterior.

As versões de objetos S3 são cópias de recuperação; elas não substituem o backup explícito dos states antes da migração. Os arquivos de plano são efêmeros no runner e nunca são publicados como artifacts.
