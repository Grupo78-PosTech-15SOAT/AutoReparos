# Relatório Crítico de Avaliação Arquitetural e Regras de Negócio: Sprint 2 (Fase 3 Tech Challenge SOAT)

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Documento Analisado:** `.tmp/plano_sprint2_auth_lambda_portal.md` e `.tmp/fase3_deep_gap_analysis.md`  
> **Referência Oficial:** `docs/tech-challenge/13SOAT - Fase 3 - Tech Challenge.pdf`  
> **Perfil do Avaliador:** Avaliador Crítico de Arquitetura e Regras de Negócio (Tech Challenge FIAP SOAT)  
> **Status da Avaliação:** **APROVADO COM RESSALVAS CRÍTICAS (Ajustes Mandatórios Pré-Codificação)**  

---

## 1. Diagnóstico Geral

O plano de implementação proposto para a **Sprint 2** (`.tmp/plano_sprint2_auth_lambda_portal.md`) apresenta um direcionamento técnico correto em relação às diretrizes acordadas para a **Fase 3 da FIAP SOAT**. A proposta acerta ao:
1. **Evitar a contaminação da tabela `AspNetUsers`**: O cliente final consulta exclusivamente seus dados sem a criação de contas com senha no ASP.NET Core Identity.
2. **Adotar o modelo Serverless desacoplado**: A autenticação é delegada a uma função serverless independente que valida o cliente diretamente no PostgreSQL e emite um JWT efêmero de 1 hora.
3. **Respeitar os pilares da Clean Architecture**: Separação clara entre Casos de Uso na camada de Aplicação e Interface Adapters na camada de Apresentação (Controllers).

No entanto, uma auditoria profunda do código-fonte existente (`AutoReparos.Infra`, `AutoReparos.API`, `AutoReparos.Domain`), do esquema do banco de dados e dos mecanismos do ASP.NET Core revelou **incompatibilidades fatais de SQL**, **armadilhas de autorização em Minimal APIs**, **gaps de performance serverless** e **inconsistências de nomenclatura de rotas**. Se a codificação for iniciada sem os ajustes prescritos neste relatório, a suíte de testes quebrará e haverá risco de vulnerabilidade de autorização (vazamento de rotas internas para clientes).

---

## 2. Análise Detalhada dos Requisitos de Negócio (Fase 3 FIAP SOAT)

### 2.1. Autenticação Serverless de Clientes via CPF + E-mail
- **Aderência à Especificação:** Total. O Tech Challenge exige: *"Validar o CPF do cliente; Consultar a existência e o status do cliente na base de dados; Gerar e devolver um token (JWT) válido para consumo das APIs protegidas"*.
- **Sem poluição de `AspNetUsers`:** A tabela `AspNetUsers` permanece restrita à equipe interna (Mecânicos, Atendentes, Administradores).
- **Ajuste de Negócio Mandatório (Sanitização):** O cliente pode digitar o CPF com pontuação (`123.456.789-00`) ou sem (`12345678900`), e o e-mail com caixa alta/baixa. A Lambda é obrigada a executar a normalização (remoção de caracteres não numéricos do CPF e `Trim().ToLowerInvariant()` do e-mail) antes de qualquer validação matemática ou consulta ao banco de dados.

### 2.2. Validação Matemática Estrita de CPF
- **Aderência:** Total. Deve validar os dois dígitos verificadores no algoritmo de módulo 11 e rejeitar estritamente sequências com 11 dígitos idênticos (ex.: `000.000.000-00`, `111.111.111-11`, etc.).
- **Isolamento de Código:** A lógica de validação deve residir de forma autocontida na Lambda (`CpfValidationService.cs`), sem referenciar a biblioteca `AutoReparos.Domain.dll`, garantindo que o repositório da Lambda seja 100% autônomo na futura segregação em 4 repositórios Git.

### 2.3. Consulta Direta no PostgreSQL Gerenciado & Status do Cliente
- **Regra de Status:** A verificação do campo `InativoEm` adicionado na Sprint 1 é mandatória.
  - Se o registro não for encontrado: Retornar `401 Unauthorized` com payload claro (`{"erro": "Cliente não localizado ou dados divergentes."}`).
  - Se o registro for encontrado, mas `InativoEm != null` (cliente inativo): Retornar `403 Forbidden` com payload descritivo (`{"erro": "Cadastro do cliente encontra-se inativo."}`).
