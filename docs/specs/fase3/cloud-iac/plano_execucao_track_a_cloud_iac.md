# Plano de Execução Técnica - Track A: Infraestrutura Cloud, IaC Terraform, API Gateway & Governança CI/CD

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3  
> **Documento de Origem / Especificação Base:** [`fase3_spec_track_a_cloud_iac.md`](./fase3_spec_track_a_cloud_iac.md)  
> **Sessão de Alinhamento:** Validação via `/grill-me` (14/09/2026)  
> **Papel Responsável:** Engenheiro de Infraestrutura Cloud & DevOps (Track A)  
> **Repositórios de Atuação Principal:**
> 1. [`submodules/AutoReparos.Infra.Database`](../../../../submodules/AutoReparos.Infra.Database)
> 2. [`submodules/AutoReparos.Infra.K8s`](../../../../submodules/AutoReparos.Infra.K8s)
> 3. Repositório Pai / Orquestrador (`AutoReparos`)  
> **Status:** Aprovado para Execução  

---

## 1. Visão Geral & Objetivos do Plano

Este documento formaliza o **Plano de Execução Detalhado** para o **Track A** da Fase 3 do Tech Challenge, incorporando integralmente as decisões arquiteturais e operacionais pactuadas durante o alinhamento interativo.

O objetivo central é garantir a entrega de uma infraestrutura em nuvem AWS robusta, desacoplada em múltiplos repositórios autônomos, segura por design (*Zero-Trust* com VPC Link Privado) e alinhada ao orçamento de custo acadêmico (padrão Free Tier / Custo Zero), acompanhada pela documentação formal de engenharia (RFCs e ADR).

---

## 2. Matriz de Decisões Técnicas Alinhadas (Grill-Me)

A tabela a seguir sintetiza as decisões consolidadas na sessão de refinamento:

| # | Dimensão Arquitetural | Decisão Alinhada | Racional & Impacto Técnico |
|---|:---|:---|:---|
| **1** | **Abrangência da Execução** | **End-to-End Completo** | Execução de todas as frentes do Track A: IaC nos 2 submódulos, roteamento do API Gateway, CI/CD, governança e redação de RFC-001, RFC-002, ADR-001 em `docs/architecture/`. |
| **2** | **Conectividade API Gateway ➔ EKS** | **VPC Link Privado** (`aws_apigatewayv2_vpc_link`) | Roteamento *zero-trust* entre o API Gateway e o Ingress Controller (NLB interno) nas subnets privadas, sem expor o NLB na internet pública. |
| **3** | **Desacoplamento entre Repositórios** | **AWS SSM Parameter Store / Tags + Fallback de Variáveis** | `AutoReparos.Infra.K8s` publica IDs de VPC e Subnets no SSM; `AutoReparos.Infra.Database` consome via Data Sources com fallback limpo em `terraform.tfvars`. |
| **4** | **Divisão de Responsabilidade IaC vs Helm** | **Infraestrutura no Terraform & Aplicação no Helm** | Terraform provisiona a base (VPC, EKS, API Gateway, VPC Link e Ingress Nginx com NLB interno). As cargas de trabalho da API (.NET 10) e HPA (2 a 10 réplicas) são gerenciadas pelo Helm em `k8s/charts/api`. |
| **5** | **Esteira CI/CD (GitHub Actions)** | **Pipeline Completa com Execução Condicional** | `fmt -check`, `validate` obrigatórios em qualquer PR; `plan` (com saída no PR) e `apply` (na `main`) ativados condicionalmente se as credenciais AWS estiverem presentes. |
| **6** | **Governança Git** | **Proteção de Branch na `main` (Sem Convite)** | Automação via script (`scripts/infra/setup-github-governance.sh`) para ativar PR obrigatório, bloqueio de force push e status check verde na `main` dos 4 repositórios. Convite a `soat-architecture` dispensado. |
| **7** | **Dimensionamento & Custos (RDS)** | **Padrão Free Tier / Custo Zero** | PostgreSQL 16 em `db.t4g.micro`, 20 GiB gp3, Single-AZ por padrão, retenção de backup de 7 dias e criptografia KMS ativa. |

---

## 3. Fases Detalhadas de Implementação

### Fase 1: Refinamento e Validação de `AutoReparos.Infra.Database`
**Diretório Alvo:** `submodules/AutoReparos.Infra.Database`

