# Especificação Fase 4 — Entregáveis dos Repositórios

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Entregáveis da Fase 4 → *Repositórios Git (um para cada microsserviço)* e Saga → *Documentar a escolha e justificar no README*  
> **Requisitos cobertos (índice):** R17, R31, R32, R33, R34, R35, R36  
> **Repositórios afetados:** todos  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Rascunho inicial

---

## 1. Critério

> **Repositórios Git (um para cada microsserviço):**
> - Código-fonte;
> - Dockerfile e manifestos Kubernetes;
> - Pipelines de CI/CD;
> - Evidências de cobertura de testes (prints ou links no README);
> - Documentação da arquitetura do serviço;
> - Swagger ou collection Postman atualizada.

---

## 2. Estado atual (tomando o App como referência)

| Entregável | AutoReparos.App hoje | Observação |
|---|---|---|
| Código-fonte | ✅ | Monólito; vira o OS Service |
| Dockerfile | ✅ `AutoReparos.API/Dockerfile` e `AutoReparos.Web/Dockerfile` | — |
| Manifestos Kubernetes | ❌ no repositório do serviço | Estão em `AutoReparos.Infra.K8s/k8s/charts/api` |
| Pipeline CI/CD | 🟡 `ci.yml` com build e testes | Sem qualidade nem deploy |
| Evidência de cobertura no README | ❌ | Nenhum badge ou print |
| Documentação da arquitetura do serviço | 🟡 | Concentrada no README do pai e em `docs/architecture/` |
| Swagger | ✅ Swashbuckle em todos os ambientes | — |
| Postman | 🟡 [`Postman/AutoReparos.postman_collection.json`](../../../../../Postman/AutoReparos.postman_collection.json) no pai, de 22/07/2026 | Faltam `/auth/cliente`, `minhas-os`, `meus-veiculos`, `dashboard/*`, `kanban` |
| README com escolha da Saga | ❌ | Não há Saga |

---

## 3. Lacunas

1. OS, Billing e Execution existem na branch do Enrico, com Dockerfile e READMEs em inglês (Billing) ou parciais; nenhum tem chart, Postman ou evidência de cobertura.
2. Manifestos fora do repositório do serviço.
3. Nenhuma evidência de cobertura nos READMEs.
4. Documentação de arquitetura não está por serviço.
5. Postman desatualizada e no repositório errado (pai).

---

## 4. Proposta

### 4.1 Matriz de entregáveis por repositório

| Entregável | OS | Cobrança | Execução | Cadastros | Web | AuthLambda |
|---|---|---|---|---|---|---|
| Código-fonte | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dockerfile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (já existe) |
| Chart Helm em `deploy/helm/` | ✅ | ✅ | ✅ | ✅ | ✅ | n/a (Terraform) |
| `ci-cd.yml` completo | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Badge SonarCloud + print de cobertura | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Seção de arquitetura do serviço | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Swagger | ✅ `/swagger` | ✅ `/swagger` | ✅ `/swagger` | ✅ `/swagger` | n/a | n/a |
| Collection Postman | ✅ | ✅ | ✅ | ✅ | n/a | ✅ (`/auth/cliente`) |
| Seção "Saga" | ✅ orquestrador (escolha e justificativa) | ✅ participante | ✅ participante | ✅ participante | — | — |

### 4.2 Modelo de README para cada serviço

1. **Visão geral:** responsabilidade do serviço e o que ele não faz.
2. **Arquitetura do serviço:** diagrama de componentes (camadas Clean Architecture, banco, broker, integrações externas).
3. **Modelo de dados:** ERD (SQL) ou padrões de acesso e chaves (NoSQL), com a justificativa do banco.
4. **Contratos:** endpoints REST (link do Swagger), mensagens publicadas e consumidas.
5. **Papel na Saga:** passos que executa e compensações que oferece.
6. **Como rodar localmente:** `docker compose up`, variáveis de ambiente, seed.
7. **Testes e cobertura:** como rodar; badge e print do SonarCloud; link do painel.
8. **CI/CD:** diagrama do pipeline, link do GitHub Actions, regras de proteção da `main`.
9. **Deploy:** chart, namespace, segredos necessários.
10. **Observabilidade:** `service.name`, métricas expostas, dashboards relacionados.
11. **Postman:** link da collection e do environment.

### 4.3 README do pai

Adicionar a seção da Fase 4 com: diagrama geral (microsserviços, bancos, mensageria), tabela de repositórios, **Saga: estratégia escolhida e justificativa** (texto da [spec da Saga](../saga/fase4_spec_saga_pattern.md#41-d-07--saga-orquestrada-com-o-orquestrador-no-os-service)), links de Swagger, Postman, SonarCloud e vídeo.

### 4.4 Postman

- Uma collection por serviço, no repositório do serviço, com environments `local` e `aws` (URL base do API Gateway).
- No pai, uma collection "Fluxo completo da OS" que encadeia as chamadas (login → abrir OS → diagnóstico → aprovação → status → histórico), com scripts que guardam os ids em variáveis. Serve para a banca e para o vídeo.
- Incluir as rotas que faltam hoje (`/auth/cliente`, `minhas-os`, `meus-veiculos`, `dashboard/*`, `kanban`).

---

## 5. Passos para a mudança

1. Criar o modelo de README (§ 4.2), em português, e aplicá-lo nos 4 serviços, no Web e na AuthLambda.
2. Mover o chart da API para o Cadastros e criar os charts de OS, Cobrança e Execução.
3. Configurar Swagger (Swashbuckle ou OpenAPI nativo) em OS, Cobrança e Execução, com autenticação JWT no Swagger UI.
4. Exportar e revisar as collections Postman por serviço; criar a collection do fluxo completo no pai.
5. Depois do SonarCloud configurado, adicionar badges e prints de cobertura.
6. Escrever a seção da Saga no README do pai e do OS Service.
7. Atualizar `docs/architecture/README.md` (tabela de repositórios e divergências apontadas no diagnóstico).
8. Conferir a lista do § 4.1 antes da entrega.

---

## 6. Critérios de aceite

- [ ] Todos os itens da matriz do § 4.1 presentes em cada repositório.
- [ ] Collections Postman executam o fluxo completo sem ajuste manual de ids.
- [ ] README do pai e do OS Service explicam a escolha da Saga.
- [ ] Prints e badges de cobertura atualizados na semana da entrega.
