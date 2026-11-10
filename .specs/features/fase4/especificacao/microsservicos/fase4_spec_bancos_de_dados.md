# Especificação Fase 4 — Banco de Dados por Microsserviço (SQL + NoSQL)

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Separação em Microsserviços → *Observação: cada microsserviço deverá ter banco de dados próprio* e Comunicação → *Nenhum serviço pode acessar diretamente o banco de outro serviço*  
> **Requisitos cobertos (índice):** R04, R09, R10, R13  
> **Repositórios afetados:** `AutoReparos.Infra.Database`, `AutoReparos.AuthLambda`, os 4 serviços  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Decidido em 09/11/2026 — DynamoDB no estado da Saga (D-04) e RDS compartilhado com database e usuário por serviço (D-05); D-11 em aberto

---

## 1. Critério

> Cada microsserviço deverá ter **banco de dados próprio**, sendo obrigatório:
> - Uso de pelo menos **um banco relacional (SQL)**;
> - Uso de pelo menos **um banco não relacional (NoSQL)**.
>
> **Nenhum serviço pode acessar diretamente o banco de outro serviço.** *(destacado no PDF)*

---

## 2. Estado atual

| Item | `develop` | Branch do Enrico |
|---|---|---|
| Banco relacional | PostgreSQL 16 no RDS (`db.t4g.micro`, criptografado, subnets privadas), por `Infra.Database/terraform` | Cada serviço com connection string própria (`ConnectionStrings__Billing` etc.) e migrations EF Core próprias |
| Databases | Uma só: `autoreparos_db`, com todas as tabelas do App | OS, Billing e Execution com database própria; criação no RDS ainda manual |
| Credenciais | Usuário master; segredo no Secrets Manager; endpoint no SSM | Papéis por serviço previstos (script SQL de papéis no OS, sem teste — item aberto do Verifier) |
| Banco NoSQL | ❌ | ❌ (DynamoDB só no plano `saga-mensageria`) |
| Acesso cruzado | A **AuthLambda lê `Clientes` diretamente** (`ClienteAuthDatabaseService.cs`) | Nenhum entre os serviços novos |
| Migrations | `DbInitializer` aplica na subida da API | Aplicadas na subida (Billing e Execution) |

---

## 3. Lacunas

1. Não há banco NoSQL.
2. As databases e usuários dos serviços novos não são criados por automação (o RDS fica em subnet privada e o runner do GitHub não alcança a porta 5432).
3. A AuthLambda continua lendo a tabela de outro serviço, o que o PDF proíbe.
4. Migrations rodam na subida dos pods.

---

## 4. Desenho

| Serviço | Banco | Tipo | Justificativa |
|---|---|---|---|
| OS | PostgreSQL — `os_db` | SQL | OS, histórico e Outbox já implementados e verificados; consultas por filtro (Kanban, por cliente) |
| OS | **DynamoDB — `autoreparos-os-saga`** | **NoSQL** | Estado da Saga acessado sempre por chave (id da OS); transição com escrita condicional; Inbox + Saga + Outbox num `TransactWriteItems`; Streams para publicar a Outbox |
| Cobrança | PostgreSQL — `cobranca_db` | SQL | Valores monetários, tentativas e reembolsos pedem ACID; chave única para idempotência de webhooks |
| Execução | PostgreSQL — `execucao_db` | SQL | Fila, etapas, catálogo e insumos; a reserva em duas fases precisa da mesma transação da conclusão |
| Cadastros | PostgreSQL — `cadastros_db` | SQL | Código e migrations atuais do App (Identity, clientes, veículos) |