1. **Parâmetros no AWS SSM Parameter Store:**
   - Adicionar recursos `aws_ssm_parameter` no Terraform para expor de forma desacoplada:
     - `/autoreparos/${var.environment}/database/endpoint`
     - `/autoreparos/${var.environment}/database/address`
     - `/autoreparos/${var.environment}/database/name`
     - `/autoreparos/${var.environment}/database/secret_arn`
2. **Data Sources e Desacoplamento de Rede:**
   - Configurar leitura opcional de VPC e subnets via SSM (`/autoreparos/${var.environment}/vpc_id` e `/autoreparos/${var.environment}/subnets/private`) quando as variáveis diretas não forem fornecidas.
3. **Pipeline de CI/CD GitHub Actions ([`.github/workflows/ci.yml`]):**
   - Atualizar a pipeline para contemplar:
     - Step 1: `terraform fmt -check -diff`
     - Step 2: `terraform init -backend=false`
     - Step 3: `terraform validate`
     - Step 4 (Condicional à presença de credenciais AWS em PR): `terraform plan`
     - Step 5 (Condicional à presença de credenciais AWS em push na `main`): `terraform apply -auto-approve`
4. **Validação Local:**
   - Executar validações sintáticas com `terraform fmt -check`, `terraform init -backend=false` e `terraform validate`.

---

### Fase 2: Refinamento de `AutoReparos.Infra.K8s` e AWS API Gateway
**Diretório Alvo:** `submodules/AutoReparos.Infra.K8s`

1. **VPC Link Privado no Módulo API Gateway (`modules/apigateway`):**
   - Adicionar recurso `aws_apigatewayv2_vpc_link` referenciando `private_subnet_ids` e o security group dedicado.
   - Atualizar o recurso `aws_apigatewayv2_integration.eks_proxy` para:
     ```hcl
     connection_type = "VPC_LINK"
     connection_id   = aws_apigatewayv2_vpc_link.this.id
     ```
2. **Rotas Obrigatórias do API Gateway:**
   - Rota 1: `POST /auth/cliente` -> Integração AWS_PROXY com Lambda de autenticação.
   - Rota 2: `ANY /api/{proxy+}` -> Integração HTTP_PROXY via VPC Link para o Ingress NLB do EKS.
   - Rota 3: `GET /health` -> Rota dedicada de verificação de disponibilidade direcionada ao backend K8s.
3. **Publicação de Metadados de Rede no SSM:**
   - Adicionar recursos `aws_ssm_parameter` em `terraform/main.tf` para gravar:
     - `/autoreparos/${var.environment}/vpc_id`
     - `/autoreparos/${var.environment}/subnets/private`
     - `/autoreparos/${var.environment}/security-groups/eks-nodes`
4. **Pipeline de CI/CD GitHub Actions ([`.github/workflows/ci.yml`]):**
   - Atualizar workflow para validações completas (Terraform fmt/validate + Helm lint) e steps condicionais de plan/apply com OIDC/AWS Secrets.
5. **Validação Local:**
   - Executar `terraform fmt -check -recursive`, `terraform init -backend=false`, `terraform validate` e `helm lint`.

---

### Fase 3: Script de Governança das Branches `main`
**Diretório Alvo:** `scripts/infra/`

1. **Criação do Script `setup-github-governance.sh`:**
   - Script bash idempotente utilizando GitHub CLI (`gh api`).
   - Itera sobre os 4 repositórios da organização:
     - `Grupo78-PosTech-15SOAT/AutoReparos.App`
     - `Grupo78-PosTech-15SOAT/AutoReparos.AuthLambda`
     - `Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database`
     - `Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s`
   - Configura regras de branch protection para `main`:
     - `required_status_checks`: obrigatoriedade de status checks passarem antes do merge.
     - `enforce_admins`: proteção válida inclusive para administradores.
     - `required_pull_request_reviews`: exigência mínima de 1 aprovação via PR.
     - `allow_force_pushes`: `false` (bloqueio rigoroso de push com `--force`).
     - `allow_deletions`: `false` (impedir remoção acidental da branch).
   - Gera relatório em tela e log com as respostas da API do GitHub.

---

### Fase 4: Documentação Formal de Arquitetura (`docs/architecture/`)
**Diretório Alvo:** `docs/architecture/`

