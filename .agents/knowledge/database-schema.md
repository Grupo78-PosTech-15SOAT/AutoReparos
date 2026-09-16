# AutoReparos - Database Schema & EF Core Specification

## 1. Overview

**AutoReparos** uses **PostgreSQL** as its relational database management system, accessed via **Entity Framework Core 9** with the `Npgsql.EntityFrameworkCore.PostgreSQL` provider.

Database configurations are defined using Fluent API mappings in `AutoReparos.Infra/Data/Mappings/` and registered automatically in [`AppDbContext.cs`](../../AutoReparos.Infra/Data/AppDbContext.cs).

---

## 2. Entity-Relationship Diagram (ERD)

```mermaid
erDiagram
    Clientes ||--o{ Veiculos : "1:N (Restrict)"
    Clientes ||--o{ OrdensServico : "1:N (Restrict)"
    Veiculos ||--o{ OrdensServico : "1:N (Restrict)"
    OrdensServico ||--|{ OrdensServicoServicos : "1:N (Cascade)"
    OrdensServico ||--|{ OrdensServicoInsumos : "1:N (Cascade)"
    Servicos ||--o{ OrdensServicoServicos : "1:N (Restrict)"
    Insumos ||--o{ OrdensServicoInsumos : "1:N (Restrict, Nullable)"
    AspNetUsers ||--o{ OrdensServico : "ResponsavelId (Soft Link)"

    Clientes {
        uuid Id PK
        varchar(100) Nome
        varchar(14) Documento UK
        integer TipoDocumento
        varchar(16) Telefone UK
        varchar(200) Email UK
        timestamp CriadoEm
        timestamp AtualizadoEm
    }

    Veiculos {
        uuid Id PK
        uuid ClienteId FK
        varchar(50) Marca
        varchar(50) Modelo
        integer AnoFabricacao
        integer AnoModelo
        varchar(7) Placa UK
        varchar(17) Chassi UK
        varchar(11) Renavam UK
        timestamp CriadoEm
        timestamp AtualizadoEm
    }

    Insumos {
        uuid Id PK
        varchar(100) Nome
        varchar(500) Descricao
        numeric(18,2) Valor
        integer QuantidadeEstoque
        timestamp CriadoEm
        timestamp AtualizadoEm
    }

    Servicos {
        uuid Id PK
        varchar(100) Nome
        varchar(500) Descricao
        numeric(18,2) ValorTabelado
        timestamp CriadoEm
        timestamp AtualizadoEm
    }

    OrdensServico {
        uuid Id PK
        uuid ClienteId FK
        uuid VeiculoId FK
        integer Status
        varchar(500) Observacao
        varchar(450) ResponsavelId
        timestamp CriadoEm
        timestamp IniciadoEm
        timestamp FinalizadoEm
        timestamp EntregueEm
        timestamp EnvioAprovacaoEm
    }

    OrdensServicoServicos {
        uuid Id PK
        uuid OrdemServicoId FK
        uuid ServicoId FK
        numeric(18,2) ValorCobrado
        integer Status
        timestamp IniciadoEm
        timestamp ConcluidoEm
    }

    OrdensServicoInsumos {
        uuid Id PK
        uuid OrdemServicoId FK
        uuid InsumoId FK
        varchar(200) Descricao
        numeric(18,2) ValorUnitario
        integer Quantidade
        integer Origem
    }

    AspNetUsers {
        uuid Id PK
        varchar(150) NomeCompleto
        integer Tipo
        varchar(256) Email
        varchar(256) UserName
        text PasswordHash
        timestamp CriadoEm
        timestamp AtualizadoEm
    }
```

---

## 3. Table Schema & EF Core Mapping Specifications

### 3.1. `Clientes`
- **Mapping Class**: [`ClienteMapping.cs`](../../AutoReparos.Infra/Data/Mappings/ClienteMapping.cs)
- **Primary Key**: `Id` (uuid)
- **Columns**:
  - `Nome`: `varchar(100)`, Required.
  - `Documento`: `varchar(14)`, Required. Owned property from `Documento.Valor`. Unique Index `IX_Clientes_Documento`.
  - `TipoDocumento`: `integer`, Required. Owned property from `Documento.Tipo`.
  - `Telefone`: `varchar(16)`, Required. Owned property from `Telefone.Numero`. Unique Index `IX_Clientes_Telefone`.
  - `Email`: `varchar(200)`, Required. Owned property from `Email.Endereco`. Unique Index `IX_Clientes_Email`.
  - `CriadoEm`: `timestamp with time zone`, Required.
  - `AtualizadoEm`: `timestamp with time zone`, Nullable.

