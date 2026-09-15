# Plano Detalhado de Implementação - Subfase 2: Cluster EKS, VPC Link & AWS API Gateway

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3 (Track A - Infraestrutura Cloud)  
> **Repositório Alvo:** [`submodules/AutoReparos.Infra.K8s`](../../../../../submodules/AutoReparos.Infra.K8s)  
> **Documento Mestre:** [`../plano_execucao_track_a_cloud_iac.md`](../plano_execucao_track_a_cloud_iac.md)  
> **Responsável:** Engenheiro de Infraestrutura Cloud & DevOps  
> **Status:** Concluído e Validado (Commit `8fc46e9` no submódulo)  

---

## 1. Contexto de Negócio & Justificativa

### 1.1. O Desafio de Negócio: Borda Segura e Arquitetura Híbrida
O **AutoReparos** combina dois tipos distintos de tráfego e perfis de carga de trabalho:
- **Tráfego Serverless Esporádico (Portal do Cliente):** Clientes acessam o portal para consultar o status de seus veículos e aprovar orçamentos. Esse fluxo é intermitente e orientado a eventos, beneficiando-se imensamente de uma função **AWS Lambda** (.NET 10) sem custos fixos de ociosidade (*idle cost zero*).
- **Tráfego Operacional Contínuo (Oficina & Gestão):** Mecânicos, recepcionistas e gerentes executam operações ininterruptas na API central (gestão de peças, movimentação de ordens, diagnósticos), justificando um cluster **AWS EKS** com autoscaling elástico (HPA) baseado em demanda de CPU e memória.
- **Ponto Único de Entrada (API Gateway HTTP v2):** Unifica o roteamento da aplicação sob um único domínio com suporte nativo a CORS, autenticação e auditoria em JSON, sem a complexidade ou custo elevado de um Application Load Balancer tradicional na borda.

### 1.2. Blindagem de Rede Zero-Trust via VPC Link
Conforme acordado no alinhamento de arquitetura, o Network Load Balancer (NLB) provisionado pelo Ingress Nginx Controller dentro do EKS opera em **subnets privadas**. O API Gateway conecta-se a esse NLB exclusivamente através de um **VPC Link Privado** (`aws_apigatewayv2_vpc_link`), garantindo:
- Zero exposição direta do cluster Kubernetes na internet pública.
- Redução drástica da superfície de ataque contra os nós do EKS.
- Criptografia e tráfego estritamente confinados à infraestrutura privada da AWS.

---

## 2. Especificações Técnicas & Arquitetura Alvo

### 2.1. Topologia de Roteamento e Conectividade

```mermaid
flowchart TD
    subgraph Internet ["Internet Pública"]
        User["Clientes & Operadores"]
    end

    subgraph AWS_Cloud ["AWS Cloud (us-east-1)"]
        APIGW["AWS API Gateway (HTTP API v2)"]
        
        subgraph Serverless ["Camada Serverless"]
            Lambda["AWS Lambda (.NET 10)<br/>AutoReparos.AuthLambda"]
        end

        subgraph VPC ["VPC: 10.0.0.0/16"]
            VPCLink["aws_apigatewayv2_vpc_link<br/>(Subnets Privadas K8s)"]
            
            subgraph Private_Subnets ["Subnets Privadas EKS (AZ 1a e 1b)"]
                NLB["Internal Network Load Balancer (NLB)"]
                Ingress["Ingress Nginx Controller"]
                AppPods["AutoReparos.App Pods (.NET 10)<br/>HPA: Min 2 / Max 10"]
            end

            subgraph SSM_Layer ["Desacoplamento"]
                SSM_VPC["SSM: /autoreparos/production/vpc_id"]
                SSM_Subnets["SSM: /autoreparos/production/subnets/private"]
            end
        end
    end

    User -->|HTTPS| APIGW
    APIGW -->|POST /auth/cliente| Lambda
    APIGW -->|ANY /api/{proxy+}| VPCLink
    APIGW -->|GET /health| VPCLink
    VPCLink --> NLB
    NLB --> Ingress
    Ingress --> AppPods
```

### 2.2. Matriz de Rotas do AWS API Gateway HTTP v2

| Método & Caminho | Tipo de Integração | Destino Técnico | Finalidade de Negócio |
|:---|:---|:---|:---|
| `POST /auth/cliente` | `AWS_PROXY` (Payload v2.0) | AWS Lambda `AutoReparos.AuthLambda` | Autenticação efêmera e segura de clientes via CPF e E-mail. |
| `ANY /api/{proxy+}` | `HTTP_PROXY` (VPC_LINK) | `http://${nlb_private_dns}/api/{proxy}` | Roteamento de todas as chamadas operacionais para o backend .NET no EKS. |
| `GET /health` | `HTTP_PROXY` (VPC_LINK) | `http://${nlb_private_dns}/health` | Sondagem de integridade e liveness/readiness de ponta a ponta. |