- **BUG CRÍTICO DE SCHEMA IDENTIFICADO NO PLANO:**
  - O plano da Sprint 2 sugeria a seguinte consulta:
    ```sql
    -- INCORRETO (Plano Sprint 2 original):
    SELECT "Id", "Nome", "Documento_Valor", "Email_Endereco", "InativoEm"
    FROM "Clientes"
    WHERE "Documento_Valor" = @cpf AND LOWER("Email_Endereco") = LOWER(@email)
    LIMIT 1;
    ```
  - **Inspeção Real do EF Core (`ClienteMapping.cs`):**
    ```csharp
    doc.Property(d => d.Valor).HasColumnName("Documento");
    email.Property(e => e.Endereco).HasColumnName("Email");
    ```
  - As colunas reais no PostgreSQL são `"Documento"` e `"Email"`, **e NÃO** `"Documento_Valor"` nem `"Email_Endereco"`. A execução da query proposta no plano geraria imediatamente uma exceção fatal do PostgreSQL: `column "Documento_Valor" does not exist`.

### 2.4. Emissão de JWT Efêmero (1 hora) e Claims Seguras
- **Tempo de Expiração:** Exatamente 60 minutos (`TimeSpan.FromHours(1)`).
- **Assinatura:** Chave simétrica HMAC-SHA256 compartilhada via variável de ambiente (`JWT_SECRET` / `Jwt:Secret`).
- **Claims Obrigatórias:**
  - `sub` / `ClaimTypes.NameIdentifier`: ID do Cliente (GUID em formato string).
  - `role` / `ClaimTypes.Role`: `"Cliente"`.
  - `cpf`: CPF normalizado (11 dígitos numéricos).
  - `email` / `ClaimTypes.Email`: E-mail normalizado.
  - `name` / `ClaimTypes.Name`: Nome completo do cliente.

### 2.5. Endpoints do Portal do Cliente & Isolamento Total de Dados
- **Endpoints Expostos:**
  - `GET /api/clientes/meus-veiculos`: Retorna exclusivamente os veículos cadastrados para o `clienteId` autenticado.
  - `GET /api/ordem-servico/minhas-os?placa=...`: Retorna as OSs do cliente. Se o parâmetro `placa` for informado, deve validar se a placa pertence ao cliente autenticado. Caso a placa pertença a outro cliente ou não exista, deve retornar lista vazia (HTTP 200 `[]`), **jamais** dados de terceiros e **jamais** revelar a existência do veículo de terceiros.
- **Invariante de Segurança (Zero-Trust):**
  - O `clienteId` **NUNCA** pode ser aceito como parâmetro de rota ou query string pelo endpoint do portal. Ele deve ser obtido exclusivamente através do `ClaimsPrincipal` injetado pelo runtime do ASP.NET Core a partir do token criptografado.

### 2.6. Fronteira de Segurança de Rotas (Defesa da Oficina)
- Um usuário autenticado com a role `Cliente` **NUNCA** pode conseguir executar ou consultar endpoints da oficina (`/api/servicos`, `/api/insumos`, `/api/usuarios`, `/api/ordem-servico/kanban`, etc.).
- Qualquer tentativa de acesso deve resultar estritamente em **HTTP 403 Forbidden**.

---

## 3. Análise Arquitetural (Clean Arch, Serverless, Segurança & JWT)

