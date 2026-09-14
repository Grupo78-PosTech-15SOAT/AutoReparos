# Plano de Implementação - Sprint 2 (Fase 3 Tech Challenge SOAT)
## Revisão Pós-Auditoria Crítica de Arquitetura, Padrões e AI Harness

> **Branch de Trabalho:** `feat/fase3-sprint2-auth-lambda-e-portal`  
> **Branch Base:** `feat/fase3-backend-fixes`  
> **Status do Plano:** **REVISADO E BLINDADO APÓS AUDITORIA CRÍTICA**  
> **Objetivo da Sprint:** Implementar a arquitetura de autenticação serverless de clientes sem atrito (CPF + E-mail), os endpoints protegidos do portal do cliente no backend, a suíte de testes correspondente e a preparação da segregação em 4 repositórios independentes.

---

## 1. Contexto & Requisitos da Banca (Fase 3 FIAP SOAT)

1. **Autenticação Serverless de Clientes:**
   - O cliente final **não possui conta com senha** no ASP.NET Identity (`AspNetUsers` permanece 100% exclusiva para mecânicos, atendentes e administradores).
   - O cliente informa apenas **CPF** e **E-mail** pré-cadastrados na oficina.
   - Uma **Function Serverless** (`AutoReparos.AuthLambda` / AWS Lambda .NET 10 desacoplada) é acionada:
     - **Normalização prévia:** Extrai apenas dígitos do CPF e sanitiza o e-mail (`Trim().ToLowerInvariant()`).
     - **Validação matemática:** Rejeita sequências repetidas (`111.111.111-11`, etc.) e valida os 2 dígitos verificadores (módulo 11).
     - **Consulta direta no PostgreSQL:** Conecta ao banco gerenciado e valida existência e status ativo (`InativoEm IS NULL`).
     - **Emissão de JWT efêmero (1 hora):** Assinado via HMAC-SHA256 compartilhando a chave simétrica (`JWT_SECRET`) com claims: `sub` (ClienteId GUID), `cpf`, `email`, `role: "Cliente"`.
2. **Endpoints Restritos do Cliente na API Principal:**
   - Com o token em mãos, o cliente acessa:
     - `GET /api/clientes/meus-veiculos`: lista exclusivamente os veículos vinculados ao `clienteId` extraído do token.
     - `GET /api/ordem-servico/minhas-os?placa=...` (com alias `/api/ordens-servico/minhas-os`): consulta o histórico de OSs do cliente (com modelo e placa do veículo), com filtro opcional por placa.
   - **Isolamento de Dados (Zero-Trust):** Se o cliente consultar uma placa pertencente a outro cliente ou inexistente, a API retorna lista vazia (`[]`), sem vazar dados ou expor a existência de outros registros.
   - **Defesa de Rotas Internas:** Tokens com `Role: Cliente` recebem estritamente `403 Forbidden` ao tentar acessar rotas operacionais da oficina (`/api/servicos`, `/api/insumos`, `/api/usuarios`, kanban, fila, fluxo de oficina).

---

## 2. Ajustes Mandatórios da Auditoria Crítica Incorporados

| Item Auditado | Diagnóstico Crítico | Correção Aplicada no Plano |
| :--- | :--- | :--- |
| **Schema SQL no Postgres** | O rascunho previa `Documento_Valor` e `Email_Endereco`. No mapeamento real (`ClienteMapping.cs`), as colunas são `"Documento"` e `"Email"`. | Query corrigida para utilizar `"Documento"` e `"Email"`. |
| **Isolamento de Repositórios Git** | Risco de acoplamento da Lambda com `AutoReparos.Domain.dll`, inviabilizando o repositório 2 (`autoreparos-auth-lambda`). | O projeto da Lambda é **100% autônomo**, com zero `ProjectReference`, possuindo seus próprios records e lógica de validação. |
| **Conexões & Cold Start** | Abrir conexão direta `new NpgsqlConnection` a cada request gera exaustão de pool no RDS PostgreSQL. | Utilização de `NpgsqlDataSource` estático/singleton com connection pooling otimizado. |
| **Herança no Minimal APIs** | No ASP.NET Core, adicionar `.RequireAuthorization("OperadorOficina")` no `MapGroup` pai força a exigência de ambas as policies aos filhos (lógica AND). | Separação explícita no mapeamento: subgrupos/rotas da oficina usam `RequireAuthorization("OperadorOficina")`, rotas do portal usam `RequireAuthorization("ClientePolicy")`. |
| **Assinatura Assíncrona** | Omissão de `CancellationToken` em operações de I/O. | Todas as assinaturas assíncronas passam a aceitar `CancellationToken`. |
| **Rota Singular vs Plural** | A convenção de rotas da API e frontend é singular (`/api/ordem-servico`). | Endpoint registrado como `/api/ordem-servico/minhas-os` com alias `/api/ordens-servico/minhas-os`. |

