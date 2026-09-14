# Especificação Técnica Fase 3 - Track A: Infraestrutura Cloud, IaC Terraform, API Gateway & Multi-Repo CI/CD

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3  
> **Papel / Responsável:** **Engenheiro de Infraestrutura Cloud & DevOps (Track A)**  
> **Repositórios de Atuação Principal:**
> 1. [`AutoReparos.Infra.Database`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database) (IaC AWS RDS PostgreSQL 16)
> 2. [`AutoReparos.Infra.K8s`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s) (IaC AWS EKS, Ingress & AWS API Gateway)
> 3. Repositório Pai / Orquestrador (`AutoReparos`) - Atualização de submódulos e governança  
> **Status:** Pronto para Implementação Paralela e Assíncrona  
> **Documento Complementar:** [`fase3_spec_track_b_observability_docs.md`](file:///home/josemd12/Code/AutoReparos/.tmp/fase3_spec_track_b_observability_docs.md) (Track B: Observabilidade, Métricas, Dashboards & Arquitetura)

---

## 1. Visão Geral & Estratégia de Paralelismo

Esta especificação define o escopo integral de **Infraestrutura como Código (IaC), Nuvem AWS, Roteamento via API Gateway e Pipelines de CI/CD** da Fase 3. 

### Princípios de Trabalho Paralelo e Assíncrono com o Track B:
1. **Isolamento de Código:** Todo o desenvolvimento do Track A ocorre nos repositórios `AutoReparos.Infra.Database` e `AutoReparos.Infra.K8s` (com branch específica no repositório pai para apontamento de submódulos). O Track B atuará em `AutoReparos.App` e `AutoReparos.Domain`/`Application`, eliminando conflitos de merge no Git.
2. **Contrato de Interface Estável (Zero Bloqueio):**
   - **Banco de Dados:** PostgreSQL 16 na porta `5432`, database `autoreparos_db`, com segredo de credenciais persistido no AWS Secrets Manager.
   - **API Gateway (HTTP API):**
     - Rota Serverless: `POST /auth/cliente` -> Integração AWS_PROXY com a Lambda `AutoReparos.AuthLambda`.
     - Rota Aplicação K8s: `ANY /api/{proxy+}` -> Integração HTTP Proxy / VPC Link com o Ingress/NLB do cluster EKS.
     - Rota de Healthcheck: `GET /health` -> EKS.
   - **OpenTelemetry / Observabilidade:** O cluster EKS exporta portas `4317` (gRPC) e `4318` (HTTP) para o OTel Collector e aceita chaves de API injetadas via Secrets/Environment Variables (`NEW_RELIC_LICENSE_KEY` ou `DD_API_KEY`).
3. **Independência de Validação Local:** Todos os planos Terraform podem ser validados e simulados com `terraform fmt`, `terraform validate`, `tflint` ou LocalStack sem depender da entrega final de observabilidade do Track B.

---

## 2. Requisitos Mandatórios da Banca Cobertos pelo Track A

Conforme a especificação oficial (`docs/tech-challenge/13SOAT - Fase 3 - Tech Challenge.pdf`):

| Requisito do Tech Challenge | Componente / Repositório | Entregável do Track A |
| :--- | :--- | :--- |
| **1. API Gateway** | `AutoReparos.Infra.K8s` | AWS API Gateway (HTTP API v2) roteando `/auth/cliente` para Lambda e `/api/*` para o EKS Ingress. |
| **2. Segregação em 4 Repositórios Git** | GitHub Organização / Submódulos | Repositórios `AutoReparos.Infra.Database` e `AutoReparos.Infra.K8s` 100% autônomos com CI/CD. |
| **3. Regras de Proteção de Branch** | Configuração GitHub / Repositórios | Branch `main` protegida (sem push direto, PR obrigatório), deploy automático para homolog/prod. |
| **4. Banco de Dados Gerenciado** | `AutoReparos.Infra.Database` | AWS RDS PostgreSQL 16 provisionado via Terraform com Subnet Groups privados, SG e Secrets Manager. |
| **5. Cluster Kubernetes Escalável** | `AutoReparos.Infra.K8s` | Cluster AWS EKS com Node Groups gerenciados, HPA configurado, VPC e Ingress Controller. |
| **6. Documentação Arquitetural (Infra)** | `docs/architecture/` | **RFC 001** (Cloud & 4 Repositórios), **RFC 002** (Estratégia RDS vs StatefulSet), **ADR 001** (API Gateway) e Diagrama de Componentes Cloud. |
| **7. Acesso da Banca (`soat-architecture`)** | 4 Repositórios GitHub | Adição do usuário `soat-architecture` com permissão de colaborador em todos os 4 repositórios. |

---

## 3. Arquitetura Alvo de Infraestrutura (Track A)

```mermaid
flowchart TD
    subgraph Internet ["Borda Externa / Internet"]
        ClientPortal["Portal do Cliente (Browser)"]
        OperatorWeb["Painel Operador / Swagger"]
    end

    subgraph AWS_Cloud ["AWS Cloud (Região: us-east-1)"]
        subgraph Edge_Routing ["Camada de Borda & Roteamento"]
            APIGW["AWS API Gateway (HTTP API v2)"]
            APIGW_Route_Auth["Rota: POST /auth/cliente"]
            APIGW_Route_API["Rota: ANY /api/{proxy+}"]
            APIGW_Route_Health["Rota: GET /health"]
        end

        subgraph Serverless_Layer ["Camada Serverless"]
            AuthLambda["AWS Lambda (.NET 10 C#)<br/>AutoReparos.AuthLambda"]
        end

        subgraph VPC_Infra ["VPC Dedicada: 10.0.0.0/16"]
            subgraph Public_Subnets ["Subnets Públicas (AZ 1a, 1b)"]
                NAT_GW["NAT Gateway"]
                NLB["Network Load Balancer (AWS NLB)"]
            end

            subgraph Private_K8s_Subnets ["Subnets Privadas K8s (AZ 1a, 1b)"]
                EKS["AWS EKS Cluster v1.30+"]
                Ingress["Ingress Nginx Controller"]
                AppPods["AutoReparos.App Pods (.NET 10 API)<br/>HPA: Min 2 / Max 10"]
                OTelCollector["OTel Collector Pod / DaemonSet"]
            end

            subgraph Private_DB_Subnets ["Subnets Privadas RDS (AZ 1a, 1b)"]
                RDS["AWS RDS PostgreSQL 16<br/>Multi-AZ / Storage Encriptado"]
                DBSubnetGroup["DB Subnet Group Privado"]
            end

            subgraph Security_Layer ["Segurança & Segredos"]
                SecretsMgr["AWS Secrets Manager<br/>(Master Password & ConnectionString)"]
                RDS_SG["Security Group RDS (Port 5432)<br/>Permite apenas EKS Nodes SG & Lambda SG"]
            end
        end
    end

    ClientPortal -->|1. POST /auth/cliente| APIGW
    APIGW --> APIGW_Route_Auth
    APIGW_Route_Auth -->|Payload {cpf, email}| AuthLambda
    AuthLambda -->|Valida status cliente| RDS

    ClientPortal -->|2. GET /api/clientes/meus-veiculos<br/>Bearer JWT| APIGW
    OperatorWeb -->|3. Requisições Operacionais /api/*| APIGW
    APIGW --> APIGW_Route_API
    APIGW_Route_API -->|VPC Link| NLB
    NLB --> Ingress
    Ingress --> AppPods
    AppPods -->|Queries EF Core| RDS
    AppPods -->|Métricas & Traces| OTelCollector
```

---

## 4. Contrato de Interface Técnica com o Track B

Para garantir zero fricção entre o Track A e o Track B, os seguintes contratos devem ser rigorosamente preservados:

### 4.1. Variáveis de Ambiente & Segredos Injetados no Backend
| Variável | Origem no Terraform / Infra | Consumo no Track B |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` | Construída a partir dos outputs do RDS: `Host=${endpoint};Port=5432;Database=autoreparos_db;Username=autoreparos_admin;Password=${secret}` | `AppDbContext.cs` / EF Core |
| `JWT_SECRET` | Segredo gerado no Secrets Manager / variável de ambiente segura (mínimo 32 caracteres) | `TokenService.cs` e Lambda |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint interno do OTel Collector (`http://otel-collector.observability:4317`) | OpenTelemetry SDK .NET 10 |
| `NEW_RELIC_LICENSE_KEY` / `DD_API_KEY` | Secret injetado no OTel Collector DaemonSet | Exportador OTLP do Collector |
| `ASPNETCORE_ENVIRONMENT` | `"Production"` ou `"Staging"` | ASP.NET Core Runtime |

### 4.2. Roteamento e Endpoints da Aplicação
- O backend .NET 10 expõe internamente na porta `8080` (contêiner) e o Ingress Nginx recebe o tráfego do NLB.
- Healthcheck endpoint obrigatório: `GET /health` deve responder HTTP 200 para os probes de Liveness/Readiness do EKS e health check do API Gateway.

---

## 5. Plano Passo a Passo de Execução (Track A)

### Passo 1: Finalização e Validação do Repositório `AutoReparos.Infra.Database`
**Diretório:** `submodules/AutoReparos.Infra.Database`  
**Branch de trabalho:** `feat/fase3-rds-terraform`  

1. **Revisar e Blindar o Código Terraform Existente:**
   - Conferir se [`terraform/main.tf`](file:///home/josemd12/Code/AutoReparos/submodules/AutoReparos.Infra.Database/terraform/main.tf) cobre:
     - `aws_db_subnet_group.rds` referenciando pelo menos 2 subnets privadas em AZs distintas.
     - `aws_security_group.rds` com regra de entrada TCP 5432 restrita estritamente ao `security_groups = [var.eks_nodes_security_group_id, var.lambda_security_group_id]` e blocos CIDR autorizados.
     - `aws_db_instance.postgres` configurado com `engine = "postgres"`, `engine_version = "16.3"`, `instance_class = "db.t4g.micro"`, `allocated_storage = 20`, `storage_encrypted = true`, `backup_retention_period = 7`, `skip_final_snapshot = false` (com snapshot para produção).
     - `aws_secretsmanager_secret` e `aws_secretsmanager_secret_version` persistindo a connection string completa em formato JSON e plain text para consumo da Lambda e do EKS.
2. **Definir Outputs Obrigatórios ([`terraform/outputs.tf`](file:///home/josemd12/Code/AutoReparos/submodules/AutoReparos.Infra.Database/terraform/outputs.tf)):**
   - `db_endpoint`: Host e porta do RDS (ex: `autoreparos-db.xxx.us-east-1.rds.amazonaws.com:5432`).
   - `db_address`: Endereço puro (sem porta).
   - `db_name`: `autoreparos_db`.
   - `db_security_group_id`: ID do SG do banco para amarrar nas regras do EKS.
   - `db_secret_arn`: ARN do segredo no AWS Secrets Manager.
3. **Pipeline de CI/CD GitHub Actions ([`.github/workflows/terraform-db.yml`](file:///home/josemd12/Code/AutoReparos/submodules/AutoReparos.Infra.Database/.github/workflows)):**
   - Step 1: `terraform fmt -check`
   - Step 2: `terraform init -backend=false`
   - Step 3: `terraform validate`
   - Step 4 (em PR): `terraform plan` com comentário do plan no Pull Request.
   - Step 5 (em merge na `main`): `terraform apply -auto-approve`.
4. **Documentação do Repositório ([`README.md`](file:///home/josemd12/Code/AutoReparos/submodules/AutoReparos.Infra.Database/README.md)):**
   - Propósito do repositório, diagrama de arquitetura do RDS, tabela de variáveis (`variables.tf`), instruções passo a passo para `terraform init`, `plan`, `apply` e `destroy`.

---

### Passo 2: Estruturação do Repositório `AutoReparos.Infra.K8s` (EKS, VPC, Ingress)
**Diretório:** Criar/clonar `AutoReparos.Infra.K8s` (utilizar como base o diretório `infra/` existente no repo pai)  
**Branch de trabalho:** `feat/fase3-eks-terraform`  

1. **Módulos Terraform no Repositório:**
   - `modules/vpc`: VPC 10.0.0.0/16, 2 Subnets Públicas, 2 Subnets Privadas para EKS, 2 Subnets Privadas para RDS, Internet Gateway, NAT Gateway. Tags obrigatórias:
     - Subnets Públicas: `kubernetes.io/role/elb = 1`
     - Subnets Privadas: `kubernetes.io/role/internal-elb = 1`
   - `modules/eks`:
     - Cluster AWS EKS (versão 1.30+).
     - Managed Node Group com instâncias `t3.medium` ou `t4g.medium`, autoscaling (min: 2, desired: 2, max: 5).
     - IAM OIDC Provider para IRSA (IAM Roles for Service Accounts).
   - `modules/ingress`: Deploy do Ingress Nginx Controller ou AWS Load Balancer Controller via Helm provider, expondo um Network Load Balancer (NLB) interno/externo.
   - `modules/hpa`: Horizontal Pod Autoscaler (HPA) baseado em CPU (target 70%) e Memória (target 80%), minReplicas: 2, maxReplicas: 10.
2. **Pipeline CI/CD GitHub Actions ([`.github/workflows/terraform-k8s.yml`]):**
   - Executa `terraform fmt`, `terraform validate`, `terraform plan` no PR e `terraform apply` em push na `main`.
3. **Documentação do Repositório ([`README.md`]):**
   - Propósito, diagrama de pods e escalabilidade, comandos `kubectl`, manifesto de secrets e outputs.

---

### Passo 3: Implementação do AWS API Gateway (HTTP API v2) no `AutoReparos.Infra.K8s`
**Módulo Terraform:** `modules/api_gateway` em `AutoReparos.Infra.K8s`

1. **Recurso `aws_apigatewayv2_api`:**
   ```hcl
   resource "aws_apigatewayv2_api" "http_api" {
     name          = "autoreparos-api-gateway-${var.environment}"
     protocol_type = "HTTP"
     cors_configuration {
       allow_origins = ["*"]
       allow_methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"]
       allow_headers = ["Content-Type", "Authorization", "X-Amz-Date", "X-Api-Key"]
       max_age       = 300
     }
   }
   ```
2. **Integração 1: Rota Serverless da Lambda (`/auth/cliente`):**
   ```hcl
   resource "aws_apigatewayv2_integration" "lambda_auth" {
     api_id                 = aws_apigatewayv2_api.http_api.id
     integration_type       = "AWS_PROXY"
     integration_uri        = var.auth_lambda_arn
     payload_format_version = "2.0"
   }

   resource "aws_apigatewayv2_route" "auth_cliente" {
     api_id    = aws_apigatewayv2_api.http_api.id
     route_key = "POST /auth/cliente"
     target    = "integrations/${aws_apigatewayv2_integration.lambda_auth.id}"
   }

   resource "aws_lambda_permission" "apigw_lambda" {
     statement_id  = "AllowAPIGatewayInvoke"
     action        = "lambda:InvokeFunction"
     function_name = var.auth_lambda_name
     principal     = "apigateway.amazonaws.com"
     source_arn    = "${aws_apigatewayv2_api.http_api.execution_arn}/*/*"
   }
   ```
3. **Integração 2: Rota da Aplicação EKS (`/api/{proxy+}`):**
   - Utilizar `aws_apigatewayv2_vpc_link` conectado às subnets privadas do EKS ou apontar diretamente para o DNS do NLB provido pelo Ingress:
   ```hcl
   resource "aws_apigatewayv2_integration" "k8s_backend" {
     api_id           = aws_apigatewayv2_api.http_api.id
     integration_type = "HTTP_PROXY"
     integration_uri  = "http://${var.nlb_dns_name}/{proxy}"
     integration_method = "ANY"
     connection_type  = "INTERNET" # ou VPC_LINK se privado
   }

   resource "aws_apigatewayv2_route" "api_proxy" {
     api_id    = aws_apigatewayv2_api.http_api.id
     route_key = "ANY /api/{proxy+}"
     target    = "integrations/${aws_apigatewayv2_integration.k8s_backend.id}"
   }
   ```
4. **Integração 3: Rota de Healthcheck (`GET /health`):**
   - Rota `GET /health` roteada para o backend K8s para verificação de uptime externo.
5. **Stage com Auto-Deploy e Access Log em JSON:**
   ```hcl
   resource "aws_apigatewayv2_stage" "default" {
     api_id      = aws_apigatewayv2_api.http_api.id
     name        = "$default"
     auto_deploy = true
     access_log_settings {
       destination_arn = aws_cloudwatch_log_group.apigw_logs.arn
       format = jsonencode({
         requestId      = "$context.requestId"
         ip             = "$context.identity.sourceIp"
         requestTime    = "$context.requestTime"
         httpMethod     = "$context.httpMethod"
         routeKey       = "$context.routeKey"
         status         = "$context.status"
         protocol       = "$context.protocol"
         responseLength = "$context.responseLength"
       })
     }
   }
   ```

---

### Passo 4: Governança dos 4 Repositórios Git & Convite à Banca
**Ação Administrativa no GitHub:**

1. **Configurar Regras de Proteção nas Branches `main` dos 4 Repositórios:**
   - `AutoReparos.App`
   - `AutoReparos.AuthLambda`
   - `AutoReparos.Infra.Database`
   - `AutoReparos.Infra.K8s`
   - *Regras ativadas:*
     - [x] Require a pull request before merging (mínimo 1 aprovação).
     - [x] Require status checks to pass before merging (CI pipeline verde).
     - [x] Do not allow bypass the above settings.
     - [x] Restrict direct commits/pushes to `main`.
2. **Convidar o Colaborador da Banca:**
   - Adicionar o usuário **`soat-architecture`** com permissão de colaborador (`Write` ou `Admin`) nos 4 repositórios.
   - Capturar prints ou registrar os links de confirmação de convite para inclusão no PDF de entrega final.

---

### Passo 5: Documentação de Arquitetura de Infraestrutura (Track A)
**Diretório de Entrega:** `docs/architecture/` (no repositório principal / submódulo de documentação)

Elaborar os documentos formais atribuídos à infraestrutura:
1. **`RFC-001-cloud-architecture-and-repo-segregation.md`**:
   - Motivação da segregação em 4 repositórios (ciclos de vida independentes, blast radius isolado, pipelines desacopladas).
   - Topologia de nuvem AWS (VPC, Subnets públicas/privadas, Segurança, NAT, EKS, RDS).
   - Matriz de responsabilidade dos repositórios.
2. **`RFC-002-managed-database-strategy-rds.md`**:
   - Justificativa formal da substituição do StatefulSet in-cluster pelo AWS RDS PostgreSQL 16.
   - Comparativo de disponibilidade (SLA 99.95%), backups automatizados (point-in-time recovery), criptografia KMS e manutenção de patches gerenciados.
3. **`ADR-001-adoption-aws-api-gateway.md`**:
   - Decisão de adotar AWS API Gateway HTTP API v2 na borda.
   - Racional: custo 70% menor que REST API, suporte nativo a HTTP_PROXY e Lambda AWS_PROXY, latência ultra-baixa, integração unificada de borda.
4. **Diagrama de Componentes de Nuvem:**
   - Diagrama Mermaid detalhado apresentando a visão completa de nuvem, VPCs, Security Groups e roteamento de tráfego.

---

## 6. Critérios de Aceite & Validação Empírica (Definition of Done - DoD)

Antes de considerar o Track A concluído, execute e valide os seguintes comandos:

```bash
# 1. Validação do Repositório AutoReparos.Infra.Database
cd submodules/AutoReparos.Infra.Database/terraform
terraform fmt -check
terraform init -backend=false
terraform validate
# Deve retornar: Success! The configuration is valid.

# 2. Validação do Repositório AutoReparos.Infra.K8s
cd submodules/AutoReparos.Infra.K8s/terraform
terraform fmt -check
terraform init -backend=false
terraform validate
# Deve retornar: Success! The configuration is valid.

# 3. Teste de Conformidade de Lint (tflint se disponível)
tflint --recursive

# 4. Verificação de Governança Git
# Conferir se os 4 repositórios possuem branch main protegida
# Conferir se soat-architecture foi convidado nos 4 repositórios
```

---

## 7. Checklist de Entregáveis Finais do Track A

- [ ] Repositório `AutoReparos.Infra.Database` com código Terraform RDS funcional e README completo.
- [ ] Pipeline CI/CD em `.github/workflows/` no repositório do banco validando `plan` e `apply`.
- [ ] Repositório `AutoReparos.Infra.K8s` com código Terraform EKS, Ingress e API Gateway funcional.
- [ ] Pipeline CI/CD em `.github/workflows/` no repositório de K8s validando `plan` e `apply`.
- [ ] Roteamento `/auth/cliente` e `/api/*` configurado no AWS API Gateway.
- [ ] Branch protection ativada na `main` dos 4 repositórios.
- [ ] Usuário `soat-architecture` adicionado como colaborador nos 4 repositórios.
- [ ] Arquivos `RFC-001`, `RFC-002`, `ADR-001` e Diagrama de Componentes finalizados em `docs/architecture/`.
