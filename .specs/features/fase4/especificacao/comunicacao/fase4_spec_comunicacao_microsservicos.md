# Especificação Fase 4 — Comunicação entre Microsserviços

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Requisitos obrigatórios → *Comunicação entre Microsserviços*  
> **Requisitos cobertos (índice):** R11, R12, R13  
> **Repositórios afetados:** os 4 serviços, `AutoReparos.AuthLambda`, `AutoReparos.Infra.K8s`  
> **Feature `.specs`:** `saga-mensageria`  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** SQS, SNS e biblioteca decididos (D-06, D-08, D-19); em aberto: contratos (D-12)

---

## 1. Critério

> - Definir estratégias de comunicação:
>   - APIs RESTful síncronas (quando necessário);
>   - Mensageria assíncrona (RabbitMQ, Kafka, SQS etc.) para eventos e integração desacoplada.
> - **Nenhum serviço pode acessar diretamente o banco de outro serviço.**

A parte de bancos está detalhada em [`fase4_spec_bancos_de_dados.md`](../microsservicos/fase4_spec_bancos_de_dados.md).

---

## 2. Estado atual

| Tipo de comunicação | `develop` | Branch do Enrico |
|---|---|---|
| Cliente → API | REST via API Gateway → VPC Link → NLB → ingress → API | Rotas `/api/v1/...` em cada serviço |
| Serviço → serviço (REST) | Não existe | Billing consulta o OS antes do checkout (`OsService__BaseUrl`, identidade `ServicoOS`) |
| Mensageria | Não existe | `AWSSDK.SQS` + Outbox no PostgreSQL de cada serviço; publicação com 5 tentativas, backoff exponencial com jitter, depois DLQ (AD-001) |
| Envelope | — | JSON em camelCase com `eventId` e `tipo` (ex.: `ordem-servico.criada.v1`) |
| Deduplicação | — | Consumidores deduplicam por `eventId` (Execution também por OS) |
| Fan-out | — | Execution grava **uma linha de Outbox por destino** (OS e Billing), com o mesmo `eventId`, uma fila por destino |
| Integrações externas | SendGrid; AuthLambda → RDS direto | Mercado Pago (Checkout Pro, webhook) |

Eventos existentes: `ordem-servico.criada.v1`, `ordem-servico.atualizada.v1` (OS), `ordem-servico.liberada.v1` (consumido pelo Execution, mas ninguém publica), `execucao.concluida.v1`, `execucao.falhou.v1` (publicados, mas ninguém consome) e o resultado de pagamento do Billing (sem `tipo` padronizado).

---

## 3. Lacunas

1. Contrato entre os serviços incompleto (ver acima) e sem comandos da Saga.
2. Sem Inbox persistida no OS; envelope sem `correlacaoId` nem `traceparent`.
3. Fan-out feito pelo produtor: cada novo consumidor exige mudar o produtor.
4. Filas não provisionadas por IaC.
5. AuthLambda lê o banco de clientes diretamente.

---

## 4. Desenho

### 4.1 Quando usar cada estilo

| Estilo | Uso | Exemplos |
|---|---|---|
| **REST síncrono** | Requisições de usuários e consultas que precisam de resposta imediata | Abrir OS, montar orçamento, aprovar, registrar etapas, checkout, webhook do Mercado Pago, console de administração |
| **REST interno** (só quando necessário) | Leitura pontual entre serviços, fora do API Gateway | OS → Cadastros para validar cliente e veículo na abertura; frontend/Cobrança → Execução para ler o catálogo e os preços; AuthLambda → Cadastros (D-11) |
| **Mensageria assíncrona** | Todos os passos da Saga (comandos e eventos), notificações | `execucao.reservar-insumos.v1`, `orcamento.aprovado.v1`, `execucao.concluida.v1` |

Regra geral: **os serviços não se chamam por HTTP para avançar o fluxo.** Os dados que um serviço precisa vão no próprio comando ou evento (ex.: os itens do orçamento seguem em `orcamento.aprovado.v1` até a reserva).

### 4.2 Roteamento REST (API Gateway → ingress por path)