```mermaid
flowchart TD
    subgraph Cliente_Portal["Cliente / Portal Web"]
        A["Navegador / App Angular"]
    end

    subgraph Edge["Camada de Borda (AWS API Gateway)"]
        GW["API Gateway (HTTP / REST)"]
    end

    subgraph Serverless["Serviço Autônomo Serverless (Repo 1: autoreparos-auth-lambda)"]
        L["AWS Lambda (.NET 10)"]
        CPF_V["CpfValidationService"]
        PG_POOL["NpgsqlDataSource (Singleton Pool)"]
        JWT_GEN["TokenGenerationService"]
    end

    subgraph DataTier["Banco de Dados Gerenciado (Repo 3: autoreparos-infra-db)"]
        RDS[("AWS RDS PostgreSQL 16")]
    end

    subgraph ClusterK8s["Cluster EKS (Repo 4: autoreparos-app)"]
        ING["Ingress Nginx / NLB"]
        API["AutoReparos.API (ASP.NET Core)"]
        POL_OFICINA["Policy: OperadorOficina (Admin, Mecanico, Atendente)"]
        POL_CLIENTE["Policy: Cliente"]
        UC_VEICULOS["ObterMeusVeiculosUseCase"]
        UC_OS["ObterMinhasOrdensServicoUseCase"]
    end

    %% Fluxo de Autenticação
    A -->|"1. POST /auth/cliente {cpf, email}"| GW
    GW -->|"2. Roteia Evento Proxy"| L
    L -->|"3. Valida Dígitos Módulo 11"| CPF_V
    L -->|"4. Query Parametrizada (Pool Reutilizável)"| PG_POOL
    PG_POOL -->|"5. SELECT FROM Clientes WHERE Documento..."| RDS
    RDS -->>|"6. Retorna ClienteDbRecord"| L
    L -->|"7. Emite JWT Efêmero (1h, role=Cliente)"| JWT_GEN
    L -->>|"8. 200 OK { token, expiresIn: 3600 }"| GW
    GW -->>|"9. Entrega JWT ao Portal"| A

    %% Fluxo de Consulta Protegida
    A -->|"10. GET /api/clientes/meus-veiculos [Bearer Token]"| GW
    GW -->|"11. Encaminha Requisição Protegida"| ING
    ING --> API
    API -->|"12. Valida Assinatura JWT & Checa Role"| POL_CLIENTE
    POL_CLIENTE -->|"13. Extrai ClienteId do ClaimsPrincipal"| UC_VEICULOS
    UC_VEICULOS -->|"14. Consulta Veículos do Cliente"| RDS
    API -->>|"15. 200 OK [ Meus Veículos ]"| A

    %% Bloqueio de Rota Operacional
    A -.->|"16. Tenta GET /api/servicos [Token Cliente]"| GW
    GW -.-> API
    API -.->|"17. Falha na Policy OperadorOficina"| POL_OFICINA
    POL_OFICINA -->>|"18. 403 FORBIDDEN"| A
```

### 3.1. Arquitetura da Função Serverless (`AutoReparos.AuthLambda`)
- **Independência de Repositório:** A Lambda deve ser concebida como uma Class Library / Container Lambda em .NET 10 sem nenhuma referência de projeto (`<ProjectReference>`) para `AutoReparos.Domain`, `AutoReparos.Infra` ou `AutoReparos.Application`. Suas dependências devem ser estritamente pacotes NuGet enxutos:
  - `Amazon.Lambda.Core`
  - `Amazon.Lambda.APIGatewayEvents`
  - `Amazon.Lambda.Serialization.SystemTextJson`
  - `Npgsql`
  - `System.IdentityModel.Tokens.Jwt`
- **Otimização de Cold Start e Connection Pooling:**
  - É proibido instanciar `new NpgsqlConnection` sem pooling a cada requisição.
  - Deve-se utilizar `NpgsqlDataSource` registrado de forma estática / singleton no escopo da função. Dessa forma, execuções "warm" da Lambda compartilham o pool de conexões do PostgreSQL, evitando a exaustão de conexões no RDS (`max_connections`) e reduzindo drasticamente a latência de autenticação.
- **Tratamento de CORS na Lambda:**
  - A Lambda deve retornar explicitamente os cabeçalhos de CORS (`Access-Control-Allow-Origin: *`, `Access-Control-Allow-Headers: Content-Type,Authorization`, `Access-Control-Allow-Methods: POST,OPTIONS`) e tratar requisições preflight `OPTIONS` retornando `200 OK` imediatamente.

