# Especificação Fase 4 — Testes e Qualidade

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Requisitos obrigatórios → *Testes e Qualidade*  
> **Requisitos cobertos (índice):** R18, R19, R20, R21, R34  
> **Repositórios afetados:** os 4 serviços, `AutoReparos.AuthLambda`, `AutoReparos.Web`, `AutoReparos` (pai)  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Rascunho inicial — ferramenta de BDD e estratégia do Sonar a validar (D-14, D-15)

---

## 1. Critério

> - Testes unitários em todos os microsserviços.
> - Pelo menos **um fluxo completo testado com BDD**.
> - Cobertura mínima de **80% por serviço**.
> - Validação de qualidade do código via **SonarQube** ou similar no CI.

---

## 2. Estado atual

Medição local em 04/11/2026 sobre a `origin/develop` (detalhes no [diagnóstico § 3](../diagnostico/fase4_diagnostico_estado_atual.md#3-testes-e-cobertura-medição-local-em-04112026)):

| Item | Situação |
|---|---|
| Stack de testes | xUnit, FluentAssertions, NSubstitute, Bogus, Respawn, Testcontainers (PostgreSQL), `Microsoft.AspNetCore.Mvc.Testing` |
| App — testes | 113 Domain + 151 Application + 78 integração; o teste instável de `NotificacaoServiceTests` foi corrigido em 06/11 |
| App — cobertura | 93,0% linhas / 73,6% branches (unit + integração, sem migrations); só unitários: 65,8% |
| AuthLambda | 40 testes; **74,8%** linhas (abaixo da meta) |
| Web (Angular 19 + Vitest) | 1 arquivo `.spec.ts` para 71 arquivos `.ts` |
| BDD | ❌ inexistente |
| Coleta de cobertura | `coverlet.runsettings` (OpenCover) no App e no pai; o CI do App **não coleta**; o CI da Lambda coleta e descarta |
| Sonar | `sonar-project.properties` no pai cobrindo todos os submódulos; SonarCloud roda por **análise automática** (GitHub App) só nos PRs do pai, sem importar cobertura; `docker-compose-sonar.yml` e `SONAR_LOCAL.md` para uso local |
| Gate no CI | Nenhum: não há quality gate nem limite de cobertura que bloqueie o merge |

---

## 3. Lacunas

1. OS, Billing e Execution (branch do Enrico) têm testes com PostgreSQL real via Testcontainers e provas verificadas por mutação, mas a cobertura não é medida nem publicada; Cadastros (App) herda os testes atuais.
2. Não há BDD.
3. A cobertura não é medida por serviço nem verificada no CI; a Lambda está abaixo de 80%.
4. O Sonar não roda no CI dos repositórios de serviço e não recebe cobertura.
5. O frontend, se ficar no mesmo projeto Sonar do OS Service, derruba a cobertura desse serviço.

---

## 4. Proposta (a validar)

### 4.1 Pirâmide por serviço

| Camada | Ferramentas | O que cobre |
|---|---|---|
| Unitários | xUnit, FluentAssertions, NSubstitute (manter a stack atual) | Entidades, value objects, use cases, máquina de estados da Saga, handlers de mensagens |
| Integração | Testcontainers (PostgreSQL, LocalStack com SQS, SNS e DynamoDB), WireMock.Net (Mercado Pago, SendGrid) | Repositórios, Outbox/Inbox, consumidores reais, webhook, endpoints |
| BDD | **Reqnroll** (sucessor do SpecFlow, descontinuado no fim de 2024) com xUnit, cenários em português (`# language: pt`) | Fluxo completo da OS e compensações |
| Ponta a ponta (opcional) | BDD no repositório pai contra o `docker compose` com os 4 serviços | Validação integrada antes do vídeo |

### 4.2 D-14 — Onde fica o BDD

**Recomendado:** no OS Service, que hospeda o orquestrador. Os cenários sobem o serviço com PostgreSQL e LocalStack em container e simulam Cobrança e Execução por consumidores de teste que respondem aos comandos. Isso roda no CI do próprio serviço e conta para a cobertura dele.

Complemento opcional: os mesmos cenários executados no pai contra os 4 serviços reais (`docker compose`), de forma manual ou agendada.

Esboço do fluxo exigido:

```gherkin
# language: pt
Funcionalidade: Ciclo de vida da ordem de serviço
  Como oficina
  Quero que a OS percorra orçamento, reserva, execução e pagamento
  Para que o cliente receba o veículo reparado com consistência entre os serviços

  Contexto:
    Dado um cliente "Leandro" com o veículo de placa "ABC1D23"
    E o insumo "Óleo 5W30" com 10 unidades disponíveis

  Cenário: Fluxo completo com pagamento aprovado
    Quando o atendente abre uma ordem de serviço para o veículo "ABC1D23"
    E o atendente gera o orçamento com "Troca de óleo" e 4 unidades de "Óleo 5W30"
    E o cliente aprova o orçamento
    Então 4 unidades de "Óleo 5W30" devem estar reservadas
    Quando o mecânico conclui a execução
    E o pagamento é aprovado pelo Mercado Pago
    Então a ordem de serviço deve estar "ProntaParaRetirada"
    E o insumo "Óleo 5W30" deve ter 6 unidades disponíveis e 0 reservadas

  Cenário: Cancelamento depois da reserva aciona as compensações
    Dado uma ordem de serviço com orçamento aprovado e 4 unidades de "Óleo 5W30" reservadas
    Quando o atendente cancela a ordem de serviço
    Então a ordem de serviço deve estar "Cancelada"
    E o orçamento deve estar "Cancelado"
    E o insumo "Óleo 5W30" deve ter 10 unidades disponíveis
    E o histórico deve registrar as compensações "execucao.cancelar-reserva" e "cobranca.cancelar-orcamento"

  Cenário: Execução fora do ar na liberação compensa a reserva
    Dado uma ordem de serviço com orçamento aprovado
    E o serviço de Execução reservou os insumos, mas não responde à liberação
    Quando o prazo técnico da liberação se esgota
    Então a reserva de "Óleo 5W30" deve estar "Cancelada"
    E a Saga deve estar em "FalhaLiberacao" no console de administração
```

### 4.3 Cobertura de 80% por serviço

- Coletar com `coverlet.collector` em formato OpenCover (o `coverlet.runsettings` já existe; copiar para cada repositório).
- O gate de 80% fica no **quality gate do Sonar** (condição de cobertura no código geral e no código novo). Opcionalmente, um passo com ReportGenerator publica o resumo como artefato e falha abaixo do limite.
- Exclusões iguais no coverlet e no Sonar (migrations, `Program.cs`, registro de DI, Swagger) e **justificadas no README**, para não parecerem manipulação.
- AuthLambda: escrever testes para `ClienteAuthDatabaseService` (ou para o novo cliente HTTP de D-11) e para os caminhos de erro do handler até passar de 80%.
- Frontend: se ficar fora do Sonar dos serviços (D-03), manter projeto próprio; se ficar junto, configurar cobertura do Vitest (`lcov`) e escrever testes dos serviços e componentes principais.

### 4.4 D-15 — Sonar no CI

**Recomendado:** SonarCloud com **análise via CI** (gratuita para repositórios públicos), um projeto por repositório.

1. Criar a organização e um projeto por repositório no SonarCloud; **desligar a análise automática** (ela não importa cobertura de C# e conflita com a análise por CI).
2. No workflow: instalar Java 17 e `dotnet-sonarscanner`; `begin` com `sonar.cs.opencover.reportsPaths` e `sonar.qualitygate.wait=true`; build; testes com cobertura; `end`.
3. Segredo `SONAR_TOKEN` em cada repositório.
4. O job do Sonar entra como status check obrigatório na proteção da `main` ([spec de CI/CD](../cicd/fase4_spec_cicd_e_protecao_de_branches.md)).
5. Retirar do `sonar-project.properties` do pai os caminhos dos submódulos (cada serviço passa a ter o seu).

Alternativa: SonarQube self-hosted (já existe `docker-compose-sonar.yml`), que precisaria ficar acessível pelo runner.

---

## 5. Passos para a mudança

1. ~~Corrigir o teste instável de `NotificacaoServiceTests`~~ — **feito** em 06/11 na `develop` do App (`aca8a6b`, testes com `MeterListener` isolados na collection `BusinessMetrics`). Falta só atualizar o ponteiro do submódulo no pai.
2. Medir a cobertura dos serviços do Enrico com Coverlet (OpenCover) e completar até 80% onde faltar; publicar no Sonar.
3. Escrever os testes unitários dos novos agregados, use cases, handlers e da máquina de estados da Saga.
4. Escrever os testes de integração com broker e banco em container, e WireMock.Net para o Mercado Pago.
5. Adicionar o projeto `AutoReparos.OS.BddTests` com Reqnroll e os cenários do § 4.2.
6. Configurar SonarCloud por repositório (D-15) e o quality gate com cobertura ≥ 80%.
7. Elevar a cobertura da AuthLambda acima de 80%.
8. Tratar o frontend conforme D-03.
9. Publicar evidências: badge do SonarCloud (quality gate e cobertura), print do painel e link no README de cada serviço (requisito R34).

---

## 6. Critérios de aceite

- [ ] Cada serviço tem testes unitários rodando no CI.
- [ ] Pelo menos um fluxo completo da OS e duas compensações testados com BDD no CI.
- [ ] Cada serviço com cobertura ≥ 80% no SonarCloud, com exclusões documentadas.
- [ ] Quality gate do Sonar bloqueia o merge quando falha.
- [ ] README de cada serviço mostra badge e print de cobertura.
- [ ] Nenhum teste instável na suíte (o caso conhecido já foi corrigido em 06/11).