A rota `ANY /api/{proxy+}` do API Gateway continua única; cada serviço declara no próprio chart o Ingress com seus paths.

| Prefixo | Serviço |
|---|---|
| `/api/v1/ordens-servico`, `/api/v1/admin/sagas`, `/api/v1/dashboard` | OS |
| `/api/v1/orcamentos`, `/api/v1/pagamentos`, `/api/v1/webhooks/mercado-pago` | Cobrança |
| `/api/v1/fila-execucao`, `/api/v1/execucoes`, `/api/v1/servicos`, `/api/v1/insumos` | Execução |
| `/api/v1/auth`, `/api/v1/usuarios`, `/api/v1/clientes`, `/api/v1/veiculos` | Cadastros |
| `/internal/*` | Fora do Ingress público (NetworkPolicy) |

O frontend troca os paths antigos (`/api/ordem-servico/...`) pelos novos.

### 4.3 D-06 — Mensageria

**Decidido:** AWS SQS com DLQ por fila, retry com backoff exponencial e jitter (AD-001), provisionada por Terraform no repositório de cada serviço; LocalStack no ambiente local e nos testes.

**Decidido em 10/11 — D-19, fan-out por SNS:**

| Opção | Como fica | Prós | Contras |
|---|---|---|---|
| Uma linha de Outbox por destino (atual do Enrico) | O produtor grava N linhas, uma por fila consumidora | Já implementado e testado; nenhuma infraestrutura nova | O produtor conhece os consumidores: Cadastros (notificações) exigiria mudar Cobrança e Execução |
| **SNS por produtor + SQS por consumidor** (escolhido) | Cada produtor publica eventos no seu tópico (`os-eventos`, `cobranca-eventos`, `execucao-eventos`); cada consumidor cria a própria fila assinando o tópico, com filtro por `tipo` | Produtor desacoplado; novo consumidor não muda ninguém; padrão AWS | Mais um recurso por serviço; a mudança no Execution é pequena (publicar no tópico em vez de N filas) |

Comandos continuam indo direto para a fila do participante (`cobranca-comandos`, `execucao-comandos`), porque têm um único destino.

### 4.4 D-08 — Biblioteca

**Decidido:** `AWSSDK.SQS` e `AWSSDK.SimpleNotificationService` com Outbox, Inbox e retry implementados pelo grupo, como o Enrico já fez nos 3 serviços. Sem MassTransit (licença da v9 e abstração que esconderia o padrão que a banca quer ver).

### 4.5 Catálogo de mensagens

Nomes em português (L-02), no formato `<contexto>.<fato-ou-ação>.v<versão>`. Detalhes do fluxo na [spec da Saga](../saga/fase4_spec_saga_pattern.md).

| Mensagem | Tipo | Produtor | Consumidor | Situação |
|---|---|---|---|---|
| `ordem-servico.criada.v1` | Evento | OS | OS (inicia a Saga) | ✅ publicado; consumo novo |
| `ordem-servico.atualizada.v1` | Evento | OS | — | ✅ existe |
| `saga.avancou.v1` | Evento interno | OS (Saga, DynamoDB) | OS (status e histórico, PostgreSQL) | Novo (L-01) |
| `orcamento.gerado.v1` / `orcamento.aprovado.v1` (com itens) / `orcamento.recusado.v1` | Evento | Cobrança | OS; Cadastros (notificação) | Novo |
| `execucao.reservar-insumos.v1` (corpo: `ordemServicoId` e itens `{insumoId, quantidade}`) | Comando | OS | Execução | Novo |
| `insumos.reservados.v1` / `insumos.reserva-falhou.v1` | Evento | Execução | OS | Novo |
| `ordem-servico.liberada.v1` | Comando | OS | Execução | ✅ consumidor existe; publicação nova |
| `execucao.enfileirada.v1` | Evento | Execução | OS | Novo (prazo técnico da liberação) |
| `execucao.concluida.v1` | Evento | Execução | OS, Cobrança, Cadastros | ✅ publicado; consumo novo |
| `execucao.falhou.v1` / `execucao.reexecutada.v1` | Evento | Execução | OS | `falhou` ✅ publicado; `reexecutada` novo |
| `pagamento.aprovado.v1` / `pagamento.recusado.v1` | Evento | Cobrança | OS, Cadastros | Padronizar o evento atual |
| `execucao.cancelar-reserva.v1` | Comando (compensação) | OS | Execução | Novo |
| `insumos.reserva-cancelada.v1` | Evento (compensação) | Execução | OS | Novo |
| `cobranca.cancelar-orcamento.v1` | Comando (compensação) | OS | Cobrança | ✅ publicado e consumido (entrada legada `billing.orcamento.cancelar.v1` aceita pela Cobrança) |
| `orcamento.cancelado.v1` | Evento (compensação) | Cobrança | OS | ✅ existe |
| `saga.lembrete.v1` | Evento | OS | Cadastros (notificação) | Novo (D-18d) |