### 3.2. Autorização Granular na API Principal (ASP.NET Core Minimal APIs)
- **Armadilha de Herança em `MapGroup`:**
  - Atualmente, todos os endpoints usam `.RequireAuthorization()` genérico (que aceita qualquer token válido).
  - Se configurarmos uma política `OperadorOficina` na raiz do grupo `app.MapGroup("/api/clientes").RequireAuthorization("OperadorOficina")`, qualquer endpoint filho (como `group.MapGet("/meus-veiculos", ...).RequireAuthorization("Cliente")`) exigirá cumulativamente **AMBAS** as políticas (`OperadorOficina` E `Cliente`), resultando em **403 Forbidden** indevido para o cliente!
  - **Solução Arquitetural Recomendada:**
    1. Registrar políticas explícitas em `DependencyInjectionApi.cs`:
       ```csharp
       services.AddAuthorization(options =>
       {
           options.AddPolicy("OperadorOficina", policy =>
               policy.RequireRole("Administrador", "Mecanico", "Atendente"));

           options.AddPolicy("Cliente", policy =>
               policy.RequireRole("Cliente"));
       });
       ```
    2. Proteger as rotas de oficina aplicando a política `OperadorOficina`.
    3. Mapear os endpoints do portal do cliente com a política `Cliente`, garantindo isolamento total.

### 3.3. Conformidade com os Padrões de Clean Architecture
- **Camada de Aplicação:** Os novos casos de uso (`ObterMeusVeiculosUseCase` e `ObterMinhasOrdensServicoUseCase`) devem residir em `AutoReparos.Application`, receber tipos primitivos ou DTOs de entrada e devolver DTOs enriquecidos de resposta.
- **Camada de Interface Adapters:** A lógica de deserialização do `ClaimsPrincipal` e montagem do `Results.Ok(...)` deve residir nos controllers da Clean Architecture (`ClientePortalController` e `OrdemServicoPortalController`), mantendo as Minimal APIs (`Endpoints/`) como meros registradores de rota.

---

## 4. Gaps Críticos e Riscos Identificados

| # | Gap / Ponto Cego | Severidade | Impacto Técnico | Ação Corretiva Obrigatória |
| :---: | :--- | :---: | :--- | :--- |
| **G1** | **Colunas incorretas no SQL da Lambda** | **FATAL** | A Lambda falha com `column "Documento_Valor" does not exist` no PostgreSQL real. | Corrigir a query SQL para `"Documento"` e `"Email"`, conforme `ClienteMapping.cs`. |
| **G2** | **Conflito de Políticas de Autorização no `MapGroup`** | **ALTA** | O cliente recebe 403 indevido em `/meus-veiculos` por herança da policy de oficina, ou rotas internas ficam expostas a clientes. | Desacoplar os grupos de rotas internas da oficina das rotas do portal do cliente no Minimal APIs. |
| **G3** | **Inconsistência na Nomenclatura das Rotas (`ordem-servico` vs `ordens-servico`)** | **MÉDIA** | Quebra de contrato com o frontend Angular e especificações existentes. | Padronizar a rota para `/api/ordem-servico/minhas-os` e registrar `/api/ordens-servico/minhas-os` como rota alternativa (alias). |
| **G4** | **Acoplamento de Dependências da Lambda** | **ALTA** | Impede a segregação do Repositório 1 (`autoreparos-auth-lambda`) na nuvem. | Garantir zero referências de projeto (`ProjectReference`) no `.csproj` da Lambda. |
| **G5** | **Ausência de Normalização de Entrada na Lambda** | **ALTA** | Cliente com CPF digitado com máscara (`123.456.789-00`) não consegue autenticar. | Criar método `NormalizarCpf(string)` na Lambda antes da validação matemática e da query no banco. |
| **G6** | **DTO de Minhas OSs Desprovido de Contexto de Veículo** | **MÉDIA** | O cliente recebe a lista de OSs mas não identifica de qual veículo se trata sem fazer queries adicionais. | Criar `OrdemServicoClienteDto` trazendo placa e modelo do veículo, ou incluir navegação ao veículo no caso de uso. |
| **G7** | **Exaustão de Conexões no PostgreSQL Gerenciado (RDS)** | **ALTA** | Conexões sem pooling a cada chamada de Lambda derrubam o banco sob pico de acessos. | Adotar `NpgsqlDataSource` estático/singleton com connection pooling ativo. |