O OS tem dois bancos, mas cada handler grava em um só (regra L-01, [Saga § 4.8](../saga/fase4_spec_saga_pattern.md#48-regra-l-01-no-os-service--opção-c)).

### 4.1 D-04 — DynamoDB no estado da Saga

**Decidido em 09/11/2026**, alinhado ao plano aprovado do Enrico. O estado da Saga tem padrão de acesso simples (por id da OS, mais um índice por estado e prazo), muda com escrita condicional e precisa de transação com Inbox e Outbox no mesmo banco: o caso de uso típico do DynamoDB. O banco é gerenciado na mesma conta AWS, acessado de forma privada pela VPC (gateway endpoint), autenticado por IAM/IRSA sem senha e restrito à tabela do serviço, com custo quase zero parado. MongoDB (Atlas M0) foi descartado por exigir tráfego pela internet e um segundo provedor.

Modelo da tabela: ver [Saga § 4.8](../saga/fase4_spec_saga_pattern.md#48-regra-l-01-no-os-service--opção-c).

| Configuração | Valor |
|---|---|
| Cobrança | Sob demanda (`PAY_PER_REQUEST`) |
| Chaves | `PK`, `SK` (string); GSI1 `estado` + `prazoEm` |
| Streams | `NEW_IMAGE`, consumido pelo publicador da Outbox |
| TTL | Atributo `expiraEm` (Inbox e passos antigos) |
| Acesso | Role IRSA do OS com permissão só nesta tabela e no stream |
| Local e testes | LocalStack (ou DynamoDB Local) via Testcontainers |

### 4.2 D-05 — Instância RDS compartilhada

**Decidido em 09/11/2026:** uma instância RDS PostgreSQL com database e usuário próprios por serviço; cada repositório mantém a IaC dos seus recursos exclusivos.

| Opção | Prós | Contras |
|---|---|---|
| **Database e usuário por serviço na mesma instância** (escolhida) | Cabe no free tier; isolamento por `GRANT`/`REVOKE` e segredo próprio | Isolamento lógico, não físico; precisa ser explicado no README |
| Uma instância RDS por serviço | Isolamento total | Quadruplica o custo e o tempo de provisionamento |

### 4.3 Como garantir "nenhum acesso ao banco de outro serviço"

1. **Credenciais:** um segredo por serviço (`autoreparos/os/db`, `autoreparos/cobranca/db`, `autoreparos/execucao/db`, `autoreparos/cadastros/db`); `REVOKE CONNECT ON DATABASE ... FROM PUBLIC` e `GRANT` só para o dono.
2. **IAM:** cada serviço usa um ServiceAccount com IRSA (o EKS já tem OIDC provider). Só a role do OS acessa a tabela DynamoDB.
3. **Leitura de dados de outro serviço:** sempre pela API ou por eventos (ex.: o OS valida o cliente com `GET /api/v1/clientes/{id}` do Cadastros).
4. **Código:** nenhum `DbContext` ou repositório compartilhado.
5. **AuthLambda (D-11):**
   - **Opção A (recomendada):** chama um endpoint interno do Cadastros (`POST /internal/clientes/autenticar`), não publicado no API Gateway e protegido por NetworkPolicy.
   - **Opção B:** o Cadastros publica eventos de cliente e a Lambda mantém uma projeção própria.

---

## 5. Passos para a mudança

1. **Instância RDS:** `Infra.Database` continua dono da instância compartilhada; atualizar o README.
2. **Databases e usuários:** criar `os_db`, `cobranca_db`, `execucao_db` e `cadastros_db` com seus usuários por um Job Kubernetes de bootstrap (usa o segredo master, roda dentro da VPC); registrar o segredo de cada serviço no Secrets Manager. Cobrir com teste o script de papéis (item aberto do Verifier do OS).
3. **DynamoDB:** Terraform da tabela, do stream e da role IRSA no repositório do OS; VPC gateway endpoint do DynamoDB na stack de rede (`Infra.K8s`).
4. **Execução:** migrations do catálogo, dos insumos e das reservas; seed.
5. **Cadastros:** remover as tabelas que saíram; renomear a database.
6. **Migrations fora da subida:** Job com hook `pre-install`/`pre-upgrade` do Helm em cada serviço.
7. **AuthLambda:** implementar D-11 e retirar a regra do SG que libera a Lambda no RDS.
8. **Ambiente local e testes:** `docker-compose.yml` com Postgres (4 databases) e LocalStack (SQS, SNS, DynamoDB); Testcontainers nos testes de integração.
9. **Documentação:** atualizar `docs/architecture/database-selection-and-data-model.md` (ERD dos SQL, padrões de acesso do DynamoDB, justificativa SQL × NoSQL).

---

## 6. Critérios de aceite

- [ ] Pelo menos um serviço em SQL e um em NoSQL, na AWS e localmente.
- [ ] Cada serviço com credencial própria; conectar ao banco de outro serviço com ela falha.
- [ ] Nenhum código de um serviço (nem a Lambda) consulta tabelas de outro serviço.
- [ ] Migrations executadas por Job antes do rollout.
- [ ] Justificativa da escolha de cada banco documentada.

---

## 7. Riscos

- **Quatro databases na mesma instância:** a banca pode questionar o isolamento; o README deve mostrar que as credenciais não cruzam.
- **Rede:** criar databases dentro do RDS exige um executor dentro da VPC.
- **Dois bancos no OS:** risco de um handler gravar nos dois; mitigado pela regra L-01, revisão de código e teste de falha entre os passos.
- **Modelagem DynamoDB:** curva de aprendizado de PK/SK; mitigada porque a Saga tem poucos padrões de acesso.
- **Rede:** sem o VPC gateway endpoint, o tráfego para o DynamoDB passa pela NAT (custo).