---

## 3. Arquitetura e Estrutura dos Componentes

```
AutoReparos/
├── AutoReparos.AuthLambda/                       # Projeto Serverless AWS Lambda (.NET 10 - Totalmente Autônomo)
│   ├── Functions/
│   │   └── ClienteAuthFunction.cs               # Handler Lambda (APIGatewayProxyRequest -> APIGatewayProxyResponse)
│   ├── Services/
│   │   ├── ICpfValidationService.cs             # Validação matemática de CPF e sequências
│   │   ├── CpfValidationService.cs
│   │   ├── IClienteAuthDatabaseService.cs       # Consulta direta ao Postgres via NpgsqlDataSource
│   │   ├── ClienteAuthDatabaseService.cs
│   │   ├── ITokenGenerationService.cs           # Emissão do JWT efêmero (1h)
│   │   └── TokenGenerationService.cs
│   ├── Models/
│   │   ├── ClienteAuthRequest.cs                # Request { cpf, email }
│   │   ├── ClienteAuthResponse.cs               # Response { token, expiresIn, nome, email }
│   │   └── ClienteDbRecord.cs                   # Record leve { Id, Nome, Documento, Email, InativoEm }
│   └── AutoReparos.AuthLambda.csproj            # Zero referências para outros projetos da solution
│
├── AutoReparos.AuthLambda.Tests/                 # Testes Unitários da Lambda (xUnit, FluentAssertions, Moq)
│   ├── CpfValidationServiceTests.cs             # CPFs válidos, inválidos, sequências homogêneas, formatação
│   ├── TokenGenerationServiceTests.cs           # Validação de claims, expiração de 1h, chave simétrica
│   └── ClienteAuthFunctionTests.cs              # 200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 500 Error
│
├── AutoReparos.Application/                      # Casos de Uso na Aplicação Principal
│   ├── Clientes/
│   │   ├── DTOs/Response/MeusVeiculosDto.cs
│   │   ├── UseCases/ObterMeusVeiculosUseCase.cs # Busca veículos pelo ClienteId extraído do token
│   │   └── UseCases/Interfaces/IObterMeusVeiculosUseCase.cs
│   └── OrdensServicos/
│       ├── DTOs/Response/MinhasOrdensServicoDto.cs # Contém OS, status, valor, modelo e placa do veículo
│       ├── UseCases/ObterMinhasOrdensServicoUseCase.cs # Valida propriedade do veículo e busca OSs
│       └── UseCases/Interfaces/IObterMinhasOrdensServicoUseCase.cs
│
├── AutoReparos.API/                              # Endpoints e Segurança
│   ├── Controllers/
│   │   ├── ClientePortalController.cs           # Interface Adapter para Meus Veículos
│   │   └── OrdemServicoPortalController.cs      # Interface Adapter para Minhas OSs
│   ├── Endpoints/
│   │   ├── ClienteEndpoint.cs                   # Mapeia /api/clientes/meus-veiculos (policy ClientePolicy)
│   │   └── OrdemServicoEndpoint.cs              # Mapeia /api/ordem-servico/minhas-os (policy ClientePolicy)
│   └── DependencyInjectionAPI.cs                 # Políticas "OperadorOficina" e "ClientePolicy"
│
└── AutoReparos.IntegrationTests/                 # Testes de Integração com Testcontainers PostgreSQL Real
    ├── Features/PortalCliente/
    │   ├── MeusVeiculosIntegrationTests.cs      # Testa fluxo do token cliente -> veículos do cliente
    │   ├── MinhasOrdensServicoIntegrationTests.cs # Testa fluxo do token cliente -> OSs e bloqueio de placa de terceiros
    │   └── SegurancaRotasIntegrationTests.cs    # Valida 403 Forbidden em rotas operacionais com token Cliente
    └── Lambda/
        └── ClienteAuthLambdaIntegrationTests.cs # Valida a Lambda contra o PostgreSQL real com Testcontainers
```

---

## 4. Plano Passo a Passo de Execução