Elaborar a suíte completa de documentação formal de engenharia da infraestrutura com caminhos estritamente relativos:

1. **`RFC-001-cloud-architecture-and-repo-segregation.md`**:
   - Racional da segregação em 4 repositórios independentes (redução do *blast radius*, desacoplamento do ciclo de vida, esteiras de CI/CD autônomas).
   - Topologia de rede AWS VPC Multi-AZ, subnets públicas/privadas, tabelas de rotas e isolamento por Security Groups.
   - Matriz de responsabilidade dos componentes e repositórios.
2. **`RFC-002-managed-database-strategy-rds.md`**:
   - Análise comparativa aprofundada: AWS RDS PostgreSQL 16 vs StatefulSet in-cluster no EKS.
   - Avaliação de confiabilidade (SLA 99.95%), recuperação contra desastres (Point-in-Time Recovery), isolamento de storage, criptografia KMS e redução de sobrecarga operacional (*toil*).
3. **`ADR-001-adoption-aws-api-gateway.md`**:
   - Registro de decisão arquitetural da adoção do AWS API Gateway HTTP API v2 com VPC Link.
   - Comparativo com REST API e Ingress exposto diretamente na internet pública.
   - Análise de latência, simplificação de certificados TLS e benefícios de segurança *zero-trust*.
4. **`README.md` (Índice de Arquitetura & Diagrama Geral)**:
   - Sumário estruturado de navegação da arquitetura.
   - Diagrama Mermaid de Componentes de Nuvem atualizado, refletindo o fluxo Internet ➔ API Gateway ➔ VPC Link ➔ NLB Interno ➔ Ingress ➔ Pods .NET ➔ RDS PostgreSQL.

---

## 4. Critérios de Aceite & Validação Empírica (DoD)

Para considerar o Track A concluído com sucesso, todas as verificações abaixo devem ser satisfeitas:

```bash
# 1. Validação Sintática e Estrutural do RDS
cd submodules/AutoReparos.Infra.Database/terraform
terraform fmt -check -diff
terraform init -backend=false
terraform validate

# 2. Validação Sintática e Estrutural do EKS & API Gateway
cd ../../AutoReparos.Infra.K8s/terraform
terraform fmt -check -diff -recursive
terraform init -backend=false
terraform validate

# 3. Validação dos Helm Charts
cd ../k8s
helm lint .

# 4. Verificação de Governança
bash scripts/infra/setup-github-governance.sh --dry-run (ou modo verificação)

# 5. Validação de Links Relativos na Documentação
# Nenhum arquivo em docs/architecture/ ou docs/specs/ pode conter caminhos absolutos (/home, C:\, file://)
```

---

## 5. Checklist Consolidado de Entregáveis

- [x] **IaC Database:** Repositório `AutoReparos.Infra.Database` atualizado com SSM, SG restrito e outputs completos (Commit `1dede09`).
- [x] **CI/CD Database:** Pipeline `.github/workflows/ci.yml` configurada com validação e steps de plan/apply condicionais (push na `main`).
- [x] **IaC K8s & API Gateway:** Repositório `AutoReparos.Infra.K8s` atualizado com VPC Link privado, rotas `/auth/cliente`, `/api/*`, `/health` e publicação SSM (Commit `8fc46e9`).
- [x] **CI/CD K8s:** Pipeline `.github/workflows/ci.yml` configurada com validação Terraform, Helm lint e deploy condicional em push na `main`.
- [x] **Script de Governança:** Script `scripts/infra/setup-github-governance.sh` funcional para proteção da branch `main` dos 4 repositórios (executado e validado 4/4) (Commit `980065c`).
- [ ] **RFC-001:** `docs/architecture/RFC-001-cloud-architecture-and-repo-segregation.md` redigida e revisada.
- [ ] **RFC-002:** `docs/architecture/RFC-002-managed-database-strategy-rds.md` redigida e revisada.
- [ ] **ADR-001:** `docs/architecture/ADR-001-adoption-aws-api-gateway.md` redigida e revisada.
- [ ] **Índice & Diagrama:** `docs/architecture/README.md` com diagrama Mermaid e navegação completa.
- [ ] **Conformidade de Diretrizes:** Nenhuma violação de caminhos absolutos em documentações.
