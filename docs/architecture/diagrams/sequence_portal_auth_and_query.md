# Diagrama de Sequência — Autenticação e Consulta no Portal do Cliente

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica
> **Fase:** Tech Challenge FIAP SOAT — Fase 3
> **Documentos relacionados:** [RFC-003](../RFC-003-serverless-client-authentication.md), [ADR-002](../ADR-002-data-isolation-and-zero-trust-claims.md), [ADR-003](../ADR-003-end-to-end-observability-strategy.md)

Este documento detalha o fluxo completo do portal do cliente, do login com CPF e e-mail
até a consulta isolada dos próprios dados — o caminho demonstrado ao vivo no vídeo de
entrega.

---

## 1. Fluxo principal (caminho feliz)

```mermaid
sequenceDiagram
    autonumber
    actor C as Cliente
    participant P as Portal Angular
    participant G as API Gateway v2
    participant L as Lambda<br/>AutoReparos.AuthLambda
    participant D as RDS PostgreSQL 16
    participant A as AutoReparos.API<br/>(EKS)
    participant O as OTel Collector

    rect rgb(238, 245, 255)
    note over C,D: Fase 1 — Autenticação serverless (RFC-003)
    C->>P: Informa CPF e e-mail
    P->>G: POST /auth/cliente {cpf, email}
    G->>L: AWS_PROXY (APIGatewayProxyRequest)
    L->>L: Valida CPF por módulo 11<br/>e normaliza o e-mail
    L->>D: SELECT Id, Nome, Documento, Email, InativoEm<br/>FROM "Clientes" WHERE Documento=@cpf<br/>AND LOWER(Email)=LOWER(@email)
    D-->>L: 1 registro (InativoEm = NULL)
    L->>L: Gera JWT HMAC-SHA256<br/>claims: sub, name, email, cpf, role=Cliente<br/>exp: 1 hora
    L-->>G: 200 {token, expiresIn: 3600, nome, email, role}
    G-->>P: 200
    P->>P: Armazena o token para a sessão
    end

    rect rgb(240, 250, 240)
    note over C,O: Fase 2 — Consulta isolada (ADR-002)
    C->>P: Abre "Meus veículos"
    P->>G: GET /api/clientes/meus-veiculos<br/>Authorization: Bearer {token}
    G->>A: HTTP_PROXY → Ingress do EKS
    A->>A: JwtBearer valida a assinatura<br/>com o segredo compartilhado
    A->>A: ClientePolicy exige role = Cliente
    A->>A: PortalClienteController extrai<br/>ClienteId da claim NameIdentifier/sub
    A->>D: SELECT ... FROM "Veiculos"<br/>WHERE "ClienteId" = @clienteIdDaClaim
    D-->>A: Apenas os veículos do titular
    A-)O: OTLP gRPC — trace, métricas e logs<br/>(TraceId correlacionado)
    A-->>G: 200 [VeiculoDto]
    G-->>P: 200
    P-->>C: Lista dos próprios veículos
    end
```

**O ponto que a banca precisa ver no passo 17:** o `ClienteId` usado na cláusula `WHERE`
vem da **claim do token**, não da requisição. Não existe parâmetro no contrato do
endpoint capaz de alterá-lo.

---

## 2. Consulta de ordens de serviço com filtro por placa

O filtro opcional por placa é onde o isolamento Zero-Trust fica visível, porque três
situações diferentes convergem para **a mesma resposta**.

```mermaid
sequenceDiagram
    autonumber
    actor C as Cliente
    participant P as Portal Angular
    participant G as API Gateway v2
    participant A as AutoReparos.API
    participant U as ObterMinhasOrdensServicoUseCase
    participant D as RDS PostgreSQL 16

    C->>P: Filtra por uma placa
    P->>G: GET /api/ordem-servico/minhas-os?placa=ABC1D23<br/>Authorization: Bearer {token}
    G->>A: HTTP_PROXY → EKS
    A->>A: Valida JWT + ClientePolicy
    A->>U: ExecuteAsync(clienteIdDaClaim, placa)
    U->>D: SELECT ... FROM "Veiculos" WHERE "Placa" = @placa
    D-->>U: Veículo (ou nenhum)

    alt Placa inexistente OU pertencente a outro cliente
        U-->>A: Enumerable vazio
        A-->>P: 200 []
        note right of U: Zero Data Leakage:<br/>resposta idêntica nos dois casos.<br/>A API não confirma nem nega<br/>a existência do registro.
    else Placa pertence ao cliente autenticado
        U->>D: SELECT ... FROM "OrdensServico"<br/>WHERE "ClienteId" = @clienteId<br/>AND "VeiculoId" = @veiculoId
        D-->>U: Ordens de serviço do titular
        U-->>A: [MinhaOrdemServicoDto]
        A-->>P: 200 com as OS daquele veículo
    end
```