### 3.2. `Veiculos`
- **Mapping Class**: [`VeiculoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/VeiculoMapping.cs)
- **Primary Key**: `Id` (uuid)
- **Columns**:
  - `ClienteId`: `uuid`, Required. Foreign Key to `Clientes(Id)` with `OnDelete(DeleteBehavior.Restrict)`.
  - `Marca`: `varchar(50)`, Required.
  - `Modelo`: `varchar(50)`, Required.
  - `AnoFabricacao`: `integer`, Required.
  - `AnoModelo`: `integer`, Required.
  - `Placa`: `varchar(7)`, Required. Unique Index `IX_Veiculos_Placa`.
  - `Chassi`: `varchar(17)`, Required. Unique Index `IX_Veiculos_Chassi`.
  - `Renavam`: `varchar(11)`, Required. Unique Index `IX_Veiculos_Renavam`.
  - `CriadoEm`: `timestamp with time zone`, Required.
  - `AtualizadoEm`: `timestamp with time zone`, Nullable.

### 3.3. `Insumos`
- **Mapping Class**: [`InsumoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/InsumoMapping.cs)
- **Primary Key**: `Id` (uuid)
- **Columns**:
  - `Nome`: `varchar(100)`, Required.
  - `Descricao`: `varchar(500)`, Nullable.
  - `Valor`: `numeric(18,2)`, Required.
  - `QuantidadeEstoque`: `integer`, Required.
  - `CriadoEm`: `timestamp with time zone`, Required.
  - `AtualizadoEm`: `timestamp with time zone`, Nullable.

### 3.4. `Servicos`
- **Mapping Class**: [`ServicoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/ServicoMapping.cs)
- **Primary Key**: `Id` (uuid)
- **Columns**:
  - `Nome`: `varchar(100)`, Required.
  - `Descricao`: `varchar(500)`, Nullable.
  - `ValorTabelado`: `numeric(18,2)`, Nullable.
  - `CriadoEm`: `timestamp with time zone`, Required.
  - `AtualizadoEm`: `timestamp with time zone`, Nullable.

### 3.5. `OrdensServico`
- **Mapping Class**: [`OrdemServicoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/OrdemServicoMapping.cs)
- **Primary Key**: `Id` (uuid, `ValueGeneratedNever()`)
- **Columns**:
  - `ClienteId`: `uuid`, Required. Foreign Key to `Clientes(Id)` (`DeleteBehavior.Restrict`).
  - `VeiculoId`: `uuid`, Required. Foreign Key to `Veiculos(Id)` (`DeleteBehavior.Restrict`).
  - `Status`: `integer`, Required (`EStatusOrdemServico` enum).
  - `Observacao`: `varchar(500)`, Nullable.
  - `ResponsavelId`: `varchar(450)`, Nullable. Mechanic User Id.
  - `CriadoEm`: `timestamp with time zone`, Required.
  - `IniciadoEm`: `timestamp with time zone`, Nullable.
  - `FinalizadoEm`: `timestamp with time zone`, Nullable.
  - `EntregueEm`: `timestamp with time zone`, Nullable.
  - `EnvioAprovacaoEm`: `timestamp with time zone`, Nullable.
- **Collection Field Access**: Navigation for `Servicos` and `Insumos` configured to use private backing fields (`_servicos` and `_insumos`) with `DeleteBehavior.Cascade`.