---

## 3. Mapeamento de Arquivos e Alterações

```
submodules/AutoReparos.Infra.K8s/
├── .github/
│   └── workflows/
│       └── ci.yml                             # [ATUALIZAR] Validações completas e steps condicionais
├── terraform/
│   ├── main.tf                                # [ATUALIZAR] Parâmetros SSM e wiring de VPC Link
│   ├── variables.tf                           # [ATUALIZAR] Defaults e descrições de VPC Link
│   ├── outputs.tf                             # [VERIFICAR] Exportar endpoint do API Gateway e VPC ID
│   └── modules/
│       ├── apigateway/
│       │   ├── main.tf                        # [ATUALIZAR] Inclusão de VPC Link e rota /health
│       │   ├── variables.tf                   # [ATUALIZAR] Variáveis de subnets e SG para VPC Link
│       │   └── outputs.tf                     # [OK] Exportação de api_endpoint e api_id
│       ├── eks/                               # [OK] EKS Cluster e Managed Node Group
│       ├── vpc/                               # [OK] VPC, Subnets Públicas/Privadas, IGW e NAT GW
│       ├── ecr/                               # [OK] ECR autoreparos-api
│       └── addons/                            # [OK] CoreDNS, VPC CNI, Kube-Proxy e EBS CSI
└── k8s/
    └── charts/
        └── api/                               # [OK] Helm Charts com Ingress e HPA (2 a 10 réplicas)
```

---

## 4. Passo a Passo Detalhado de Implementação

### Passo 2.1: Implementar o VPC Link e Rotas em `modules/apigateway/main.tf`

Atualizar o módulo de API Gateway para criar o recurso de link privado e amarrar as integrações:

```hcl
# Security Group dedicado para a interface de rede do VPC Link
resource "aws_security_group" "vpc_link" {
  name        = "autoreparos-apigw-vpc-link-sg-${var.environment}"
  description = "Security Group para a interface do AWS API Gateway VPC Link"
  vpc_id      = var.vpc_id

  egress {
    description = "Permitir saida para as portas da aplicacao e NLB"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "autoreparos-apigw-vpc-link-sg-${var.environment}"
    Environment = var.environment
  }
}

# 1. AWS API Gateway v2 VPC Link conectando as subnets privadas
resource "aws_apigatewayv2_vpc_link" "eks_link" {
  name               = "autoreparos-vpc-link-${var.environment}"
  security_group_ids = [aws_security_group.vpc_link.id]
  subnet_ids         = var.private_subnet_ids

  tags = {
    Name        = "autoreparos-vpc-link-${var.environment}"
    Environment = var.environment
  }
}

# 2. Integração com a API Backend no cluster EKS (/api/{proxy+}) via VPC Link
resource "aws_apigatewayv2_integration" "eks_proxy" {
  api_id                 = aws_apigatewayv2_api.http_api.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.eks_link.id
  integration_uri        = "${var.eks_ingress_url}/api/{proxy}"
  payload_format_version = "1.0"
  description            = "Proxy HTTP privado via VPC Link para o Ingress NLB do EKS"
}

resource "aws_apigatewayv2_route" "eks_proxy_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "ANY /api/{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.eks_proxy.id}"
}

# 3. Rota Dedicada de Healthcheck (/health)
resource "aws_apigatewayv2_integration" "health_check" {
  api_id                 = aws_apigatewayv2_api.http_api.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "GET"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.eks_link.id
  integration_uri        = "${var.eks_ingress_url}/health"
  payload_format_version = "1.0"
  description            = "Sondagem de integridade de ponta a ponta para o backend EKS"
}

resource "aws_apigatewayv2_route" "health_check_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "GET /health"
  target    = "integrations/${aws_apigatewayv2_integration.health_check.id}"
}
```

### Passo 2.2: Exportar Metadados de Rede no AWS SSM Parameter Store em `terraform/main.tf`
Permitir que o repositório `AutoReparos.Infra.Database` e scripts externos consumam a VPC e subnets sem acoplamento direto de código:

```hcl
# 6. Publicação de Parâmetros de Rede no SSM Parameter Store
resource "aws_ssm_parameter" "vpc_id" {
  name        = "/autoreparos/${var.environment}/vpc_id"
  description = "ID da VPC provisionada para o AutoReparos"
  type        = "String"
  value       = module.vpc.vpc_id
  overwrite   = true

  tags = {
    Environment = var.environment
  }
}

resource "aws_ssm_parameter" "private_subnets" {
  name        = "/autoreparos/${var.environment}/subnets/private"
  description = "Lista separada por virgula dos IDs de subnets privadas"
  type        = "StringList"
  value       = join(",", module.vpc.private_subnet_ids)
  overwrite   = true

  tags = {
    Environment = var.environment
  }
}
```