**Envelope comum** (camelCase, mantendo o que já existe): `eventId`, `tipo`, `correlacaoId` (= id da OS), `causaId` (id da mensagem que originou), `ocorridoEm`, `traceparent`, `tracestate` e o `payload`.

O comando de compensação de orçamento é publicado como `cobranca.cancelar-orcamento.v1` (D-24, feature [`eventos-portugues`](../../eventos-portugues/plan.md)); etapas e comandos pendentes legados com `billing.orcamento.cancelar.v1` são normalizados pelo OS Service antes da publicação, e a Cobrança continua aceitando `billing.orcamento.cancelar.v1` apenas como entrada legada deduplicada pelo mesmo `eventId`. Os demais tipos implementados já estão em português; as mensagens marcadas como novas acima pertencem às respectivas features futuras.

### 4.6 Garantias

| Garantia | Como |
|---|---|
| Não perder mensagens | **Transactional Outbox** no mesmo banco da mudança de estado (PostgreSQL nos serviços; DynamoDB na Saga, publicada por Streams) |
| Não processar duas vezes | **Inbox** persistida na mesma transação do efeito (regra L-01); operações de negócio idempotentes (reserva única por OS e insumo) |
| Falhas transitórias | Retry com backoff exponencial e jitter; depois do limite, DLQ com alerta e reprocessamento pelo console (D-18d) |
| Ordem | Não presumida; a máquina de estados ignora eventos que não se aplicam ao estado atual e registra no histórico |
| Evolução de contratos | Versão no `tipo`; só mudanças aditivas; contratos duplicados em cada serviço com o catálogo acima como fonte única (D-12) |
| HTTP | Timeout, retry e circuit breaker com `Microsoft.Extensions.Http.Resilience` |

---

## 5. Passos para a mudança

1. Fechar D-12.
2. Publicar o catálogo do § 4.5 com exemplos de payload em `docs/architecture/contratos-mensagens.md` e referenciá-lo nos READMEs.
3. Padronizar o envelope nos 3 serviços existentes (acrescentar `correlacaoId`, `causaId`, `ocorridoEm` e `traceparent`).
4. Provisionar tópicos SNS, filas, assinaturas e DLQs no Terraform de cada serviço; LocalStack no `docker-compose.yml`.
5. Implementar os produtores e consumidores novos do catálogo (ver passos da [Saga](../saga/fase4_spec_saga_pattern.md#5-passos-para-a-mudança)).
6. Declarar no chart de cada serviço o Ingress com seus paths; adicionar no API Gateway a rota do webhook.
7. Ajustar a AuthLambda (D-11) e remover a liberação do SG da Lambda no RDS.
8. Ajustar o frontend para os novos paths.
9. Testes de integração com LocalStack via Testcontainers: mensagem duplicada, consumidor fora do ar, envio para DLQ e reprocessamento.

---

## 6. Critérios de aceite

- [ ] Os passos da Saga trafegam só por mensageria; as chamadas REST entre serviços são só leituras.
- [ ] Derrubar um participante não perde mensagens: ao voltar, ele processa a fila.
- [ ] Mensagem repetida não duplica efeito; mensagem com erro permanente vai para a DLQ, gera alerta e pode ser reprocessada.
- [ ] Nenhum acesso a banco de outro serviço (inclusive a Lambda).
- [ ] Catálogo de contratos documentado, com nomes em português e versão.
