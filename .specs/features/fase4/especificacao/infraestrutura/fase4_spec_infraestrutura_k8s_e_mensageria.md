# Especificação Fase 4 — Infraestrutura: Kubernetes por Serviço e Mensageria

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Requisitos obrigatórios → *Infraestrutura* (repositório próprio, deploy em Kubernetes, mensageria) e Separação em Microsserviços → *infraestrutura própria*  
> **Requisitos cobertos (índice):** R03, R27, R28, R29, R32  
> **Repositórios afetados:** `AutoReparos.Infra.K8s`, `AutoReparos.Infra.Database`, os 4 serviços, `AutoReparos` (pai — `docker-compose.yml`)  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Mensageria decidida (SQS + SNS, D-06 e D-19); deploy por repositório (D-13)

---

## 1. Critério

> - Cada microsserviço deve estar em seu próprio repositório;
> - Deploy automatizado para ambiente Kubernetes;
> - Mensageria para orquestração ou coreografia;
> - Uso de ferramentas de monitoramento e observabilidade já implementadas na Fase 3 *(detalhado em [`fase4_spec_observabilidade_distribuida.md`](./fase4_spec_observabilidade_distribuida.md))*.
>
> Separação em Microsserviços: cada um com seu próprio repositório, **infraestrutura** e banco de dados.

---

## 2. Estado atual

| Camada | O que existe | Repositório |
|---|---|---|
| Rede | VPC dedicada, subnets públicas e privadas em 2 AZs, NAT | Infra.K8s (`modules/vpc`) |
| Cluster | EKS com Managed Node Group em subnets privadas; OIDC provider (usado pelo EBS CSI) | Infra.K8s (`modules/eks`) |
| Add-ons | EBS CSI + StorageClass `gp3`, metrics-server, ingress-nginx (NLB), `kube-prometheus-stack` | Infra.K8s (`modules/addons`) |
| Registro de imagens | ECR `autoreparos-api` (o Web é publicado sem repositório criado pelo Terraform) | Infra.K8s (`modules/ecr`) |
| Borda | API Gateway HTTP v2 + VPC Link | Infra.K8s (`modules/apigateway`) |
| Compartilhamento | Parâmetros SSM (`/autoreparos/{env}/vpc_id`, subnets, SG dos nós, endpoint do API Gateway) | Infra.K8s e Infra.Database |
| Manifestos | Chart guarda-chuva `k8s/` com subcharts `api` (Deployment, Service, HPA, ConfigMap, Secret, Ingress), `web`, `database` e `observability` | Infra.K8s |
| Banco | RDS PostgreSQL 16 único | Infra.Database |
| Mensageria | ❌ | — |
| Local | `docker-compose.yml` do pai: Postgres, API, Web, pgAdmin, OTel Collector, Prometheus, Jaeger, Loki, Grafana | pai |

Na branch `feature/os-service-contracts`, o Infra.K8s ganhou configuração de telemetria OTLP no chart da API, mas a branch partiu de um commit anterior ao provisionamento centralizado e precisa ser rebaseada na `develop` antes do PR.

Divergências já conhecidas (apontadas no plano de provisionamento centralizado): `className: alb` nos values × ingress-nginx instalado; namespace `default` fixo; ECR do Web ausente; Grafana exposto por LoadBalancer com senha padrão.

---

## 3. Lacunas

1. Manifestos e ECR dos serviços ficam no Infra.K8s, não no repositório de cada serviço.
2. Não há mensageria.
3. Não há isolamento por serviço no cluster (namespace, ServiceAccount, IAM, NetworkPolicy).
4. A capacidade do node group foi pensada para um serviço só.

---

## 4. Proposta (a validar)

### 4.1 Divisão de responsabilidades

| Fica na plataforma (`Infra.K8s`) | Vai para cada serviço |
|---|---|
| VPC, EKS, node groups, add-ons, ingress controller | Chart Helm do serviço (`deploy/helm/<servico>`) |
| API Gateway, VPC Link e rotas públicas | ECR do serviço |
| Stack de observabilidade (chart `observability`) | Role IAM (IRSA) e ServiceAccount do serviço |
| VPC gateway endpoint do DynamoDB | Tópico SNS (se produtor), filas, assinaturas e DLQs do serviço; tabela DynamoDB da Saga (OS) |
| Parâmetros SSM compartilhados; instância RDS (`Infra.Database`) | Database e usuário do serviço ([spec de bancos](../microsservicos/fase4_spec_bancos_de_dados.md)) |

### 4.2 Estrutura de cada repositório de serviço