---

## 3. Caminhos de exceção

```mermaid
sequenceDiagram
    autonumber
    participant P as Portal / Cliente
    participant G as API Gateway v2
    participant L as Lambda de autenticação
    participant A as AutoReparos.API
    participant D as RDS PostgreSQL

    rect rgb(255, 245, 245)
    note over P,D: Falhas na autenticação
    P->>G: POST /auth/cliente {cpf: "111.111.111-11", email}
    G->>L: AWS_PROXY
    L--xD: CPF reprovado no módulo 11 —<br/>o banco nunca é consultado
    L-->>P: 400 {erro: "CPF inválido."}

    P->>G: POST /auth/cliente {cpf válido, e-mail divergente}
    G->>L: AWS_PROXY
    L->>D: Consulta o par CPF + e-mail
    D-->>L: Nenhum registro
    L-->>P: 401 {erro: "Cliente não localizado ou dados divergentes."}
    note right of L: Mensagem idêntica para<br/>"CPF não existe" e "e-mail não confere",<br/>evitando enumeração de cadastro.

    P->>G: POST /auth/cliente {cliente inativado}
    G->>L: AWS_PROXY
    L->>D: Consulta o par CPF + e-mail
    D-->>L: Registro com InativoEm preenchido
    L-->>P: 403 {erro: "Cadastro do cliente encontra-se inativo."}
    end

    rect rgb(255, 250, 240)
    note over P,A: Falhas na autorização
    P->>A: GET /api/clientes/meus-veiculos (sem Authorization)
    A-->>P: 401 Unauthorized

    P->>A: GET /api/clientes (token de Cliente em rota operacional)
    A->>A: OperadorOficina exige Administrador,<br/>Atendente ou Mecanico
    A-->>P: 403 Forbidden
    end
```

---

## 4. Como a telemetria acompanha o fluxo

Durante todo o trecho que roda no EKS, o SDK OpenTelemetry emite os três sinais para o
Collector, que os distribui ([ADR-003](../ADR-003-end-to-end-observability-strategy.md)):

```mermaid
flowchart LR
    A["AutoReparos.API<br/>span da requisição<br/>+ log JSON com TraceId/SpanId<br/>+ métricas"]
    COL["OTel Collector"]
    JAE["Jaeger — trace completo<br/>HTTP → EF Core → Npgsql"]
    PROM["Prometheus — métricas<br/>prefixo autoreparos_"]
    LOKI["Loki — logs JSON"]
    NR["New Relic / Datadog<br/>(overlay opcional)"]

    A -->|OTLP gRPC :4317| COL
    COL --> JAE
    COL --> PROM
    COL --> LOKI
    COL -.-> NR
```

Na prática, isso permite partir de um pico no dashboard, abrir o trace correspondente no
Jaeger e chegar à linha de log exata daquela requisição no Loki, pelo mesmo `TraceId`.

**Lacuna conhecida:** a Lambda de autenticação **não** está instrumentada com
OpenTelemetry — ela registra apenas via `ILambdaContext`. O trace distribuído começa,
efetivamente, na chegada ao backend; o salto Portal → API Gateway → Lambda não aparece
no Jaeger.

---

## 5. Roteiro de reprodução local

Para exercitar o fluxo sem AWS, com o stack de observabilidade completo:

```bash
cp .env.example .env            # preencher as senhas
docker compose up -d postgres otel-collector prometheus jaeger loki
dotnet build AutoReparos.slnx
dotnet run --project submodules/AutoReparos.App/AutoReparos.API
```

Pontos que costumam custar tempo:

- O PostgreSQL do compose é publicado em **`5433`** no host, não em `5432`.
- O `launchSettings.json` sobrepõe `ASPNETCORE_URLS`: a API sobe em **`http://localhost:5137`**.
- O usuário operacional de seed é **`admin@autoreparos.com`**.
- Métricas recém-emitidas levam **até 60 segundos** para aparecer no Prometheus, e
  qualquer `rate()`/`increase()` sobre elas exige janela de **`[5m]`** ou maior.
