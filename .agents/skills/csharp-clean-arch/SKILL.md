---
name: csharp-clean-arch
description: Guide and code generator for C# Clean Architecture and DDD components in AutoReparos (.NET 10), including Use Cases (Vertical Slice / CQRS), Domain Entities, Value Objects, Domain Exceptions, Repository interfaces and EF Core implementations, and xUnit + NSubstitute + FluentAssertions unit tests.
---

# C# Clean Architecture & DDD Skill Guide

This skill provides step-by-step procedures and code generators for building domain-driven, clean architecture components in the AutoReparos (.NET 10) backend solution.

---

## 1. Architecture Overview & Layer Responsibilities

AutoReparos follows **Clean Architecture** with **Domain-Driven Design (DDD)** and **CQRS / Vertical Slice Use Cases**:

```
AutoReparos.slnx
├── AutoReparos.Domain              # Pure Domain (Entities, Value Objects, Enums, Exceptions, Repository Interfaces)
├── AutoReparos.Application         # Use Cases, DTOs, Mappers, Application Interfaces, DI Registration
├── AutoReparos.Infra               # Infrastructure (AppDbContext, EF Mappings, Repositories, Identity, External Services)
├── AutoReparos.API                 # Presentation (Minimal API Endpoints / Controllers, Middlewares)
├── AutoReparos.Domain.Tests        # Unit tests for Domain Entities & Value Objects (xUnit)
├── AutoReparos.Application.Tests   # Unit tests for Use Cases (xUnit + NSubstitute + FluentAssertions)
└── AutoReparos.IntegrationTests    # WebApplicationFactory integration tests
```

### Key Rules
- **Domain Layer (`AutoReparos.Domain`)**: Must remain free of external dependencies or infrastructure frameworks. All entities inherit from `Entity` ([`Entity.cs`](../../../AutoReparos.Domain/Shared/Entity.cs)). Value Objects are defined as `sealed record`.
- **Application Layer (`AutoReparos.Application`)**: Implements primary constructor use cases (`public class MyUseCase(IRepository repository) : IMyUseCase`). Request/Response DTOs and static mappers isolate external contracts from domain models.
- **Infrastructure Layer (`AutoReparos.Infra`)**: Implements domain repository interfaces using EF Core 10 (`AppDbContext`).
- **Testing (`AutoReparos.Application.Tests` & `AutoReparos.Domain.Tests`)**: Tests use **xUnit**, **NSubstitute** for mocking dependencies, and **FluentAssertions** for assertions.

---

## 2. Step-by-Step Procedure: Creating a New Feature / Use Case

Follow these exact steps to introduce a new entity and vertical slice use case into AutoReparos:

### Step 1: Create Domain Components (`AutoReparos.Domain`)
1. Create entity class under `AutoReparos.Domain/<Feature>/Entities/<EntityName>.cs`.
   - Inherit from `Entity`.
   - Use private setters for properties.
   - Provide a `protected <EntityName>() { }` parameterless constructor for EF Core.
   - Enforce domain validation inside public constructors and mutation methods, throwing domain-specific exceptions.
2. Create domain exceptions under `AutoReparos.Domain/<Feature>/Exceptions/Invalid<EntityName>Exception.cs` (inheriting from `DomainException`).
3. Create Repository Interface under `AutoReparos.Domain/<Feature>/Repositories/I<EntityName>Repository.cs`.

### Step 2: Create Application Layer Use Case (`AutoReparos.Application`)
1. Define Request/Response DTOs under `AutoReparos.Application/<Feature>/DTOs/Request/` and `Response/`.
2. Define static mapper under `AutoReparos.Application/<Feature>/Mappers/<EntityName>Mapper.cs`.
3. Create Use Case Interface under `AutoReparos.Application/<Feature>/UseCases/Interfaces/I<UseCaseName>UseCase.cs`.
4. Create Use Case Implementation under `AutoReparos.Application/<Feature>/UseCases/<UseCaseName>UseCase.cs` using primary constructor injection.

### Step 3: Create Infrastructure Repository (`AutoReparos.Infra`)
1. Define EF Core mapping configuration in `AutoReparos.Infra/Data/Mappings/<EntityName>Mapping.cs` (`IEntityTypeConfiguration<T>`).
2. Add `DbSet<T>` property to `AppDbContext.cs`.
3. Implement `I<EntityName>Repository` in `AutoReparos.Infra/Repositories/<EntityName>Repository.cs`.

### Step 4: Register Dependency Injection
1. Register Use Case in `AutoReparos.Application/DependencyInjection.cs`:
   `services.AddScoped<I<UseCaseName>UseCase, <UseCaseName>UseCase>();`
2. Register Repository in `AutoReparos.Infra/IoC/DependencyInjectionInfra.cs`:
   `services.AddScoped<I<EntityName>Repository, <EntityName>Repository>();`

### Step 5: Write Unit Tests (`AutoReparos.Application.Tests`)
1. Create test file `AutoReparos.Application.Tests/<Feature>/<UseCaseName>UseCaseTests.cs`.
2. Use `NSubstitute` to mock `I<EntityName>Repository`.
3. Write test methods using `[Fact(DisplayName = "...")]` and `FluentAssertions`.

---

## 3. Code Templates

