# Clean Architecture, DDD & Vertical Slice Architecture

This rule defines the architectural principles, layer boundaries, Domain-Driven Design (DDD) constructs, and Vertical Slice structure for `AutoReparos`.

---

## 1. Architectural Layers & Dependency Flow

Dependencies **MUST ONLY** flow inward:

```
[ AutoReparos.API ]  ───────┐
                            ▼
[ AutoReparos.Infra ] ──► [ AutoReparos.Application ] ──► [ AutoReparos.Domain ]
```

### Layer Responsibilities & Constraints

| Layer | Responsibility | Allowed Dependencies | Prohibited Dependencies |
| :--- | :--- | :--- | :--- |
| **`AutoReparos.Domain`** | Enterprise Business Rules, Entities, Value Objects, Aggregates, Domain Events, Domain Exceptions, Repository Interfaces. | **None** (.NET Standard / BCL only) | EF Core, ASP.NET Core, Infrastructure libraries, Application layer. |
| **`AutoReparos.Application`** | Application Business Rules, Use Cases (Vertical Slices), DTOs, Mappers, Interface Contracts. | `AutoReparos.Domain` | EF Core `DbContext`, HTTP context, Infra implementations. |
| **`AutoReparos.Infra`** | Database Persistence (EF Core / Npgsql), External APIs (SendGrid), JWT Auth, Repositories implementations. | `AutoReparos.Application`, `AutoReparos.Domain` | ASP.NET Controllers, Presentation logic. |
| **`AutoReparos.API`** | Endpoints / Controllers, Routing, Middleware, Authentication setup, Dependency Injection composition. | `AutoReparos.Application`, `AutoReparos.Infra` | Direct EF Core DbContext calls (must use Use Cases). |

---

## 2. Domain-Driven Design (DDD) Rules

### 2.1 Aggregate Roots & Entities
1. Inherit from `Entity` base class (`AutoReparos.Domain.Shared.Entity`).
2. Encapsulate state: Properties must have `private set` or `{ get; }`.
3. Invariant Protection: State changes must happen via domain methods, never by directly mutating public setters.
4. Parameterless Constructor: Keep a `protected` parameterless constructor for EF Core materialization.

```csharp
// GOOD: Aggregate Root with Encapsulated Mutators
public class OrdemServico : Entity
{
    public EStatusOrdemServico Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    private readonly List<OrdemServicoServico> _servicos = [];
    public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();

    protected OrdemServico() { } // Required for EF Core

    public void AdicionarServico(Servico servico, int quantidade)
    {
        if (Status != EStatusOrdemServico.Orcamento)
            throw new InvalidOrdemServicoException("Não é possível alterar serviços de uma OS já aprovada.");

        _servicos.Add(new OrdemServicoServico(Id, servico.Id, quantidade, servico.PrecoBase));
        RecalcularValorTotal();
    }
}
```

### 2.2 Value Objects
1. Value objects represent domain concepts without identity (e.g., `Documento`, `Telefone`, `Email`).
2. Must be immutable.
3. Must validate themselves upon instantiation via factory methods (`Create`).

```csharp
// GOOD: Self-validating Value Object
public record Email
{
    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    public static Email Create(string rawEmail)
    {
        if (string.IsNullOrWhiteSpace(rawEmail))
            throw new InvalidEmailException("E-mail é obrigatório.");

        var normalized = rawEmail.Trim().ToLower();
        if (!normalized.Contains('@'))
            throw new InvalidEmailException("Formato de e-mail inválido.");

        return new Email(normalized);
    }
}
```

### 2.3 Repositories
1. **Aggregate Root Boundaries**: Repositories are defined ONLY for Aggregate Roots (e.g., `IClienteRepository`, `IOrdemServicoRepository`). Never create a repository for a child entity or value object (`IOrdemServicoItemRepository` is strictly forbidden).
2. **Interface Location**: Repository interfaces reside in `AutoReparos.Domain/{Aggregate}/Repositories/`.

---

## 3. Vertical Slice Architecture (Application Layer)

The Application layer is organized by **Feature Context Slices** rather than horizontal layers (e.g. `Clientes`, `OrdensServicos`, `Insumos`).

```
AutoReparos.Application/
├── Clientes/
│   ├── DTOs/
│   │   ├── Request/ (ClienteCreateDto.cs, ClienteUpdateDto.cs, ClientePagedRequest.cs)
│   │   └── Response/ (ClienteDto.cs)
│   ├── Mappers/ (ClienteMapper.cs)
│   └── UseCases/
│       ├── Interfaces/ (ICriarClienteUseCase.cs, IListarClientesUseCase.cs)
│       ├── CriarClienteUseCase.cs
│       └── ListarClientesUseCase.cs
```

### Use Case Design Rules
1. **Single Responsibility**: One Use Case per user action/business scenario (`CriarClienteUseCase`, `AprovarOrdemServicoUseCase`).
2. **Interface Contract**: Every Use Case implements its corresponding interface (e.g., `ICriarClienteUseCase`).
3. **Primary Constructor Injection**: Inject domain repositories and services via primary constructor.
4. **DTO Input & Output**: Use Cases accept Request DTOs and return Response DTOs or primitive results. Domain entities must **NEVER** leak past the Application layer into the API responses.

```csharp
// GOOD: Vertical Slice Use Case implementation
public class CriarClienteUseCase(IClienteRepository repository) : ICriarClienteUseCase
{
    public async Task<ClienteDto> ExecuteAsync(ClienteCreateDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedDocumento = Documento.Normalizar(dto.Documento);
        var clienteExiste = await repository.GetByDocumentoOrEmail(normalizedDocumento, dto.Email);
        
        if (clienteExiste is not null)
            throw new InvalidClienteException("Cliente já cadastrado com este documento ou e-mail.");

        var cliente = new Cliente(dto.Nome, dto.Documento, dto.Telefone, dto.Email);
        await repository.Create(cliente, cancellationToken);

        return ClienteMapper.ToDto(cliente);
    }
}
```

---

## 4. Exception Strategy

- **Domain Exceptions**: Derived from `DomainException` (e.g., `InvalidClienteException`, `NotFoundException`). Thrown by Domain Entities, Value Objects, or Use Case validation checks.
- **Global Exception Middleware**: `GlobalExceptionHandler` in `AutoReparos.API` catches domain exceptions and maps them to HTTP Problem Details (`400 Bad Request`, `404 Not Found`, `409 Conflict`). Use Cases should NOT catch domain exceptions just to log and rethrow.
