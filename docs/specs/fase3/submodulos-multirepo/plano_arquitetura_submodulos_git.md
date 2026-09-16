# Plano Arquitetural e de Execução: Migração Multi-Repo com Submódulos Git

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Fase 3 Tech Challenge (13SOAT / 15SOAT FIAP)  
> **Data de Conclusão:** 14 de Setembro de 2026  
> **Branch de Trabalho:** `feat/fase3-submodulos`  
> **Branch Alvo:** `feat/fase3-backend-fixes` (PR #34 atualizado)  
> **Status:** **100% CONCLUÍDO COM SUCESSO**  
> **Diretrizes Críticas Respeitadas:**  
> 1. **Nenhum reset destrutivo (`git reset --hard`) executado.**  
> 2. **Isolamento Estrito no workspace raiz mantido** (apenas `AutoReparos` e `FinanceHub`). Todos os submódulos residem exclusivamente dentro de `AutoReparos/submodules/`.  
> 3. **Padrão de Commits:** Gitmoji + Português (`✨ <Gitmoji> <Descrição em português>`).  
> 4. **Planos não commitados** (mantidos localmente).

---

## 1. Visão Geral da Arquitetura Multi-Repo Entregue

A especificação da Fase 3 (*13SOAT - Fase 3 - Tech Challenge.pdf*, páginas 2 a 5) foi **100% atendida**, com 4 repositórios autônomos organizados no padrão **Umbrella Repository com Git Submodules**:

```
                                    ┌─────────────────────────────────────────────────────────┐
                                    │               REPOSITÓRIO PAI (UMBRELLA)                │
                                    │          Grupo78-PosTech-15SOAT/AutoReparos             │
                                    │  - docker-compose.yml global (orquestração unificada)   │
                                    │  - AutoReparos.slnx consolidada (.NET 10)               │
                                    │  - .agents/ (AI Harness: AGENTS.md, GEMINI.md, skills)  │
                                    │  - Documentação centralizada da Fase 3 (ADRs, RFCs, ER) │
                                    └───────────────────────────┬─────────────────────────────┘
                                                                │
                 ┌──────────────────────────────┬───────────────┴──────────────┬──────────────────────────────┐
                 │ git submodule                │ git submodule                │ git submodule                │ git submodule
                 ▼                              ▼                              ▼                              ▼
┌────────────────────────────────┐ ┌───────────────────────────┐ ┌───────────────────────────┐ ┌────────────────────────────────┐
│          SUBMÓDULO 1           │ │        SUBMÓDULO 2        │ │        SUBMÓDULO 3        │ │          SUBMÓDULO 4           │
│    AutoReparos.AuthLambda      │ │  AutoReparos.Infra.Database│ │   AutoReparos.Infra.K8s   │ │        AutoReparos.App         │
│ (Repositório Git Independente) │ │ (Repositório Independente)│ │ (Repositório Independente)│ │ (Repositório Git Independente) │
├────────────────────────────────┤ ├───────────────────────────┤ ├───────────────────────────┤ ├────────────────────────────────┤
│ • Serverless Function .NET 10  │ │ • Terraform AWS RDS Pg 16 │ │ • Terraform AWS EKS       │ │ • Backend ASP.NET Core (.NET10)│
│ • Validação de CPF + Npgsql    │ │ • Subnets Privadas & SGs  │ │ • AWS API Gateway v2 HTTP │ │   (API, Domain, App, Infra)    │
│ • Emissão JWT Efêmero (1h)     │ │ • Parameter Group PG 16   │ │ • Helm Charts (Ingress)   │ │ • Frontend Angular 19 (Web)    │
│ • AutoReparos.AuthLambda.Tests │ │ • Secrets Manager         │ │ • Ingress & HPA Config    │ │ • Suíte de Testes Integrada    │
│ • CI/CD: Testes + Zip Lambda   │ │ • CI/CD: Terraform Check  │ │ • CI/CD: Terraform + Helm │ │ • CI/CD: Test + Docker Build   │
└────────────────────────────────┘ └───────────────────────────┘ └───────────────────────────┘ └────────────────────────────────┘
```

---

## 2. Matriz de Entregas e Status Real (100% Concluído)

| Componente | Ação Realizada | Evidência / Status |
|---|---|---|
| **Submódulo 1 (`AutoReparos.AuthLambda`)** | Repositório autônomo no GitHub, Function Serverless .NET 10, validação CPF Módulo 11, 40 testes unitários passando, CI/CD e `README.md`. Submódulo registrado em `.gitmodules`. | Branch `main` em [`AutoReparos.AuthLambda`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.AuthLambda). Submódulo ativo. |
| **Submódulo 2 (`AutoReparos.Infra.Database`)** | Repositório autônomo no GitHub, Terraform para AWS RDS PostgreSQL 16.3, Subnet Group privado, Security Group restrito, Parameter Group, Secrets Manager, CI/CD (`terraform fmt/validate`) e `README.md`. Submódulo registrado em `.gitmodules`. | Branch `main` em [`AutoReparos.Infra.Database`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database). Submódulo ativo. |
| **Submódulo 3 (`AutoReparos.Infra.K8s`)** | Repositório autônomo no GitHub, Terraform para VPC Multi-AZ, EKS Cluster, Node Groups com Auto-scaling, ECR, Addons, AWS API Gateway v2 HTTP API (`/auth/cliente` e `/api/*`), Helm Charts completos (api, web, ingress, observability), CI/CD e `README.md`. Submódulo registrado em `.gitmodules`. | Branch `main` em [`AutoReparos.Infra.K8s`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s). Submódulo ativo. |
| **Submódulo 4 (`AutoReparos.App`)** | Repositório autônomo no GitHub, Backend ASP.NET Core e Frontend Angular 19, desacoplamento de testes da Lambda, 303 testes passando (113 Domain, 123 Application, 67 Integration), CI/CD e `README.md`. Submódulo registrado em `.gitmodules`. | Branch `main` em [`AutoReparos.App`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.App). Submódulo ativo. |
| **Repositório Pai (`AutoReparos`)** | Código duplicado de aplicação e Lambda removido da raiz. Manifestos duplicados de infraestrutura removidos. `AutoReparos.slnx` consolidada compilando com 0 erros (276 testes unitários passando). `docker-compose.yml` validado. `.gitmodules` com 4 submódulos oficiais. | Commits pushados para `origin feat/fase3-submodulos` (PR #34 atualizado). |
| **Isolamento de Workspace** | Diretório raiz do workspace mantido estritamente limpo com apenas `AutoReparos` e `FinanceHub`. | Confirmado via listagem do workspace. |

---

## 3. Fases de Execução Realizadas

### [x] Fase 1: Remoção de Código Duplicado da Aplicação e Lambda da Raiz
- `git rm -r AutoReparos.API AutoReparos.Application AutoReparos.Application.Tests AutoReparos.Domain AutoReparos.Domain.Tests AutoReparos.Infra AutoReparos.IntegrationTests AutoReparos.Web`
- `rm -rf AutoReparos.AuthLambda AutoReparos.Web` (resíduos untracked)
- Atualização dos caminhos em `scripts/dev/dev.sh`, `scripts/infra/build-push-images.sh` e `scripts/k8s/test-local-helm.sh` para apontarem para `submodules/AutoReparos.App/...`.
- `dotnet build AutoReparos.slnx` -> 0 erros de compilação.
- Commit: `🔥 Removendo código duplicado da aplicação principal e lambda da raiz do repositório pai`

### [x] Fase 2: Estruturação e Publicação do Submódulo 2 (`AutoReparos.Infra.Database`)
- Estruturação completa em `submodules/AutoReparos.Infra.Database/`:
  - `terraform/main.tf`: AWS RDS PostgreSQL 16.3, Subnet Group, Security Group restrito (porta 5432), Parameter Group, Secrets Manager.
  - `terraform/variables.tf`, `terraform/outputs.tf`, `terraform/providers.tf`, `terraform/terraform.tfvars.example`.
  - `.github/workflows/ci.yml`: Pipeline com `terraform fmt` e `terraform validate`.
  - `.gitignore` e `README.md` abrangente com diagrama de arquitetura e tabela de variáveis.
- `terraform validate` -> Sucesso.
- Git commit e push para `origin main` em `Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database`.
- Registro no repositório pai via `git submodule add`.
- Commit no pai: `✨ Adicionando submódulo oficial de banco de dados gerenciado (AutoReparos.Infra.Database)`

### [x] Fase 3: Estruturação e Publicação do Submódulo 3 (`AutoReparos.Infra.K8s`)
- Estruturação completa em `submodules/AutoReparos.Infra.K8s/`:
  - Módulos Terraform: `vpc`, `eks`, `ecr`, `addons`, `apigateway` (AWS API Gateway HTTP API v2 com rotas `/auth/cliente` -> Lambda e `/api/*` -> EKS Ingress).
  - Helm Charts: `charts/api`, `charts/web`, `charts/ingress`, `charts/observability`, `Chart.yaml`, `values.yaml`, `values-production.yaml`.
  - `.github/workflows/ci.yml`: Pipeline com validação Terraform e `helm lint`.
  - `.gitignore` e `README.md` abrangente com diagrama de roteamento e comandos de deploy.
- `terraform validate` -> Sucesso.
- `helm lint k8s/` -> 0 failed.
- Git commit e push para `origin main` em `Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s`.
- Registro no repositório pai via `git submodule add`.
- Commit no pai: `✨ Adicionando submódulo oficial de infraestrutura Kubernetes e Gateway (AutoReparos.Infra.K8s)`

### [x] Fase 4: Limpeza de Infraestrutura Legada na Raiz e Validação Global
- Remoção dos manifestos duplicados de Terraform e Helm da raiz: `git rm -r k8s infra/modules infra/*.tf infra/.terraform.lock.hcl infra/terraform.tfvars.example`.
- Preservação em `infra/` exclusivamente de `grafana` e `otel` para manter a orquestração do `docker-compose.yml` local.
- Atualização do path do Helm em `scripts/k8s/test-local-helm.sh` para `submodules/AutoReparos.Infra.K8s/k8s/`.
- Validação do `.gitmodules` com os 4 submódulos oficiais.
- Validação de `dotnet build AutoReparos.slnx` -> 0 erros.
- Validação de testes unitários: 276 testes aprovados (113 Domain, 40 Lambda, 123 Application).
- Validação de `docker compose config` -> 100% válido.
- Validação de isolamento do diretório raiz do workspace -> Apenas `AutoReparos` e `FinanceHub`.
- Commit no pai: `🔥 Removendo manifestos duplicados legados de Terraform e Helm da raiz do repositório pai`
- `git push origin feat/fase3-submodulos` (PR #34 atualizado no GitHub).

---

## 4. Estado Final do Repositório Pai (`AutoReparos`)

```
AutoReparos/
├── .agents/                                     # AI Harness centralizado (AGENTS.md, GEMINI.md, skills)
├── .github/workflows/                           # CI do repositório pai
├── .gitmodules                                  # Configuração oficial dos 4 submódulos
├── docker-compose.yml                           # Orquestração local unificada (Postgres, API, Web, Otel)
├── AutoReparos.slnx                             # Solution consolidada .NET 10 para desenvolvimento
├── README.md                                    # Documentação mestra Fases 1, 2 e 3
├── SONAR_LOCAL.md                               # Guia de qualidade SonarQube
├── docs/                                        # RFCs, ADRs e especificações da Fase 3
├── scripts/                                     # Scripts de desenvolvimento atualizados
├── infra/                                       # Configurações de observabilidade local (grafana, otel)
│
└── submodules/
    ├── AutoReparos.AuthLambda/                  # Submódulo 1: Function Serverless .NET 10
    ├── AutoReparos.Infra.Database/              # Submódulo 2: Terraform AWS RDS PostgreSQL 16
    ├── AutoReparos.Infra.K8s/                   # Submódulo 3: Terraform EKS & AWS API Gateway v2 + Helm
    └── AutoReparos.App/                         # Submódulo 4: Backend ASP.NET Core & Angular 19 Web
```

---

## 5. Conclusão

A arquitetura multi-repo da Fase 3 do Tech Challenge FIAP SOAT está **completamente implementada, limpa, testada e versionada**. Todos os quatro repositórios no GitHub possuem CI/CD e documentação técnica, e o repositório pai orquestra todos os submódulos de maneira transparente para o desenvolvedor local.