### 3.6. `OrdensServicoServicos`
- **Mapping Class**: [`OrdemServicoServicoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/OrdemServicoServicoMapping.cs)
- **Primary Key**: `Id` (uuid, `ValueGeneratedNever()`)
- **Columns**:
  - `OrdemServicoId`: `uuid`, Required. Foreign Key to `OrdensServico(Id)` (`DeleteBehavior.Cascade`).
  - `ServicoId`: `uuid`, Required. Foreign Key to `Servicos(Id)` (`DeleteBehavior.Restrict`).
  - `ValorCobrado`: `numeric(18,2)`, Required.
  - `Status`: `integer`, Required (`EStatusServicoOS` enum).
  - `IniciadoEm`: `timestamp with time zone`, Nullable.
  - `ConcluidoEm`: `timestamp with time zone`, Nullable.
- **Ignored Property**: `TempoExecucao` (computed getter property in domain entity).

### 3.7. `OrdensServicoInsumos`
- **Mapping Class**: [`OrdemServicoInsumoMapping.cs`](../../AutoReparos.Infra/Data/Mappings/OrdemServicoInsumoMapping.cs)
- **Primary Key**: `Id` (uuid, `ValueGeneratedNever()`)
- **Columns**:
  - `OrdemServicoId`: `uuid`, Required. Foreign Key to `OrdensServico(Id)` (`DeleteBehavior.Cascade`).
  - `InsumoId`: `uuid`, Nullable. Foreign Key to `Insumos(Id)` (`DeleteBehavior.Restrict`). Nullable for custom/manual parts.
  - `Descricao`: `varchar(200)`, Required.
  - `ValorUnitario`: `numeric(18,2)`, Required.
  - `Quantidade`: `integer`, Required.
  - `Origem`: `integer`, Required (`EOrigemInsumo`: `Estoque = 1`, `Avulso = 2`).
- **Ignored Property**: `ValorTotal` (computed getter `ValorUnitario * Quantidade`).

### 3.8. `AspNetUsers` (Identity Table)
- **Mapping Class**: [`UsuarioIdentityMapping.cs`](../../AutoReparos.Infra/Data/Mappings/UsuarioIdentityMapping.cs)
- Extends standard ASP.NET Core Identity table with `NomeCompleto` (varchar 150), `Tipo` (integer enum), `CriadoEm`, and `AtualizadoEm`.

---

## 4. Migration History

EF Core migrations are located in [`AutoReparos.Infra/Data/Migrations/`](../../AutoReparos.Infra/Data/Migrations):

1. **`20260415013615_v1_clientes`**: Initial schema for `Clientes` and Owned Value Objects (`Documento`, `Telefone`, `Email`).
2. **`20260416011923_v2_veiculos`**: Created `Veiculos` table and FK relationship to `Clientes`.
3. **`20260421054143_v3_servicos_ordens_pecas`**: Added `Servicos`, `OrdensServico`, `OrdensServicoServicos`, and `OrdensServicoPecas` tables.
4. **`20260423165912_v4_usuarios`**: Added Identity Core tables for user management.
5. **`20260429235300_v5_renomeando_peca_para_insumo`**: Renamed `Peca` domain concepts to `Insumo` (`OrdensServicoInsumos`).
6. **`20260502011032_v6_desacoplando_usuario_identity`**: Decoupled Domain `Usuario` entity from Infrastructure `UsuarioIdentity` model.
7. **`20260727175525_v7_responsavel_e_envio_aprovacao_os`**: Added `ResponsavelId` and `EnvioAprovacaoEm` columns to `OrdensServico`.

---

## 5. Seed Data Pipeline (`DbInitializer`)

The [`DbInitializer.cs`](../../AutoReparos.Infra/Data/DbInitializer.cs) handles database migration execution and seed data populating on startup:

- **Always Executed**: Default Admin User creation based on `SeedUsuarioSettings` (`admin@autoreparos.com`).
- **Development / Testing / Staging Environments**:
  - Sample **Clientes**: Leandro Tavares, Fernanda Oliveira Costa, Roberto Alves Pereira.
  - Sample **Veiculos**: Volkswagen Gol (`ABC1D23`), Chevrolet Onix (`XYZ2E34`), Fiat Strada (`DEF3F45`).
  - Sample **Insumos**: Óleo Motor 5W30, Filtro de Óleo, Pastilha de Freio.
  - Sample **Servicos**: Troca de Óleo, Revisão de Freios, Alinhamento.
  - Sample **OrdensServico**: Initial work orders seeded in `EmExecucao`, `AguardandoAprovacao`, and `Recebida` states for testing Kanban workflows.
