# PostgreSQL & EF Core Npgsql Best Practices

This rule defines data persistence standards, EF Core 10 entity mapping rules, PostgreSQL optimization, indexing strategies, and connection resilience for `AutoReparos`.

---

## 1. Stack & Provider Configuration

- **Provider**: `Npgsql.EntityFrameworkCore.PostgreSQL` (Version `10.0.1`).
- **DbContext**: Configured in `AutoReparos.Infra` and registered in DI via `AddDbContext<AppDbContext>`.
- **Connection Resilience**: Always configure Npgsql retry on transient failures.

```csharp
// GOOD: DbContext Registration with Resilience Strategy
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsAssembly("AutoReparos.Infra");
    }));
```

---

## 2. Entity Configuration & Mapping Standards

1. **Fluent API Configuration**: Do not pollute Domain entities with DataAnnotation attributes (`[Table]`, `[Column]`, `[Required]`). Use `IEntityTypeConfiguration<T>` in `AutoReparos.Infra`.
2. **Primary Keys**: Use `Guid` with PostgreSQL native UUID generation (`gen_random_uuid()`).
3. **Value Objects Mapping**: Use EF Core `ComplexProperty` or `OwnsOne` to map Value Objects into database columns seamlessly.
4. **Timestamps**: All timestamps must store UTC values (`timestamp with time zone`).

```csharp
// GOOD: Fluent API Configuration (IEntityTypeConfiguration<T>)
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Nome)
               .IsRequired()
               .HasMaxLength(100);

        // Value Object mapping using ComplexProperty
        builder.ComplexProperty(c => c.Documento, doc =>
        {
            doc.Property(d => d.Valor)
               .HasColumnName("Documento")
               .IsRequired()
               .HasMaxLength(14);
        });

        builder.ComplexProperty(c => c.Email, email =>
        {
            email.Property(e => e.Valor)
               .HasColumnName("Email")
               .IsRequired()
               .HasMaxLength(150);
        });

        builder.ComplexProperty(c => c.Telefone, tel =>
        {
            tel.Property(t => t.Valor)
               .HasColumnName("Telefone")
               .IsRequired()
               .HasMaxLength(20);
        });

        builder.Property(c => c.CriadoEm)
               .HasColumnType("timestamp with time zone")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes & Unique Constraints
        builder.HasIndex(c => c.Nome);
    }
}
```

---

## 3. Indexing & Database Optimization

- **Unique Constraints**: Define unique indexes on natural keys (`Documento`, `Email`) to enforce database-level integrity.
- **Foreign Keys**: Always ensure foreign key columns have indexes defined.
- **Composite Indexes**: Use composite indexes when queries filter by multiple columns together (e.g. `Status` + `CriadoEm`).

```csharp
// Unique Indexes
builder.HasIndex("Documento").IsUnique();
builder.HasIndex("Email").IsUnique();

// Composite Index for Filtered Listing
builder.HasIndex(os => new { os.Status, os.CriadoEm });
```

---

## 4. Query Performance & Execution Rules

### 4.1 Read Operations (`AsNoTracking`)
Always append `.AsNoTracking()` to read-only queries. Tracking entities incurs change tracker overhead and memory waste.

```csharp
// GOOD: Read-Only Query
public async Task<List<ClienteDto>> GetActiveClientesAsync(CancellationToken cancellationToken)
{
    return await dbContext.Clientes
        .AsNoTracking()
        .Where(c => c.Nome != null)
        .OrderBy(c => c.Nome)
        .Select(c => ClienteMapper.ToDto(c))
        .ToListAsync(cancellationToken);
}
```

### 4.2 Avoiding N+1 Queries
- Never iterate over an entity collection and perform database calls inside the loop.
- Use explicit projection (`.Select()`) or eager loading (`.Include()` / `.ThenInclude()`).

```csharp
// GOOD: Single query with Projection
var osDtos = await dbContext.OrdensServico
    .AsNoTracking()
    .Where(os => os.ClienteId == clienteId)
    .Select(os => new OrdemServicoSummaryDto(
        os.Id,
        os.Status.ToString(),
        os.ValorTotal,
        os.Servicos.Count
    ))
    .ToListAsync(cancellationToken);
```

### 4.3 Deterministic Pagination
Always pair `.Skip()` and `.Take()` with an explicit `.OrderBy()` or `.OrderByDescending()` to prevent non-deterministic page results.

```csharp
// GOOD: Deterministic Pagination
var page = await dbContext.Clientes
    .AsNoTracking()
    .OrderBy(c => c.CriadoEm)
    .ThenBy(c => c.Id)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

---

## 5. Migrations & Deployment Rules

1. **Migration Command**: Always run migrations targeting the `AutoReparos.Infra` project with `AutoReparos.API` as the startup project:
   ```bash
   dotnet ef migrations add <MigrationName> --project AutoReparos.Infra --startup-project AutoReparos.API
   ```
2. **Review Generated Code**: Inspect the generated migration file before applying. Ensure indexes and column types match specifications.
3. **Production Deployment**: Do NOT use `context.Database.Migrate()` automatically in high-traffic production environments; generate idempotent SQL scripts using `dotnet ef migrations script`.

---

## 6. Transaction Management

Use explicit transactions (`IDbContextTransaction`) when coordinating operations across multiple aggregate boundaries or manual SQL operations.

```csharp
// GOOD: Explicit Transaction Strategy
public async Task ProcessarPagamentoAsync(Guid osId, CancellationToken cancellationToken)
{
    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        // 1. Update OS Status
        // 2. Adjust Stock / Insumos
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
```