### Template A: Domain Entity (`<EntityName>.cs`)
```csharp
using AutoReparos.Domain.<Feature>.Exceptions;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.<Feature>.Entities
{
    public class <EntityName> : Entity
    {
        public string Nome { get; private set; } = null!;
        public string Descricao { get; private set; } = null!;
        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        protected <EntityName>() { }

        public <EntityName>(string nome, string descricao) : base()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new Invalid<EntityName>Exception("Nome é obrigatório.");

            Nome = nome;
            Descricao = descricao;
            CriadoEm = DateTime.UtcNow;
        }

        public void Atualizar(string nome, string descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new Invalid<EntityName>Exception("Nome é obrigatório.");

            Nome = nome;
            Descricao = descricao;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
```

### Template B: Value Object (`<ValueObject>.cs`)
```csharp
using AutoReparos.Domain.<Feature>.Exceptions;

namespace AutoReparos.Domain.<Feature>.ValueObjects
{
    public sealed record <ValueObject>
    {
        public string Valor { get; }

        private <ValueObject>(string valor)
        {
            Valor = valor;
        }

        public static <ValueObject> Create(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new Invalid<EntityName>Exception("Valor é obrigatório.");

            return new <ValueObject>(valor.Trim());
        }

        public override string ToString() => Valor;
    }
}
```

### Template C: Repository Interface (`I<EntityName>Repository.cs`)
```csharp
using AutoReparos.Domain.<Feature>.Entities;

namespace AutoReparos.Domain.<Feature>.Repositories
{
    public interface I<EntityName>Repository
    {
        Task Create(<EntityName> entity);
        Task<<EntityName>?> GetById(Guid id);
        Task<(IEnumerable<<EntityName>> Items, int Total)> GetAll(string? filter, int skip, int take);
        Task Update(<EntityName> entity);
        Task Delete(<EntityName> entity);
    }
}
```

### Template D: Application DTO & Mapper
```csharp
// DTO: <EntityName>Dto.cs
namespace AutoReparos.Application.<Feature>.DTOs.Response
{
    public record <EntityName>Dto(Guid Id, string Nome, string Descricao, DateTime CriadoEm);
}

// DTO: Criar<EntityName>Dto.cs
namespace AutoReparos.Application.<Feature>.DTOs.Request
{
    public record Criar<EntityName>Dto(string Nome, string Descricao);
}

// Mapper: <EntityName>Mapper.cs
using AutoReparos.Application.<Feature>.DTOs.Response;
using AutoReparos.Domain.<Feature>.Entities;

namespace AutoReparos.Application.<Feature>.Mappers
{
    public static class <EntityName>Mapper
    {
        public static <EntityName>Dto ToDto(<EntityName> entity)
        {
            return new <EntityName>Dto(entity.Id, entity.Nome, entity.Descricao, entity.CriadoEm);
        }
    }
}
```

### Template E: Use Case (`Criar<EntityName>UseCase.cs`)
```csharp
using AutoReparos.Application.<Feature>.DTOs.Request;
using AutoReparos.Application.<Feature>.DTOs.Response;
using AutoReparos.Application.<Feature>.Mappers;
using AutoReparos.Application.<Feature>.UseCases.Interfaces;
using AutoReparos.Domain.<Feature>.Entities;
using AutoReparos.Domain.<Feature>.Repositories;

namespace AutoReparos.Application.<Feature>.UseCases
{
    public class Criar<EntityName>UseCase(I<EntityName>Repository repository) : ICriar<EntityName>UseCase
    {
        public async Task<<EntityName>Dto> ExecuteAsync(Criar<EntityName>Dto dto)
        {
            var entity = new <EntityName>(dto.Nome, dto.Descricao);
            await repository.Create(entity);
            return <EntityName>Mapper.ToDto(entity);
        }
    }
}
```

### Template F: Unit Test (`Criar<EntityName>UseCaseTests.cs`)
```csharp
using AutoReparos.Application.<Feature>.DTOs.Request;
using AutoReparos.Application.<Feature>.UseCases;
using AutoReparos.Domain.<Feature>.Entities;
using AutoReparos.Domain.<Feature>.Exceptions;
using AutoReparos.Domain.<Feature>.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AutoReparos.Application.Tests.<Feature>
{
    public class Criar<EntityName>UseCaseTests
    {
        private readonly I<EntityName>Repository _repository;
        private readonly Criar<EntityName>UseCase _useCase;

        public Criar<EntityName>UseCaseTests()
        {
            _repository = Substitute.For<I<EntityName>Repository>();
            _useCase = new Criar<EntityName>UseCase(_repository);
        }

        [Fact(DisplayName = "Create With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new Criar<EntityName>Dto("Teste", "Descrição teste");

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            await _repository.Received(1).Create(Arg.Any<<EntityName>>());
        }

        [Fact(DisplayName = "Create With Invalid Data Should Throw Exception")]
        public async Task Create_WithEmptyName_ShouldThrowException()
        {
            var dto = new Criar<EntityName>Dto("", "Descrição");

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<Invalid<EntityName>Exception>();
            await _repository.DidNotReceive().Create(Arg.Any<<EntityName>>());
        }
    }
}
```

---

## 4. Verification & Testing

Execute solution builds and test suites using bash:

```bash
# Build complete solution
dotnet build AutoReparos.slnx

# Run Domain Unit Tests
dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj

# Run Application Unit Tests
dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj
```
