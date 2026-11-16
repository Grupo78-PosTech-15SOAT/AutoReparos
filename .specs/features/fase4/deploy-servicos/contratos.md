# Contratos de build - deploy-servicos

Combinados de implementação entre os lotes do S2 ao S5 (16/11). Não substituem o [plano](./plan.md) nem os [checks](./checks.md): fixam os nomes e formatos que atravessam repositórios, para que serviços e plataforma sejam construídos em paralelo. Quem precisar mudar algo daqui avisa o orquestrador antes de mudar.

## Nomes

`<servico>` ∈ `os`, `cobranca`, `execucao`, `cadastros` (minúsculo, sem acento); `<Repo>` ∈ `OS`, `Cobranca`, `Execucao`, `Cadastros`.

| Coisa | Nome |
| --- | --- |
| Conta / região | `198903849859` / `us-east-1` |
| Namespace e ServiceAccount | `<servico>` / `<servico>` |
| Release Helm e Service Kubernetes | `<servico>`, porta `8080` |
| ECR | `autoreparos-<servico>`, tag = SHA do commit |
| Role IRSA (criada pelo Terraform do serviço com o boundary) | `autoreparos-<servico>-irsa` |
| Role de deploy (criada pela plataforma) | `autoreparos-deploy-<servico>` |
| Permissions boundary | `autoreparos-servico-boundary` (política gerenciada) |
| Filas | `autoreparos-<consumidor>-<finalidade>` e `...-dlq` |
| Tabelas DynamoDB (OS) | `autoreparos-os-saga`, `autoreparos-os-saga-eventos` |
| Segredo da database | `autoreparos/production/<servico>/database` |
| Database / usuário | `os_db`/`os_app`, `cobranca_db`/`cobranca_app`, `execucao_db`/`execucao_app`; Cadastros: `autoreparos_db` (D-34, nome lido de `/autoreparos/production/database/name`) com o usuário `cadastros_app`, dono da database |
| State | bucket `autoreparos-tfstate-198903849859` (`TF_STATE_BUCKET`), chave `servicos/<servico>/production.tfstate`, lock `autoreparos-terraform-locks` (`TF_LOCK_TABLE`); backend parcial com `-backend-config` |
| Secret Kubernetes da connection string | `<servico>-database`, criado pelo job `deploy` antes do Helm |
| Lambda | `autoreparos-provisionador-db` |

## Parâmetros SSM publicados pela plataforma (`/autoreparos/production/...`)

Já existem no Terraform legado e precisam continuar publicados pelos stacks que o `provision-aws.yml` aplica: `vpc_id`, `subnets/private`, `security-groups/eks-nodes`, `apigateway/endpoint`, `database/endpoint`, `database/address`, `database/name`, `database/secret_arn`. Novos: `eks/cluster_name`, `eks/oidc_provider_arn`, `eks/oidc_provider_url` (**sem** `https://`, pronto para chave de condição) e `provisionador-db/url` (Function URL). Todos `String`. Os serviços leem só por `data "aws_ssm_parameter"` (D-53).

## Segredo da database do serviço

O Terraform do serviço gera a senha (`random_password`, sem caracteres que quebrem connection string) e grava em `autoreparos/production/<servico>/database` o JSON:

```json
{"username": "os_app", "password": "...", "dbname": "os_db", "host": "<database/address>", "port": 5432, "connectionString": "Host=...;Port=5432;Database=os_db;Username=os_app;Password=...;<mesmos parâmetros de SSL do segredo do RDS>"}
```

O job `deploy` lê `.connectionString` e cria o Secret `<servico>-database` com a chave que o serviço já usa (`ConnectionStrings__OrdemServico`, `ConnectionStrings__Billing`, `ConnectionStrings__Execucao`, `ConnectionStrings__DbConnection`). O provisionador lê `username`, `password` e `dbname`.

## Provisionador (`autoreparos-provisionador-db`)

- Function URL `AuthType = AWS_IAM`; política de recurso permite `lambda:InvokeFunctionUrl` (e `lambda:InvokeFunction` com `lambda:InvokedViaFunctionUrl = true`, exigido pela AWS para URLs novas) só às 4 roles de deploy.
- Chamada: `POST <url>` assinada SigV4 (serviço `lambda`), corpo vazio ou `{}`. O serviço é deduzido de `requestContext.authorizer.iam.userArn` = `arn:aws:sts::198903849859:assumed-role/autoreparos-deploy-<servico>/<sessão>`; qualquer outro chamador → `403` sem abrir conexão com o banco.
- Faz, de forma idempotente: cria o usuário se não existe (com a senha do segredo; se já existe, **não** troca a senha), cria `<servico>_db` com dono `<servico>_app` se não existe (Cadastros: `ALTER DATABASE autoreparos_db OWNER TO cadastros_app`). Responde `200` com `{"servico", "database", "usuario", "criado": {"database": bool, "usuario": bool}}`.
- Lê a credencial admin `autoreparos/production/database/credentials` (via `database/secret_arn`) e os segredos `autoreparos/production/*/database`; é a única role com acesso ao admin.

## Políticas IAM como modelo

Toda política que o simulador avalia fica num arquivo `.json.tftpl` renderizado com `templatefile()` e usa só `${nome}` simples (sem `%{}` nem funções), para que `scripts/specs/simular_iam.py` a renderize sem Terraform. IRSA: `infra/terraform/politicas/irsa.json.tftpl` de cada serviço. Deploy e boundary: `terraform/stacks/base/politicas/deploy-servico.json.tftpl` e `servico-boundary.json.tftpl` no Infra.K8s.

## Variáveis e segredos do GitHub (escritos por `scripts/infra/configurar-github.sh`)

- Cada serviço, nível repositório: variáveis `AWS_REGION=us-east-1`, `TF_STATE_BUCKET`, `TF_LOCK_TABLE`. Environment `production`: revisores José e Enrico, só a branch `main`, variável `AWS_ROLE_ARN=arn:aws:iam::198903849859:role/autoreparos-deploy-<servico>`.
- Cadastros, Environment `production`, segredos: `JWT_CHAVE_PRIVADA_PEM`, `CHAVE_INTERNA`, `SEED_USER_PASSWORD`, `SENDGRID_API_KEY`; variáveis `SENDGRID_FROM_EMAIL`, `SENDGRID_FROM_NAME`, `SEED_USER_EMAIL`.
- OS, Environment `production`, segredo `CHAVE_INTERNA` (o OS chama o Cadastros com `Cadastros:ChaveInterna`; `Cadastros:BaseUrl` = `http://cadastros.cadastros.svc.cluster.local:8080`).
- Origem: `SecureString` em `/autoreparos/production/cadastros/{jwt-chave-privada-pem,chave-interna,seed-user-password}` (gerados uma vez, versão 1) e `sendgrid-*` (já existem). A `CHAVE_INTERNA` do pai sai do mesmo parâmetro.

## Chart e job `deploy`

- O chart só tem objetos que a `AmazonEKSEditPolicy` com escopo de namespace permite: sem `Namespace`, `Role`, `RoleBinding` ou objetos de cluster.
- Ingress no grupo `autoreparos-internal` com as anotações do Ingress atual do guarda-chuva (`scheme: internal`, `target-type: ip`, healthcheck `/health`); `group.order`: OS 10, Cobrança 20, Execução 30, Cadastros 100, Web 1000. O Ingress `/internal` do Cadastros (grupo `autoreparos-cadastros-interno`) vai para o chart do Cadastros.
- `Authentication__Authority` dos serviços e o emissor do Cadastros recebem `/autoreparos/production/apigateway/endpoint`, lido pelo job `deploy`.