```text
AutoReparos.<Servico>/
├── src/                      # Domain, Application, Infra, API
├── tests/                    # unitários, integração, BDD
├── Dockerfile
├── docker-compose.yml        # serviço + dependências para desenvolvimento
├── deploy/helm/<servico>/    # Deployment, Service, Ingress (paths do serviço), HPA, PDB,
│                             # ConfigMap, ExternalSecret/Secret, ServiceAccount (IRSA),
│                             # Job de migration (hook pre-upgrade)
├── infra/terraform/          # ECR, IAM/IRSA, filas/tópicos, tabela NoSQL ou database
├── .github/workflows/ci-cd.yml
└── README.md
```

### 4.3 Kubernetes

| Tema | Proposta |
|---|---|
| Namespaces | Um por serviço (`autoreparos-os`, `autoreparos-cobranca`, `autoreparos-execucao`, `autoreparos-cadastros`) + `observability` |
| Ingress | ingress-nginx (corrigir `className`); cada serviço cria o próprio Ingress com seus paths no mesmo host; a rota `ANY /api/{proxy+}` do API Gateway continua única |
| Escala | HPA por serviço (CPU/memória); requests e limits por serviço; `PodDisruptionBudget` com `minAvailable: 1` |
| Saúde | Liveness em `/health/live`; readiness em `/health/ready` verificando banco e broker |
| Segurança | IRSA por serviço; NetworkPolicy permitindo só ingress controller → serviço e serviço → observabilidade/broker; endpoints `/internal/*` fora do Ingress público |
| Capacidade | Rever `instance_types` e `desired/max_size` do node group para 4 serviços + Web + observabilidade |

### 4.4 Mensageria e DynamoDB

| Tema | Como |
|---|---|
| Provisionamento | Cada serviço cria no próprio Terraform suas filas de entrada e DLQs; o produtor cria seu tópico SNS e cada consumidor a assinatura da própria fila |
| Acesso | IAM (IRSA): cada serviço só lê as próprias filas e só publica nas filas/tópicos permitidos; só o OS acessa a tabela e o stream da Saga |
| Local e testes | LocalStack (SQS, SNS, DynamoDB) |
| Métricas | CloudWatch (profundidade da fila e da DLQ), exportadas para o Prometheus (ex.: YACE) ou lidas pelo próprio serviço |

### 4.5 Ambiente local

`docker-compose.yml` do pai com: OS, Cobrança, Execução, Cadastros, Web, PostgreSQL (com `os_db`, `cobranca_db`, `execucao_db` e `cadastros_db`), LocalStack (SQS, SNS, DynamoDB) e a stack de observabilidade atual. Cada repositório de serviço mantém um compose mínimo próprio.

---

## 5. Passos para a mudança

1. Combinar com o Enrico a retirada do deploy dos serviços do pai (D-13).
2. **Infra.K8s:** corrigir as divergências conhecidas (ingress, namespace, ECR do Web, Grafana); retirar os subcharts `api` e `web` do chart guarda-chuva após os serviços terem seus charts; manter `observability`; publicar no SSM os valores que os serviços precisam (nome do cluster, ARN do OIDC provider, host do ingress).
3. **Mensageria e DynamoDB:** filas, DLQs e tópicos no Terraform de cada serviço; tabela da Saga no Terraform do OS; VPC gateway endpoint do DynamoDB no Infra.K8s.
4. **Repositórios de serviço:** criar `deploy/helm/<servico>` e `infra/terraform` seguindo a estrutura do § 4.2; o Cadastros parte do chart `api` atual; OS, Cobrança e Execução partem dos Dockerfiles que o Enrico já criou.
5. **API Gateway:** adicionar a rota pública do webhook do Mercado Pago; manter `/api/{proxy+}` e `/health`.
6. **Capacidade e custo:** ajustar o node group; manter o workflow de destruição do ambiente para controlar custos.
7. **Ambiente local:** atualizar o `docker-compose.yml` do pai e criar o de cada serviço.
8. **Documentação:** atualizar `docs/architecture/` (diagrama de componentes cloud) e o README do Infra.K8s.

---

## 6. Critérios de aceite

- [ ] Cada serviço tem no próprio repositório o Dockerfile, o chart Helm e o Terraform dos seus recursos.
- [ ] Os 4 serviços sobem no EKS, cada um em seu namespace, com HPA e health checks.
- [ ] A mensageria está provisionada por IaC e acessível só pelos serviços autorizados.
- [ ] O ambiente local completo sobe com um `docker compose up`.
- [ ] Nenhuma divergência conhecida (ingress, namespace, ECR) permanece.
