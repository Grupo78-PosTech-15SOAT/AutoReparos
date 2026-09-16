# AutoReparos - Vertical Slice Feature Implementation Specification

## 1. Overview

This document defines the strict specification for implementing new domain features in **AutoReparos**.

Even though the solution is divided into Clean Architecture project assemblies (`Domain`, `Application`, `Infra`, `API`, `Web`), feature code is organized into **Vertical Slices / Feature Modules** inside each layer. When a new feature (e.g., `Fornecedor`) is added, code must be implemented slice-by-slice across all 5 projects.

```mermaid
flowchart TB
    subgraph Step 1: Domain
        D1[Entities] --> D2[Value Objects]
        D2 --> D3[Exceptions]
        D3 --> D4[Repository Interface]
    end

    subgraph Step 2: Application
        A1[DTOs & Mappers] --> A2[Use Case Interfaces]
        A2 --> A3[Use Case Implementations]
        A3 --> A4[DI Registration]
    end

    subgraph Step 3: Infrastructure
        I1[EF Mapping Configuration] --> I2[DbSet in AppDbContext]
        I2 --> I3[Repository Implementation]
        I3 --> I4[Migration & DI Registration]
    end

    subgraph Step 4: API
        P1[Controller Class] --> P2[Endpoint Mapping Group]
        P2 --> P3[API DI & Program.cs Map]
    end

    subgraph Step 5: Web (Frontend)
        W1[TypeScript Model] --> W2[Angular Service]
        W3[Angular Pages/Components] --> W4[app.routes.ts]
    end

    Step 1 --> Step 2 --> Step 3 --> Step 4 --> Step 5
```

---

## 2. Layer-by-Layer Implementation Checklist

### Step 1: Domain Layer (`AutoReparos.Domain`)
Location: `AutoReparos.Domain/<FeatureName>/`

- [ ] **Entities**: Create Rich Entity extending `Entity` ([Entity.cs](../../AutoReparos.Domain/Shared/Entity.cs)). Mark setter properties private, validate in constructors and state methods.
- [ ] **Value Objects** (if required): Implement validation in `Create()` static factory methods.
- [ ] **Exceptions**: Inherit from `DomainException` ([DomainException.cs](../../AutoReparos.Domain/Shared/Exceptions/DomainException.cs)).
- [ ] **Repository Interface**: Define `I<FeatureName>Repository.cs` with async methods (`GetById`, `GetAll`, `Create`, `Update`, `Delete`).

### Step 2: Application Layer (`AutoReparos.Application`)
Location: `AutoReparos.Application/<FeatureName>/`

- [ ] **DTOs**: Create Request/Response DTOs (`<Feature>CreateDto`, `<Feature>UpdateDto`, `<Feature>Dto`, `<Feature>PagedRequest`).
- [ ] **Mappers**: Create static or extension Mapper class (`<Feature>Mapper.cs`).
- [ ] **Use Cases**: Create single-responsibility Use Case interfaces and implementations (`ICriar<Feature>UseCase`, `Criar<Feature>UseCase`).
- [ ] **DI Registration**: Add registrations in [`DependencyInjection.cs`](../../AutoReparos.Application/DependencyInjection.cs).

### Step 3: Infrastructure Layer (`AutoReparos.Infra`)
Location: `AutoReparos.Infra/`

- [ ] **EF Mapping**: Implement `IEntityTypeConfiguration<T>` in `Data/Mappings/<Feature>Mapping.cs`.
- [ ] **DbContext**: Add `DbSet<T>` to [`AppDbContext.cs`](../../AutoReparos.Infra/Data/AppDbContext.cs).
- [ ] **Repository Implementation**: Implement `I<Feature>Repository` in `Repositories/<Feature>Repository.cs`.
- [ ] **DI Registration**: Register Repository in [`DependencyInjectionInfra.cs`](../../AutoReparos.Infra/IoC/DependencyInjectionInfra.cs).
- [ ] **Migration**: Run EF Core migration `dotnet ef migrations add vX_<feature_name> --project AutoReparos.Infra --startup-project AutoReparos.API`.

### Step 4: API Layer (`AutoReparos.API`)
Location: `AutoReparos.API/`

- [ ] **Controller**: Create scoped controller class in `Controllers/<Feature>Controller.cs` returning `IResult` (`Results.Ok`, `Results.CreatedAtRoute`, `Results.NoContent`).
- [ ] **Endpoint Mapping**: Create endpoint extension class in `Endpoints/<Feature>Endpoint.cs` (`Map<Feature>Endpoints(this WebApplication app)`).
- [ ] **DI Registration**: Register Controller in [`DependencyInjectionAPI.cs`](../../AutoReparos.API/DependencyInjectionAPI.cs).
- [ ] **Program.cs**: Add `app.Map<Feature>Endpoints()` to [`Program.cs`](../../AutoReparos.API/Program.cs).

### Step 5: Web Layer (`AutoReparos.Web`)
Location: `AutoReparos.Web/src/app/features/<feature-name>/`

- [ ] **Models**: Define TypeScript interfaces in `models/<feature>.model.ts`.
- [ ] **Service**: Implement Angular `@Injectable()` service in `services/<feature>.service.ts` using `HttpClient`.
- [ ] **Pages/Components**: Create Standalone Angular page components for list, detail, and form views.
- [ ] **Routing**: Add route definitions to [`app.routes.ts`](../../AutoReparos.Web/src/app/app.routes.ts) protected by `authGuard`.

---

## 3. Concrete Implementation Walkthrough: Adding "Fornecedor"

