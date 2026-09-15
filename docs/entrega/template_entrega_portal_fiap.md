# Tech Challenge — Fase 3 — Documento de Entrega

> **Minuta para compilação em PDF e submissão no Portal FIAP.**
> Os campos marcados com 🔲 dependem de informação que ainda não existe e precisam ser
> preenchidos antes de gerar o PDF. Todo o restante já está verificado.

---

## Capa

| | |
|---|---|
| **Curso** | Pós-Graduação em Software Architecture (SOAT) — FIAP |
| **Turma** | 15SOAT |
| **Grupo** | 78 |
| **Fase** | 3 — Operação Corporativa, Serverless e Segregação Multi-Repo |
| **Projeto** | AutoReparos — Sistema Integrado de Oficina Mecânica |
| **Data de entrega** | 🔲 *preencher* |

### Integrantes

| Nome | RM |
|---|---|
| Enrico Gollner | rm370737 |
| José Dotta | rm372959 |
| Júlia Santos | rm370364 |
| Lucas Bastos | rm370749 |
| Mateus Lecchi | rm371085 |

---

## 1. Resumo da entrega

O AutoReparos é uma plataforma de gestão de oficina mecânica que cobre o ciclo completo
da ordem de serviço: recepção do veículo, diagnóstico, orçamento, aprovação pelo cliente,
execução e entrega, com controle de estoque de insumos.

A Fase 3 entregou quatro evoluções sobre a base das fases anteriores:

1. **Desacoplamento em 4 repositórios Git independentes**, orquestrados por submódulos.
2. **Autenticação serverless do cliente final** por CPF e e-mail, em AWS Lambda, sem
   criação de contas no ASP.NET Core Identity.
3. **Banco de dados gerenciado** (AWS RDS PostgreSQL 16) e roteamento unificado de borda
   com API Gateway v2.
4. **Observabilidade de negócio** com OpenTelemetry, três dashboards mandatórios,
   políticas de alerta e exportação para ferramenta de APM de mercado.

---

## 2. Repositórios oficiais

| # | Repositório | Responsabilidade | URL |
|---|---|---|---|
| 1 | **AutoReparos.App** | Aplicação principal: API .NET 10, domínio, frontend Angular 19 | https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.App |
| 2 | **AutoReparos.AuthLambda** | Função serverless de autenticação do cliente | https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.AuthLambda |
| 3 | **AutoReparos.Infra.Database** | Terraform do AWS RDS PostgreSQL | https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database |
| 4 | **AutoReparos.Infra.K8s** | Terraform do EKS, API Gateway v2 e Helm Charts | https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s |
| — | **AutoReparos** (pai) | Orquestrador por submódulos, documentação e stack local | https://github.com/Grupo78-PosTech-15SOAT/AutoReparos |

---

## 3. Vídeo demonstrativo

| | |
|---|---|
| **Link** | 🔲 *URL do YouTube ou Vimeo (público ou não listado)* |
| **Duração** | 🔲 *preencher — máximo de 15 minutos* |

Roteiro utilizado: [`docs/entrega/roteiro_video_demonstracao_15min.md`](./roteiro_video_demonstracao_15min.md)

| Bloco | Janela | Conteúdo |
|---|---|---|
| 1 | 00:00–02:00 | Apresentação, contexto e topologia dos 4 repositórios |
| 2 | 02:00–05:00 | Pipelines de CI/CD no GitHub Actions |
| 3 | 05:00–07:30 | Autenticação serverless com CPF via API Gateway |
| 4 | 07:30–10:00 | Rotas protegidas e isolamento Zero-Trust |
| 5 | 10:00–13:00 | Os 3 dashboards mandatórios |
| 6 | 13:00–15:00 | Logs estruturados, traces distribuídos e APM |

---

## 4. Documentação de arquitetura

Todos os documentos estão em [`docs/architecture/`](../architecture/) no repositório pai.

| Documento | Conteúdo |
|---|---|
| [RFC-001](../architecture/RFC-001-cloud-architecture-and-repo-segregation.md) | Arquitetura Cloud AWS e segregação em 4 repositórios autônomos. Topologia detalhada de VPC, subnets privadas, diagramas de tráfego e matriz RACI |
| [RFC-002](../architecture/RFC-002-managed-database-strategy-rds.md) | Estratégia de banco de dados gerenciado: transição de StatefulSet in-cluster para AWS RDS PostgreSQL 16, SLA 99.95%, PITR, KMS e Free Tier |
| [RFC-003](../architecture/RFC-003-serverless-client-authentication.md) | Autenticação serverless por CPF e e-mail, sem contas no Identity. Alternativas avaliadas (Cognito, magic link, autorizador nativo) e riscos aceitos |
| [ADR-001](../architecture/ADR-001-adoption-aws-api-gateway.md) | Adoção do AWS API Gateway HTTP API v2 com VPC Link: comparativo com REST API v1, ALB e Ingress NGINX, latência, custos e Zero-Trust |
| [ADR-002](../architecture/ADR-002-data-isolation-and-zero-trust-claims.md) | Isolamento de dados do portal a partir das claims do JWT, com Zero Data Leakage no filtro por placa e proteção BOLA |
| [ADR-003](../architecture/ADR-003-end-to-end-observability-strategy.md) | Observabilidade ponta a ponta com OpenTelemetry vendor-agnostic, com as validações executadas |
| [Modelo de dados](../architecture/database-selection-and-data-model.md) | Justificativa do PostgreSQL 16, comparativo com MongoDB e MySQL, modelo ER e dicionário de dados |
| [Diagramas de sequência](../architecture/diagrams/sequence_portal_auth_and_query.md) | Fluxo end-to-end do portal: caminho feliz, filtro por placa e caminhos de erro |

