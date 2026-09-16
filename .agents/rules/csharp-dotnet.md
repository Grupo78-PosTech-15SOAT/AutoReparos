# C# .NET 10 Coding Standards & Conventions

This rule defines the mandatory C# .NET 10 standards, modern language feature usage, and code quality practices for the `AutoReparos` backend solution.

---

## 1. Project & Framework Configuration

- **Target Framework**: `.NET 10` (`net10.0`).
- **Nullable Reference Types**: Mandatory `#nullable enable` across all projects (`<Nullable>enable</Nullable>`).
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`).
- **File Structure**: One type per file matching the type name.
- **File-Scoped Namespaces**: Always use file-scoped namespaces.

```csharp
// GOOD
namespace AutoReparos.Application.Clientes.UseCases;

public class CriarClienteUseCase(IClienteRepository repository) : ICriarClienteUseCase
{
    // ...
}
```

---

## 2. Naming Conventions & Code Style

| Target | Convention | Example |
| :--- | :--- | :--- |
| Classes, Records, Structs, Enums | `PascalCase` | `OrdemServico`, `ClienteDto` |
| Interfaces | `IPascalCase` | `IClienteRepository`, `ICriarClienteUseCase` |
| Public Properties, Methods, Events | `PascalCase` | `ExecuteAsync`, `CriadoEm`, `Telefone` |
| Private Fields (non-primary constructor) | `_camelCase` | `private readonly ILogger _logger;` |
| Method Parameters, Local Variables | `camelCase` | `clienteId`, `normalizedEmail` |
| Constants | `PascalCase` | `MaxNomeLength` |
| Enums | Prefix with `E` (Domain standard) | `EStatusOrdemServico`, `ETipoDocumento` |

---

## 3. C# 12 / 13 & .NET 10 Features

### 3.1 Primary Constructors
Use primary constructors for dependency injection in classes and for simple data containers.

```csharp
// GOOD: Primary Constructor in UseCases / Controllers
public class CriarClienteUseCase(IClienteRepository repository) : ICriarClienteUseCase
{
    public async Task<ClienteDto> ExecuteAsync(ClienteCreateDto dto, CancellationToken cancellationToken = default)
    {
        // repository is directly captured
    }
}
```

### 3.2 Records & Immutability
- Use `record` or `record struct` for **DTOs**, **Value Objects**, **Commands**, and **Queries**.
- Use positional parameters for concise, immutable definitions.

```csharp
// GOOD: DTO as Record
public record ClienteCreateDto(
    string Nome,
    string Documento,
    string Telefone,
    string Email
);
```

### 3.3 Collection Expressions & Spread Operator
Prefer `[...]` collection expressions over `new List<T>()` or `new T[]`.

```csharp
// GOOD
List<string> statuses = ["Pendente", "EmAndamento", "Concluido"];
int[] numbers = [1, 2, ..otherNumbers, 5];
```

### 3.4 Pattern Matching & Switch Expressions
Prefer switch expressions and pattern matching over nested `if/else` checks.

```csharp
// GOOD
public decimal CalcularDesconto(EStatusOrdemServico status) => status switch
{
    EStatusOrdemServico.Aprovada => 0.05m,
    EStatusOrdemServico.EmAndamento => 0.02m,
    EStatusOrdemServico.Concluida => 0.10m,
    _ => 0.0m
};
```

---

## 4. Nullable Reference Types & Null Safety

1. **Zero Suppression Operators (`!`)**: Avoid using `!` (null-forgiving operator) except for EF Core parameterless constructors (`null!`).
2. **Guard Clauses**: Use standard framework guard methods:
   - `ArgumentNullException.ThrowIfNull(arg);`
   - `ArgumentException.ThrowIfNullOrWhiteSpace(arg);`
3. **Null Check Pattern**: Use `is null` or `is not null` instead of `== null` / `!= null`.

```csharp
// GOOD: Guard clause and null check
public static Documento Create(string valor)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(valor, nameof(valor));
    
    var normalized = Normalizar(valor);
    if (!IsValido(normalized))
        throw new InvalidDocumentoException("CPF/CNPJ inválido.");

    return new Documento(normalized);
}
```

---

## 5. Async / Await Guidelines

- **Never use `async void`**: Use `async Task` or `async ValueTask`.
- **CancellationToken**: Always accept and pass `CancellationToken` down to async IO calls (EF Core, HTTP clients, streams).
- **Avoid Blocking**: Never call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- **Lightweight Async**: Return completed tasks without `async/await` overhead when no continuation logic exists.

```csharp
// GOOD: Cancellation token propagation & pure async flow
public async Task<ClienteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    var cliente = await repository.GetByIdAsync(id, cancellationToken);
    return cliente is not null ? ClienteMapper.ToDto(cliente) : null;
}
```

---

## 6. LINQ Best Practices

- **Avoid Multiple Enumeration**: Call `.ToList()` or `.ToArray()` if materializing an `IEnumerable<T>` to iterate multiple times.
- **`Any()` over `Count() > 0`**: Use `.Any()` to test for existence.
- **Asymmetric Filtering**: Keep LINQ expressions inside EF Core queries server-side (`IQueryable`), materializing into DTOs via `.Select()` to minimize data transfer.

```csharp
// GOOD: Projection directly to DTO
var dtos = await dbContext.Clientes
    .AsNoTracking()
    .Where(c => c.Nome.Contains(searchTerm))
    .Select(c => new ClienteDto(c.Id, c.Nome, c.Documento.Valor, c.Telefone.Valor, c.Email.Valor))
    .ToListAsync(cancellationToken);
```

---

## 7. Structured Logging & Telemetry

- Inject `ILogger<T>` into handlers and use structured templates (message templates with named parameters).
- **Do NOT** use string interpolation inside logging calls (prevents template caching and metrics aggregation).

```csharp
// GOOD: Structured logging
logger.LogInformation("Cliente {ClienteId} criado com sucesso para o documento {Documento}", cliente.Id, cliente.Documento.Valor);

// BAD: String interpolation in logger
logger.LogInformation($"Cliente {cliente.Id} criado");
```
