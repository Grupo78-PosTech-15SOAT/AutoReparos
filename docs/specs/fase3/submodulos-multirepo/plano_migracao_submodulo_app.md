# Plano de Execução: Migração da Aplicação Principal para o Submódulo `submodules/AutoReparos.App`

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Fase 3 Tech Challenge (13SOAT / 15SOAT FIAP)  
> **Branch de Trabalho no Repositório Pai:** `feat/fase3-submodulos`  
> **Branch Alvo para PR:** `feat/fase3-backend-fixes`  
> **Status:** **CONCLUÍDO COM SUCESSO (PR #34 ABERTO)**  
> **PR no GitHub:** [#34 - Submódulos AutoReparos.App e AutoReparos.AuthLambda](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos/pull/34)  
> **Data de Conclusão:** 14/09/2026

---

## 1. Diretriz Mandatória de Workspace

> [!IMPORTANT]
> **Isolamento Estrito no Workspace Raiz:**  
> O diretório raiz de projetos deve conter **exclusivamente**:
> 1. `FinanceHub` (outra aplicação independente)
> 2. `AutoReparos` (repositório pai / orquestrador)  
> **Nenhum submódulo ou pasta intermediária pode residir solta no workspace raiz.**  
> Todos os 4 repositórios da Fase 3 residem obrigatoriamente dentro de `AutoReparos/submodules/`:
> - `submodules/AutoReparos.AuthLambda`
> - `submodules/AutoReparos.App`
> - `submodules/AutoReparos.Infra.Database`
> - `submodules/AutoReparos.Infra.K8s`

---

## 2. Conteúdo e Responsabilidades dos Repositórios

```
workspace/
├── FinanceHub/                                  # Aplicação externa (intocada)
└── AutoReparos/                                 # REPOSITÓRIO PAI (Umbrella)
    ├── .agents/                                 # AI Harness centralizado
    ├── .gitmodules                              # Configuração oficial dos submódulos
    ├── docker-compose.yml                       # Orquestração local unificada
    ├── AutoReparos.slnx                         # Solution consolidada
    ├── docs/                                    # Documentação centralizada (ADRs, RFCs, C4)
    │
    └── submodules/
        ├── AutoReparos.AuthLambda/              # SUBMÓDULO 1: Function Serverless (C# .NET 10)
        │   ├── src/AutoReparos.AuthLambda/
        │   ├── tests/AutoReparos.AuthLambda.Tests/
        │   ├── .github/workflows/ci.yml
        │   └── README.md
        │
        ├── AutoReparos.App/                     # SUBMÓDULO 4: Aplicação Principal
        │   ├── AutoReparos.API/                 # ASP.NET Core Minimal APIs & Controllers
        │   ├── AutoReparos.Application/         # Casos de Uso, DTOs
        │   ├── AutoReparos.Domain/              # Entidades DDD, Value Objects
        │   ├── AutoReparos.Infra/               # EF Core, Repositories, Identity, Npgsql
        │   ├── AutoReparos.Web/                 # Frontend Angular 19 (Signals)
        │   ├── AutoReparos.Domain.Tests/        # 113 testes
        │   ├── AutoReparos.Application.Tests/   # 123 testes
        │   ├── AutoReparos.IntegrationTests/    # 67 testes com Testcontainers
        │   ├── AutoReparos.App.slnx             # Solution autônoma da aplicação
        │   ├── .github/workflows/ci.yml         # CI/CD (Testes + Docker Build)
        │   └── README.md                        # Documentação da aplicação
        │
        ├── AutoReparos.Infra.Database/          # SUBMÓDULO 2: Terraform AWS RDS PostgreSQL 16
        └── AutoReparos.Infra.K8s/               # SUBMÓDULO 3: Terraform AWS EKS + API Gateway v2
```

---

## 3. Desacoplamento da Suíte de Testes da Aplicação

Para que `submodules/AutoReparos.App` seja 100% autônomo (sem qualquer dependência de compilação da Lambda):
1. **Em `AutoReparos.IntegrationTests`:**
   - Removido `<ProjectReference>` para AuthLambda.
   - Em `PortalClienteIntegrationTests.cs`, gerado o token de teste via `JwtSecurityTokenHandler` usando a mesma chave simétrica de teste (`JwtSecret`), com consulta ao cliente real persistido pelo DbInitializer.
   - O teste do endpoint valida o comportamento da API diante de um JWT autêntico contendo as claims exigidas (`sub`, `cpf`, `email`, `role: "Cliente"`).
2. **Em `submodules/AutoReparos.AuthLambda`:**
   - O teste do handler da Lambda contra o banco real permanece autônomo no repositório da Lambda (40 testes).

---

## 4. Status de Execução do Roteiro

- [x] **Passo 1: Registrar submódulo `submodules/AutoReparos.App` no pai** — Submódulo configurado e registrado em `.gitmodules`.
- [x] **Passo 2: Migrar projetos da aplicação para `submodules/AutoReparos.App`** — Copiados todos os projetos com histórico e estrutura intactos.
- [x] **Passo 3: Desacoplar `AutoReparos.IntegrationTests` no submódulo** — Removida referência de compilação cruzada à Lambda e implementado gerador de token JWT autônomo.
- [x] **Passo 4: Configurar `AutoReparos.App.slnx`, CI/CD e Documentação** — Criados `.slnx`, `.gitignore`, `.github/workflows/ci.yml` e `README.md`.
- [x] **Passo 5: Validação Empírica no Submódulo** — 303 testes aprovados (Domain: 113, Application: 123, Integration: 67) e build do frontend Angular 19 concluído.
- [x] **Passo 6: Publicação no GitHub do Repositório `AutoReparos.App`** — Commit inicial e push para branch `main` em `origin`.
- [x] **Passo 7: Ajustes no Repositório Pai (`AutoReparos`)** — `AutoReparos.slnx` e `docker-compose.yml` apontados para caminhos dos submódulos.
- [x] **Passo 8: Validação Global** — Compilação da solution consolidada (0 erros) e validação sintática do Docker Compose (`docker compose config`).
- [x] **Passo 9: Commit no Repositório Pai e Pull Request** — Commit efetuado em `feat/fase3-submodulos` e **[PR #34](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos/pull/34)** criado com sucesso contra a branch `feat/fase3-backend-fixes`.

---

## 5. Garantia de Conformidade do Workspace Raiz
 
Ao final do processo, a estrutura física no workspace será:
```
workspace/
├── AutoReparos/
│   └── submodules/
│       ├── AutoReparos.AuthLambda/
│       └── AutoReparos.App/
└── FinanceHub/
```
Nenhuma pasta adicional será criada no diretório raiz do workspace.