Below is a complete, standardized reference for adding a new `Fornecedor` (Supplier) feature across all layers.

### Step 1: Domain Implementation

#### Entity ([`AutoReparos.Domain/Fornecedores/Entities/Fornecedor.cs`](../../AutoReparos.Domain/Fornecedores/Entities/Fornecedor.cs))
```csharp
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Fornecedores.Exceptions;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.Fornecedores.Entities
{
    public class Fornecedor : Entity
    {
        public string RazaoSocial { get; private set; } = null!;
        public Documento Cnpj { get; private set; } = null!;
        public Telefone Telefone { get; private set; } = null!;
        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        protected Fornecedor() { }

        public Fornecedor(string razaoSocial, string cnpj, string telefone) : base()
        {
            if (string.IsNullOrWhiteSpace(razaoSocial))
                throw new InvalidFornecedorException("Razão Social é obrigatória.");

            RazaoSocial = razaoSocial;
            Cnpj = Documento.Create(cnpj);
            Telefone = Telefone.Create(telefone);
            CriadoEm = DateTime.UtcNow;
        }
    }
}
```

#### Repository Interface ([`AutoReparos.Domain/Fornecedores/Repositories/IFornecedorRepository.cs`](../../AutoReparos.Domain/Fornecedores/Repositories/IFornecedorRepository.cs))
```csharp
using AutoReparos.Domain.Fornecedores.Entities;

namespace AutoReparos.Domain.Fornecedores.Repositories
{
    public interface IFornecedorRepository
    {
        Task<Fornecedor?> GetById(Guid id);
        Task<(IEnumerable<Fornecedor> Items, int TotalCount)> GetAll(string? busca, int skip, int take);
        Task Create(Fornecedor fornecedor);
        Task Update(Fornecedor fornecedor);
        Task Delete(Guid id);
    }
}
```

---

### Step 2: Application Implementation

#### DTO & Use Case ([`AutoReparos.Application/Fornecedores/UseCases/CriarFornecedorUseCase.cs`](../../AutoReparos.Application/Fornecedores/UseCases/CriarFornecedorUseCase.cs))
```csharp
namespace AutoReparos.Application.Fornecedores.UseCases
{
    public class CriarFornecedorUseCase(IFornecedorRepository repository) : ICriarFornecedorUseCase
    {
        public async Task<FornecedorDto> ExecuteAsync(FornecedorCreateDto dto)
        {
            var fornecedor = new Fornecedor(dto.RazaoSocial, dto.Cnpj, dto.Telefone);
            await repository.Create(fornecedor);
            return fornecedor.ToDto();
        }
    }
}
```

---

### Step 3: Infrastructure Implementation

#### Mapping ([`AutoReparos.Infra/Data/Mappings/FornecedorMapping.cs`](../../AutoReparos.Infra/Data/Mappings/FornecedorMapping.cs))
```csharp
public class FornecedorMapping : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.RazaoSocial).IsRequired().HasMaxLength(150);

        builder.OwnsOne(f => f.Cnpj, cnpj =>
        {
            cnpj.Property(c => c.Valor).HasColumnName("Cnpj").IsRequired().HasMaxLength(14);
            cnpj.HasIndex(c => c.Valor).IsUnique().HasDatabaseName("IX_Fornecedores_Cnpj");
        });
    }
}
```

---

### Step 4: API Endpoint & Controller

#### Controller ([`AutoReparos.API/Controllers/FornecedorController.cs`](../../AutoReparos.API/Controllers/FornecedorController.cs))
```csharp
namespace AutoReparos.API.Controllers
{
    public class FornecedorController(ICriarFornecedorUseCase criarFornecedorUseCase)
    {
        public async Task<IResult> Create(FornecedorCreateDto dto)
        {
            var result = await criarFornecedorUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetFornecedorById", new { id = result.Id }, result);
        }
    }
}
```

#### Endpoint Group ([`AutoReparos.API/Endpoints/FornecedorEndpoint.cs`](../../AutoReparos.API/Endpoints/FornecedorEndpoint.cs))
```csharp
namespace AutoReparos.API.Endpoints
{
    public static class FornecedorEndpoint
    {
        public static void MapFornecedoresEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/fornecedores").WithTags("Fornecedores").RequireAuthorization();
            group.MapPost("/", (FornecedorCreateDto dto, FornecedorController controller) => controller.Create(dto))
                .WithName("CreateFornecedor")
                .Produces<FornecedorDto>(StatusCodes.Status201Created);
        }
    }
}
```

---

### Step 5: Web (Angular Frontend) Implementation

#### Service ([`AutoReparos.Web/src/app/features/fornecedores/services/fornecedor.service.ts`](../../AutoReparos.Web/src/app/features/fornecedores/services/fornecedor.service.ts))
```typescript
@Injectable({ providedIn: 'root' })
export class FornecedorService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/fornecedores`;

  criar(dto: FornecedorCreateDto): Observable<Fornecedor> {
    return this.http.post<Fornecedor>(this.apiUrl, dto);
  }
}
```

---

## 4. Verification & Testing

Every vertical slice feature must include unit tests and integration tests before deployment:

1. **Domain Unit Tests**: Test constructors, Value Object validations, and invalid state transitions in `AutoReparos.Domain.Tests`.
2. **Application Unit Tests**: Test Use Case execution flow and repository mock calls in `AutoReparos.Application.Tests`.
3. **API Integration Tests**: Test HTTP status codes, payload serialization, and database persistence in `AutoReparos.IntegrationTests`.