---

## 5. Recomendações e Ajustes Obrigatórios no Plano antes da Codificação

### Ajuste 1: Query SQL Definitiva e Sanitização no `ClienteAuthDatabaseService.cs`
Ajustar o serviço de banco de dados da Lambda para executar:
```csharp
// Normalização mandatória antes da query
var cpfDigitos = new string(request.Cpf.Where(char.IsDigit).ToArray());
var emailNormalizado = request.Email.Trim().ToLowerInvariant();

const string sql = """
    SELECT "Id", "Nome", "Documento", "Email", "InativoEm"
    FROM "Clientes"
    WHERE "Documento" = @cpf AND LOWER("Email") = @email
    LIMIT 1;
""";
```

### Ajuste 2: Configuração de Autorização Robusta em `DependencyInjectionApi.cs`
Configurar políticas explícitas com nomes semânticos:
- `"OperadorOficina"`: `options.AddPolicy("OperadorOficina", p => p.RequireRole("Administrador", "Mecanico", "Atendente"));`
- `"Cliente"`: `options.AddPolicy("Cliente", p => p.RequireRole("Cliente"));`

### Ajuste 3: Estruturação dos Endpoints do Portal
No arquivo `ClienteEndpoint.cs`:
```csharp
// Subgrupo operacional da oficina (exige OperadorOficina)
var oficinaGroup = app.MapGroup("/api/clientes").RequireAuthorization("OperadorOficina");
// Mapeia CRUD de clientes...

// Endpoint específico do Portal do Cliente (exige Cliente)
app.MapGet("/api/clientes/meus-veiculos", (
    ClaimsPrincipal user, 
    ClientePortalController controller) => controller.ObterMeusVeiculos(user))
    .RequireAuthorization("Cliente")
    .WithName("GetMeusVeiculos")
    .WithTags("PortalCliente");
```

No arquivo `OrdemServicoEndpoint.cs`:
```csharp
// Endpoint do Portal do Cliente (com suporte a singular e plural)
var portalOsHandler = (
    ClaimsPrincipal user, 
    [AsParameters] MinhasOrdensServicoRequest request, 
    OrdemServicoPortalController controller) => controller.ObterMinhasOrdens(user, request);

app.MapGet("/api/ordem-servico/minhas-os", portalOsHandler)
    .RequireAuthorization("Cliente")
    .WithName("GetMinhasOrdensServico")
    .WithTags("PortalCliente");

// Alias para garantir compatibilidade com testes e documentações que usam o plural
app.MapGet("/api/ordens-servico/minhas-os", portalOsHandler)
    .RequireAuthorization("Cliente")
    .ExcludeFromDescription();
```

### Ajuste 4: Isolamento Arquitetural Estrito da Lambda
Garantir que o arquivo `AutoReparos.AuthLambda.csproj` contenha apenas:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Amazon.Lambda.Core" Version="2.5.0" />
    <PackageReference Include="Amazon.Lambda.APIGatewayEvents" Version="2.7.1" />
    <PackageReference Include="Amazon.Lambda.Serialization.SystemTextJson" Version="2.4.4" />
    <PackageReference Include="Npgsql" Version="9.0.3" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.6.1" />
  </ItemGroup>
</Project>
```
*(Zero `<ProjectReference>` para outras camadas do monorepo)*.

---

## 6. Conclusão do Avaliador

O plano da Sprint 2 é conceitualmente excelente e atende perfeitamente à visão arquitetural da Fase 3 do Tech Challenge SOAT. Com a incorporação dos **7 ajustes obrigatórios** detalhados neste relatório (em especial a correção dos nomes das colunas na query SQL, o isolamento das políticas de autorização no Minimal APIs e a normalização de entradas na Lambda), a implementação estará blindada contra bugs de persistência, quebras de contrato e falhas de segurança de acesso.

**O plano está formalmente APROVADO para execução assim que os ajustes acima forem consolidados na especificação.**