### Passo 2.3: Atualizar Esteira de CI/CD em `.github/workflows/ci.yml`
Garantir validações estáticas completas de Terraform e Helm, com `plan` e `apply` condicionais:

```yaml
name: CI/CD - AutoReparos.Infra.K8s

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'terraform/**'
      - 'k8s/**'
      - '.github/workflows/ci.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'terraform/**'
      - 'k8s/**'
      - '.github/workflows/ci.yml'

jobs:
  validate-terraform:
    name: Validação Sintática IaC (Terraform)
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./terraform

    steps:
      - name: Checkout Code
        uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2

      - name: Setup Terraform
        uses: hashicorp/setup-terraform@b9cd54a3c349d3f38e8881555d616ced269862dd # v3.1.2
        with:
          terraform_version: 1.8.0

      - name: Terraform Format Check
        run: terraform fmt -check -diff -recursive

      - name: Terraform Init (no backend)
        run: terraform init -backend=false

      - name: Terraform Validate
        run: terraform validate

  lint-helm:
    name: Validação dos Helm Charts
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Code
        uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2

      - name: Setup Helm
        uses: azure/setup-helm@1a275c3b69536ee54be43f2070a358922e12c8d4 # v4.3.1
        with:
          version: v3.14.0

      - name: Lint Helm Charts
        run: helm lint k8s/

  plan-and-apply:
    name: Terraform Plan & Apply
    needs: [validate-terraform, lint-helm]
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./terraform
    if: ${{ secrets.AWS_ACCESS_KEY_ID != '' && secrets.AWS_SECRET_ACCESS_KEY != '' }}

    steps:
      - name: Checkout Code
        uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2

      - name: Setup Terraform
        uses: hashicorp/setup-terraform@b9cd54a3c349d3f38e8881555d616ced269862dd # v3.1.2
        with:
          terraform_version: 1.8.0

      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@e3ddf14f329da44ef5b750c8fb86e6b66ad4ce60 # v4.0.2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Terraform Init
        run: terraform init

      - name: Terraform Plan
        run: terraform plan -no-color

      - name: Terraform Apply (somente na main)
        if: github.ref == 'refs/heads/main' && github.event_name == 'push'
        run: terraform apply -auto-approve
```

---

## 5. Estratégia de Testes & Validação Empírica

Comandos locais sem custo de nuvem:

```bash
cd submodules/AutoReparos.Infra.K8s/terraform

# 1. Checagem recursiva de formatação HCL
terraform fmt -check -diff -recursive

# 2. Inicialização isolada
terraform init -backend=false

# 3. Validação dos módulos (VPC, EKS, API Gateway, ECR, Addons)
terraform validate
# Saída esperada: "Success! The configuration is valid."

# 4. Validação dos manifests e charts Helm
cd ../k8s
helm lint .
```

---

## 6. Gestão de Riscos, Mitigações e Rollback

| Risco Identificado | Severidade | Probabilidade | Mitigação Arquitetural | Procedimento de Rollback |
|:---|:---:|:---:|:---|:---|
| Falha de resolução DNS entre VPC Link e NLB interno | Alta | Média | Garantir que o Ingress crie um NLB privado nas subnets corretas com o atributo `service.beta.kubernetes.io/aws-load-balancer-internal: "true"`. | Reverter temporariamente para IP fixo ou target group manual via console. |
| Ingress não disponível no deploy inicial | Média | Baixa | Utilizar flag condicional `count = var.eks_ingress_url != "" ? 1 : 0` para permitir bootstrap desacoplado. | Ajustar variável `eks_ingress_url` para o DNS final do NLB gerado. |
| Timeout na criação do EKS Managed Node Group | Média | Baixa | Utilizar instâncias econômicas de ampla disponibilidade (`t3.small` / `t3.medium`). | Re-executar terraform apply ou ajustar zona de disponibilidade em `availability_zones`. |

---

## 7. Critérios de Aceite & Definition of Done (DoD)

- [x] Recurso `aws_apigatewayv2_vpc_link` configurado conectando subnets privadas.
- [x] Rotas `POST /auth/cliente`, `ANY /api/{proxy+}` e `GET /health` operacionais no HCL do API Gateway.
- [x] Exportação de `vpc_id`, `private_subnets`, `eks-nodes` e `apigateway/endpoint` no AWS SSM Parameter Store.
- [x] Pipeline CI/CD `.github/workflows/ci.yml` cobrindo Terraform e Helm lint com deploy na branch `main` em push.
- [x] Execução com sucesso de `terraform fmt -check`, `terraform validate` e `helm lint`.