---

## 5. Requisitos da Fase 3 e onde cada um é atendido

| Requisito | Atendimento | Evidência |
|---|---|---|
| Segregação em repositórios independentes | 4 repositórios + orquestrador por submódulos | Seção 2 e RFC-001 |
| Função serverless de autenticação | `AutoReparos.AuthLambda` com validação de CPF por módulo 11 e JWT de 1 hora | RFC-003 |
| Banco de dados gerenciado | AWS RDS PostgreSQL 16 via Terraform | `AutoReparos.Infra.Database` e RFC-002 |
| API Gateway na borda | API Gateway v2: `POST /auth/cliente` → Lambda, `ANY /api/{proxy+}` → EKS, `GET /health` | ADR-001 |
| Isolamento de dados do cliente | `ClienteId` extraído da claim; resposta vazia indistinguível no filtro por placa | ADR-002 |
| CI/CD automatizado | Esteira própria por repositório no GitHub Actions | Seção 6 |
| Dashboards de monitoramento | 3 dashboards mandatórios provisionados automaticamente | Seção 7 |
| Integração com APM de mercado | Exportação OTLP para New Relic, validada com license key real | ADR-003 |

---

## 6. Qualidade e testes

Suíte executada com `dotnet test AutoReparos.slnx`:

| Projeto | Testes |
|---|---|
| `AutoReparos.Domain.Tests` | 113 |
| `AutoReparos.AuthLambda.Tests` | 40 |
| `AutoReparos.Application.Tests` | 134 |
| `AutoReparos.IntegrationTests` (Testcontainers PostgreSQL) | 67 |
| **Total** | **354** |

Zero falhas e zero testes ignorados. Os testes de integração sobem um PostgreSQL real em
contêiner, sem mocks de banco.

🔲 *Atualizar os números caso a suíte cresça até a entrega. O total precisa ser
exatamente a soma das partes.*

---

## 7. Observabilidade — o que foi verificado

Registrado em detalhe no [ADR-003](../architecture/ADR-003-end-to-end-observability-strategy.md).

**Dashboards mandatórios**, provisionados automaticamente nos dois ambientes a partir de
uma fonte única em [`docs/observability/dashboards/`](../observability/dashboards/):

| Dashboard | Conteúdo |
|---|---|
| Volume Diário de Ordens de Serviço | Total em 30 dias, volume do dia e série por status |
| Tempo Médio por Status | Diagnóstico, execução e permanência total na oficina |
| Erros e Falhas nas Integrações | Falhas de notificação, respostas 5xx e latência de banco |

**Métricas de negócio** emitidas por um interceptor do EF Core no momento da
persistência, de modo que nenhum caminho de código altere o status de uma OS sem
contabilizar.

**Validações executadas:**

- Painéis de erro exercitados com falha real de banco: `500` em 18 de 18 requisições,
  com ambos os painéis de 5xx reagindo.
- Exportação para o New Relic confirmada pela autotelemetria do Collector: 480 logs,
  588 pontos de métrica e 541 spans entregues, **zero falhas** nos três sinais.
- Logs em JSON com `TraceId` e `SpanId` correlacionados aos traces do Jaeger.

---

## 8. Evidências complementares

| Item | Status |
|---|---|
| Usuário `soat-architecture` como colaborador nos 4 repositórios | 🔲 *anexar print de cada repositório em Settings → Collaborators* |
| Execuções verdes das esteiras de CI | 🔲 *anexar print da aba Actions* |
| Dashboards com dados reais | 🔲 *anexar print do Grafana* |
| New Relic recebendo telemetria | 🔲 *anexar print do APM & Services* |

---

## 9. Como executar o projeto

```bash
git clone --recurse-submodules https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.git
cd AutoReparos
cp .env.example .env     # preencher as senhas
docker compose up -d --build
```

| Serviço | URL |
|---|---|
| API (Swagger) | http://localhost:8080/swagger |
| Grafana | http://localhost:3000 (admin/admin) |
| Jaeger | http://localhost:16686 |
| Prometheus | http://localhost:9090 |

O banco é migrado e populado automaticamente na primeira execução, incluindo o usuário
administrador definido em `SEED_USER_EMAIL` e `SEED_USER_PASSWORD`.

---

## Antes de gerar o PDF

- [ ] Preencher todos os campos 🔲
- [ ] Confirmar que o link do vídeo está acessível em janela anônima
- [ ] Reexecutar `dotnet test` e conferir se o total da seção 6 ainda bate
- [ ] Confirmar que os 4 repositórios estão acessíveis ao avaliador
- [ ] Verificar que nenhum print contém credenciais, tokens ou license keys