### Passo 1: Construção da Function Serverless Autônoma (`AutoReparos.AuthLambda`)
1. Criar projeto class library em .NET 10 (`AutoReparos.AuthLambda`) com `<Nullable>enable</Nullable>` e dependências NuGet ultraleves:
   - `Amazon.Lambda.Core`
   - `Amazon.Lambda.APIGatewayEvents`
   - `Amazon.Lambda.Serialization.SystemTextJson`
   - `Npgsql`
   - `System.IdentityModel.Tokens.Jwt`
   - *(NENHUMA dependência de `AutoReparos.Domain` ou `AutoReparos.Infra`)*.
2. Implementar `CpfValidationService`:
   - Sanitização: extrai apenas dígitos numéricos.
   - Rejeição de strings de tamanho diferente de 11.
   - Rejeição de 11 dígitos repetidos (`000.000.000-00` a `999.999.999-99`).
   - Cálculo do 1º e 2º dígitos verificadores (módulo 11).
3. Implementar `ClienteAuthDatabaseService`:
   - Utilização de `NpgsqlDataSource` estático singleton.
   - Query parametrizada corrigida:
     ```sql
     SELECT "Id", "Nome", "Documento", "Email", "InativoEm"
     FROM "Clientes"
     WHERE "Documento" = @cpf AND LOWER("Email") = LOWER(@email)
     LIMIT 1;
     ```
   - Validação de status: se `InativoEm != null`, retorna indicador `ClienteInativo`.
4. Implementar `TokenGenerationService`:
   - Emissão de JWT assinado com a chave simétrica compartilhada (`JWT_SECRET`).
   - Claims:
     - `ClaimTypes.NameIdentifier` (`sub`) = `cliente.Id`
     - `ClaimTypes.Role` = `"Cliente"`
     - `ClaimTypes.Email` = `cliente.Email`
     - `ClaimTypes.Name` = `cliente.Nome`
     - `cpf` = `cliente.Documento`
     - Expiração: exatamente 60 minutos (`TimeSpan.FromHours(1)`).
5. Implementar `ClienteAuthFunction`:
   - Decorar com `[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]`.
   - Headers CORS obrigatórios em todas as respostas (`Access-Control-Allow-Origin: *`, `Access-Control-Allow-Headers: *`).
   - Retornos:
     - `200 OK`: `{ "token": "...", "expiresIn": 3600, "nome": "...", "email": "..." }`
     - `400 Bad Request`: `{ "erro": "CPF ou E-mail em formato inválido." }`
     - `401 Unauthorized`: `{ "erro": "Cliente não localizado ou dados divergentes." }`
     - `403 Forbidden`: `{ "erro": "Cadastro do cliente encontra-se inativo." }`
     - `500 Internal Server Error`: `{ "erro": "Erro interno ao processar a autenticação." }` (com log estruturado).
6. Criar projeto de testes unitários `AutoReparos.AuthLambda.Tests` com cobertura completa:
   - `CpfValidationServiceTests`: casos de sucesso, formatações, dígitos incorretos, sequências repetidas.
   - `TokenGenerationServiceTests`: asserção de claims, validação com `TokenValidationParameters` oficiais.
   - `ClienteAuthFunctionTests`: simulação de todos os status HTTP e headers CORS.

---

### Passo 2: Endpoints Restritos do Cliente na API Principal
1. **Configuração de Políticas de Autorização (`DependencyInjectionApi.cs`):**
   ```csharp
   services.AddAuthorization(options =>
   {
       options.AddPolicy("OperadorOficina", policy =>
           policy.RequireRole("Administrador", "Atendente", "Mecanico"));
       options.AddPolicy("ClientePolicy", policy =>
           policy.RequireRole("Cliente"));
   });
   ```
2. **Proteção das Rotas Operacionais:**
   - Assegurar que as rotas operacionais (`/api/servicos`, `/api/insumos`, `/api/usuarios`, fluxo de OS, etc.) exijam a policy `OperadorOficina`.
3. **Casos de Uso na Aplicação (`AutoReparos.Application`):**
   - `ObterMeusVeiculosUseCase`:
     - Recebe `clienteId` e `CancellationToken`.
     - Executa `IVeiculoRepository.GetAll(clienteId, ...)`.
     - Mapeia para DTO contendo modelo, marca, placa, ano e quilometragem.
   - `ObterMinhasOrdensServicoUseCase`:
     - Recebe `clienteId`, `placa?` e `CancellationToken`.
     - Se informada placa: busca o veículo via `IVeiculoRepository.GetByPlaca(placa)`. Se o veículo não existir ou `veiculo.ClienteId != clienteId`, retorna lista vazia `[]` imediatamente (proteção contra enumeração/vazamento).
     - Busca as OSs via `IOrdemServicoRepository.GetAll(clienteId, veiculoId, ...)`.
     - Retorna DTO enriquecido com dados do veículo e status da OS.
