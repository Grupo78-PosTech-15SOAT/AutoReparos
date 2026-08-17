---
name: postgres-efcore
description: Guide and runner for EF Core 10 and PostgreSQL (Npgsql) migrations, DbContext entity mappings (IEntityTypeConfiguration), query optimization, indexing strategy, and database diagnostics in AutoReparos.
---

# PostgreSQL & EF Core 10 Skill Guide

This skill provides step-by-step procedures, migration workflows, entity mapping conventions, and performance optimization guidelines for PostgreSQL and Entity Framework Core 10 in `AutoReparos.Infra`.

---

## 1. Architecture Overview & Infrastructure Conventions

Database context and EF Core mapping rules are centered in the `AutoReparos.Infra` project:

```
AutoReparos.Infra/
├── Data/
│   ├── AppDbContext.cs               # Central EF Core DbContext
│   ├── DbInitializer.cs              # Seed data initializer
│   ├── Mappings/                     # IEntityTypeConfiguration<T> classes
│   │   ├── ClienteMapping.cs
│   │   ├── VeiculoMapping.cs
│   │   ├── ServicoMapping.cs
│   │   ├── InsumoMapping.cs
│   │   └── OrdemServicoMapping.cs
│   └── Migrations/                   # Auto-generated EF Core migrations
├── Repositories/                     # Repository implementations
└── IoC/
    └── DependencyInjectionInfra.cs   # Npgsql & DbContext registration
```

### Essential Mapping & PostgreSQL Rules
1. **Explicit Entity Configurations**: Every domain entity MUST have a corresponding `IEntityTypeConfiguration<T>` class inside `AutoReparos.Infra/Data/Mappings/`.
2. **Value Objects (`OwnsOne`)**: Mapped as owned entities with explicit database column names and indexes:
   ```csharp
   builder.OwnsOne(e => e.Documento, doc => {
       doc.Property(d => d.Valor).HasColumnName("Documento").IsRequired().HasMaxLength(14);
       doc.Property(d => d.Tipo).HasColumnName("TipoDocumento").IsRequired();
       doc.HasIndex(d => d.Valor).IsUnique().HasDatabaseName("IX_Clientes_Documento");
   });
   ```
3. **UTC Date Times**: PostgreSQL `timestamp with time zone` fields must store `DateTime.UtcNow`.
4. **Cascade Delete Prevention**: Foreign key relationships should be configured with `DeleteBehavior.Restrict` or `DeleteBehavior.ClientSetNull` to prevent accidental cascading deletes. Catch `DbUpdateException` in repositories and throw `RelatedEntityException`.
5. **Database Naming**: Specify database index names explicitly using `.HasDatabaseName("IX_<Table>_<Column>")`.

---

## 2. Step-by-Step Procedure: Creating and Applying Migrations

Follow these exact commands and steps whenever database schema changes occur:

### Step 1: Create Entity Mapping Class
Create or update `AutoReparos.Infra/Data/Mappings/<EntityName>Mapping.cs` implementing `IEntityTypeConfiguration<<EntityName>>`.

### Step 2: Register Mapping in `AppDbContext.cs`
Ensure `DbSet<<EntityName>>` is exposed on `AppDbContext` and `builder.ApplyConfiguration(new <EntityName>Mapping());` is called inside `OnModelCreating`.

### Step 3: Add EF Core Migration (CLI)
Run the migration command from bash:
```bash
dotnet ef migrations add <MigrationName> \
  --project AutoReparos.Infra/AutoReparos.Infra.csproj \
  --startup-project AutoReparos.API/AutoReparos.API.csproj
```

### Step 4: Verify Migration Code
Inspect the newly generated `.cs` file in `AutoReparos.Infra/Data/Migrations/` to verify table schema, primary keys, foreign keys, and indexes.

### Step 5: Apply Migration to PostgreSQL Database
Apply schema changes to the target PostgreSQL database:
```bash
dotnet ef database update \
  --project AutoReparos.Infra/AutoReparos.Infra.csproj \
  --startup-project AutoReparos.API/AutoReparos.API.csproj
```

### Step 6: Rollback / Revert Migration (If Needed)
If a migration needs to be removed before pushing:
```bash
# Rollback database to a previous migration
dotnet ef database update <PreviousMigrationName> \
  --project AutoReparos.Infra/AutoReparos.Infra.csproj \
  --startup-project AutoReparos.API/AutoReparos.API.csproj

# Remove the last migration snapshot
dotnet ef migrations remove \
  --project AutoReparos.Infra/AutoReparos.Infra.csproj \
  --startup-project AutoReparos.API/AutoReparos.API.csproj
```

---

## 3. Query Optimization & Indexing Guidelines

### Read-Only Queries (`AsNoTracking`)
Always append `.AsNoTracking()` to queries that fetch data purely for reading/display (e.g. `GetAll`, `GetById` for GET API endpoints):
```csharp
public async Task<<EntityName>?> GetByIdReadOnly(Guid id)
{
    return await _context.Set<<EntityName>>()
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id);
}
```

### Pagination and Sorting
Always perform pagination at the database query level using `Skip` and `Take` after applying filtering and sorting:
```csharp
public async Task<(IEnumerable<Cliente> Items, int Total)> GetAll(string? nome, int skip, int take)
{
    var query = _context.Clientes.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(nome))
    {
        query = query.Where(c => c.Nome.Contains(nome));
    }

    var total = await query.CountAsync();
    var items = await query
        .OrderBy(c => c.Nome)
        .Skip(skip)
        .Take(take)
        .ToListAsync();

    return (items, total);
}
```

### Avoiding N+1 Queries
Use explicit eager loading with `.Include(...)` / `.ThenInclude(...)` or projection with `.Select(...)` when querying aggregate roots with child collections.

### Indexing Strategy
- **Unique Indexes**: Required on columns used for uniqueness checks (e.g. `Documento`, `Email`, `Telefone`, `Placa`, `CPF`).
- **Composite Indexes**: Use composite indexes for frequent multi-column filters (e.g. `Status` + `CriadoEm`).

---

## 4. Code Template: Entity Mapping Configuration

```csharp
using AutoReparos.Domain.<Feature>.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class <EntityName>Mapping : IEntityTypeConfiguration<<EntityName>>
    {
        public void Configure(EntityTypeBuilder<<EntityName>> builder)
        {
            builder.ToTable("<EntityName>s");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.CriadoEm)
                .IsRequired();

            builder.Property(e => e.AtualizadoEm);

            // Example Value Object mapping
            builder.OwnsOne(e => e.Email, email =>
            {
                email.Property(m => m.Endereco)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(200);

                email.HasIndex(m => m.Endereco)
                    .IsUnique()
                    .HasDatabaseName("IX_<EntityName>s_Email");
            });
        }
    }
}
```

---

## 5. Verification Commands

Run builds and EF Core diagnostics via bash:

```bash
# Verify project build and EF Core compilation
dotnet build AutoReparos.Infra/AutoReparos.Infra.csproj

# List existing database migrations
dotnet ef migrations list \
  --project AutoReparos.Infra/AutoReparos.Infra.csproj \
  --startup-project AutoReparos.API/AutoReparos.API.csproj
```