4. **Controllers & Endpoints:**
   - `ClientePortalController` e `OrdemServicoPortalController` extraem o `clienteId` do `ClaimsPrincipal`.
   - Registrar no `ClienteEndpoint.cs`:
     - `GET /api/clientes/meus-veiculos` sob a policy `ClientePolicy`.
   - Registrar no `OrdemServicoEndpoint.cs`:
     - `GET /api/ordem-servico/minhas-os` e alias `GET /api/ordens-servico/minhas-os` sob a policy `ClientePolicy`.

---

### Passo 3: Testes de Integração com Testcontainers PostgreSQL
1. Criar testes em `AutoReparos.IntegrationTests`:
   - `ClienteAuthLambdaIntegrationTests`: executa a função Lambda contra o banco real do Testcontainers (testando cliente ativo, cliente inexistente e cliente inativado com status 403).
   - `PortalClienteIntegrationTests`:
     - Obtém o token emitido pela Lambda para o cliente seeded.
     - Chama `GET /api/clientes/meus-veiculos` com o Bearer token e valida se retornou apenas o veículo do cliente.
     - Chama `GET /api/ordem-servico/minhas-os` e valida os dados retornados.
     - Chama `GET /api/ordem-servico/minhas-os?placa=XYZ2E34` (placa de outro cliente) e valida que retorna lista vazia `[]` sem erro 500 nem dados alheios.
   - `SegurancaRotasIntegrationTests`:
     - Tenta acessar `POST /api/servicos`, `GET /api/ordem-servico/kanban` e `GET /api/clientes` com o token de `Cliente` e valida que a API retorna **403 Forbidden**.
     - Tenta acessar `/api/clientes/meus-veiculos` sem token e valida que retorna **401 Unauthorized**.

---

### Passo 4: Verificação Empírica, Testes Manuais Docker Compose & Curl
1. Executar `dotnet build AutoReparos.slnx`.
2. Executar `dotnet test AutoReparos.slnx` (todos os 291 testes existentes + novos testes da Lambda e do portal).
3. Executar `yarn --cwd AutoReparos.Web build` para assegurar que nenhuma alteração quebrou o frontend.
4. Rebuild do container `autoreparos-api` no Docker Compose.
5. Execução de requisições manuais via `curl`:
   - Chamada à autenticação do cliente.
   - Consulta de veículos com token Bearer.
   - Consulta de ordens de serviço com e sem filtro de placa.
   - Prova de bloqueio 403 em rotas operacionais.

---

### Passo 5: Estruturação para os 4 Repositórios Git & Pull Request
1. Documentar o roteiro de segregação dos 4 repositórios:
   - `autoreparos-app` (API + Web Frontend)
   - `autoreparos-auth-lambda` (Function Serverless totalmente autônoma)
   - `autoreparos-infra-db` (Terraform RDS)
   - `autoreparos-infra-k8s` (Terraform EKS + Gateway)
2. Finalização das alterações na branch `feat/fase3-sprint2-auth-lambda-e-portal`, apresentação do relatório final e criação do PR para a branch base.

---

## 5. Matriz de Critérios de Aceite

| Requisito | Critério de Aceite |
| :--- | :--- |
| **Normalização & Sanitização** | Remove formatação do CPF (pontos e traço) e padroniza e-mail com trim/lowercase. |
| **Validação Matemática de CPF** | Rejeita CPFs inválidos ou sequências repetidas; aceita CPFs matematicamente válidos. |
| **Consulta no PostgreSQL Real** | Consulta colunas `"Documento"` e `"Email"`; rejeita cliente inexistente (401) e inativo (403). |
| **Token Efêmero de 1 hora** | Emite JWT com validade de 60 minutos contendo `Role: Cliente`, `clienteId`, `cpf`, `email` e `name`. |
| **Autonomia da Lambda** | Projeto `AutoReparos.AuthLambda` sem referências para outras DLLs do projeto. |
| **Isolamento de Dados (Zero-Trust)** | Cliente autenticado só visualiza seus próprios veículos e OSs. Placa de terceiro retorna lista vazia. |
| **Defesa das Rotas de Oficina** | Token de cliente recebe estritamente 403 Forbidden ao tentar acessar rotas operacionais. |
| **Qualidade & Testes** | 100% dos testes unitários e de integração com Testcontainers PostgreSQL passando sem falhas. |
