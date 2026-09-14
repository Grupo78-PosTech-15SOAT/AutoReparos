# AutoReparos - REST API Contracts & Endpoint Specification

## 1. Overview

The **AutoReparos API** is built as a RESTful web service using ASP.NET Core 9 Minimal APIs with Controller delegation.

- **Base URL**: `/api`
- **Content Type**: `application/json`
- **Authentication**: JWT Bearer token via `Authorization: Bearer <token>` header for protected routes. Public routes are marked with `AllowAnonymous()`.

---

## 2. Authentication & Authorization

### 2.1. Authentication Flow
- **`POST /api/auth/login`**
  - **Auth**: Anonymous (`AllowAnonymous()`)
  - **Request Body**:
    ```json
    {
      "email": "admin@autoreparos.com",
      "password": "AdminPassword123!"
    }
    ```
  - **Response `200 OK`**:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiration": "2026-08-11T20:00:00Z",
      "usuario": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "nomeCompleto": "Administrador do Sistema",
        "email": "admin@autoreparos.com",
        "tipo": 1
      }
    }
    ```
  - **Response `401 Unauthorized`**: Invalid credentials.

### 2.2. Public Quote Approval via HMAC Token
- **`GET /api/ordem-servico/aprovar?token={token}`**
  - **Auth**: Anonymous
  - **Query Parameter**: `token` (Base64Url HMAC token signed by server).
  - **Response `200 OK`**: Content `"Orçamento aprovado"`.
- **`GET /api/ordem-servico/recusar?token={token}`**
  - **Auth**: Anonymous
  - **Query Parameter**: `token`.
  - **Response `200 OK`**: Content `"Orçamento recusado"`.

---

## 3. API Endpoints Inventory

### 3.1. Customers (`/api/clientes`) — Require Authorization

| HTTP Method | Route | Description | Request DTO | Response DTO / Code |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/clientes` | Create new customer | `ClienteCreateDto` | `201 Created` (`ClienteDto`) |
| `GET` | `/api/clientes` | Search/list customers paginated | `ClientePagedRequest` | `200 OK` (`PagedResult<ClienteDto>`) |
| `GET` | `/api/clientes/{id}` | Get customer by ID | - | `200 OK` (`ClienteDto`) / `404` |
| `PUT` | `/api/clientes/{id}` | Update customer details | `ClienteUpdateDto` | `204 No Content` / `404` / `400` |
| `DELETE` | `/api/clientes/{id}` | Delete customer | - | `204 No Content` / `404` |

#### DTO Examples:
- **`ClienteCreateDto`**:
  ```json
  {
    "nome": "Leandro Tavares",
    "documento": "97632180044",
    "telefone": "27999111111",
    "email": "leandro.silva@email.com"
  }
  ```
- **`ClienteDto`**:
  ```json
  {
    "id": "a3b4c5d6-e7f8-4901-8234-56789abcdef0",
    "nome": "Leandro Tavares",
    "documento": "97632180044",
    "tipoDocumento": "CPF",
    "telefone": "27999111111",
    "email": "leandro.silva@email.com",
    "criadoEm": "2026-08-10T20:00:00Z"
  }
  ```

---

### 3.2. Vehicles (`/api/veiculos`) — Require Authorization

| HTTP Method | Route | Description | Request DTO | Response DTO / Code |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/veiculos` | Register new vehicle | `VeiculoCreateDto` | `201 Created` (`VeiculoDto`) |
| `GET` | `/api/veiculos` | List vehicles paginated | `VeiculoPagedRequest` | `200 OK` (`PagedResult<VeiculoDto>`) |
| `GET` | `/api/veiculos/{id}` | Get vehicle by ID | - | `200 OK` (`VeiculoDto`) / `404` |
| `GET` | `/api/veiculos/placa/{placa}` | Get vehicle by license plate | - | `200 OK` (`VeiculoDto`) / `404` |
| `PUT` | `/api/veiculos/{id}` | Update vehicle | `VeiculoUpdateDto` | `204 No Content` / `404` |
| `DELETE` | `/api/veiculos/{id}` | Delete vehicle | - | `204 No Content` / `404` |

---

### 3.3. Services (`/api/servicos`) — Require Authorization

| HTTP Method | Route | Description | Request DTO | Response DTO / Code |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/servicos` | Create catalog service | `ServicoCreateDto` | `201 Created` (`ServicoDto`) |
| `GET` | `/api/servicos` | List catalog services | `ServicoPagedRequest` | `200 OK` (`PagedResult<ServicoDto>`) |
| `GET` | `/api/servicos/{id}` | Get service by ID | - | `200 OK` (`ServicoDto`) / `404` |
| `GET` | `/api/servicos/tempo-medio` | Get average completion time for all services | - | `200 OK` (`IEnumerable<TempoMedioServicoDto>`) |
| `GET` | `/api/servicos/{id}/tempo-medio` | Get average execution time for specific service | - | `200 OK` (`TempoMedioServicoDto`) |
| `PUT` | `/api/servicos/{id}` | Update service | `ServicoUpdateDto` | `204 No Content` |
| `DELETE` | `/api/servicos/{id}` | Delete service | - | `204 No Content` |

---

### 3.4. Inventory & Parts (`/api/insumos`) — Require Authorization

| HTTP Method | Route | Description | Request DTO | Response DTO / Code |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/insumos` | Create new inventory part | `CriarInsumoDto` | `201 Created` (`InsumoDto`) |
| `GET` | `/api/insumos` | List inventory parts | `InsumoPagedRequest` | `200 OK` (`PagedResult<InsumoDto>`) |
| `GET` | `/api/insumos/{id}` | Get part by ID | - | `200 OK` (`InsumoDto`) |
| `PUT` | `/api/insumos/{id}` | Update inventory part | `AtualizarInsumoDto` | `204 No Content` |
| `POST` | `/api/insumos/{id}/adicionar-estoque` | Add stock quantity | `AtualizarEstoqueDto` | `204 No Content` |
| `POST` | `/api/insumos/{id}/remover-estoque` | Deduct stock quantity | `AtualizarEstoqueDto` | `204 No Content` |
| `DELETE` | `/api/insumos/{id}` | Delete inventory part | - | `204 No Content` |

---

### 3.5. Work Orders (`/api/ordem-servico`)

| HTTP Method | Route | Auth | Description | Response / Body |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/ordem-servico` | Required | List all work orders paginated | `200 OK` (`PagedResult<OrdemServicoDto>`) |
| `GET` | `/api/ordem-servico/kanban` | Required | Kanban column group list | `200 OK` (`IEnumerable<KanbanColumnDto>`) |
| `GET` | `/api/ordem-servico/fila` | Required | Queue ordered by priority (excludes Finalized/Delivered) | `200 OK` (`PagedResult<OrdemServicoDto>`) |
| `GET` | `/api/ordem-servico/consulta` | **Public** | Public search by document or license plate | `200 OK` (`PagedResult<OrdemServicoPublicoDto>`) |
| `GET` | `/api/ordem-servico/consulta/{id}` | **Public** | Public detailed status by OS ID | `200 OK` (`OrdemServicoPublicoDetalheDto`) |
| `POST` | `/api/ordem-servico` | Required | Create new Work Order | `201 Created` (`CriarOrdemServicoDto` -> `OrdemServicoDto`) |
| `GET` | `/api/ordem-servico/{id}` | Required | Get detailed OS by ID | `200 OK` (`OrdemServicoDetalheDto`) |
| `POST` | `/api/ordem-servico/{id}/servicos` | Required | Add service item to OS | `204 No Content` (`AdicionarServicoDto`) |
| `POST` | `/api/ordem-servico/{id}/insumos` | Required | Add input/part item to OS | `204 No Content` (`AdicionarInsumoDto`) |
| `PATCH` | `/api/ordem-servico/{id}/iniciar-diagnostico` | Required | Transition OS to `EmDiagnostico` | `204 No Content` |
| `PATCH` | `/api/ordem-servico/{id}/enviar-para-aprovacao` | Required | Transition OS to `AguardandoAprovacao` & send email | `204 No Content` |
| `GET` | `/api/ordem-servico/aprovar` | **Public** | Approve quote via token parameter | `200 OK` (`"Orçamento aprovado"`) |
| `GET` | `/api/ordem-servico/recusar` | **Public** | Reject quote via token parameter | `200 OK` (`"Orçamento recusado"`) |
| `PATCH` | `/api/ordem-servico/{id}/servicos/{servicoId}/iniciar` | Required | Start execution of service item | `204 No Content` |
| `PATCH` | `/api/ordem-servico/{id}/servicos/{servicoId}/concluir` | Required | Complete service item execution | `204 No Content` |
| `PATCH` | `/api/ordem-servico/{id}/entregar` | Required | Mark vehicle delivered to customer | `204 No Content` |

---

### 3.6. Users & Management (`/api/usuarios`) — Require Authorization

| HTTP Method | Route | Description | Request DTO | Response Code |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/usuarios` | Create new system user | `UsuarioCreateDto` | `201 Created` (`UsuarioDto`) |
| `GET` | `/api/usuarios` | List system users | `UsuarioPagedRequest` | `200 OK` (`PagedResult<UsuarioDto>`) |
| `GET` | `/api/usuarios/{id}` | Get user by ID | - | `200 OK` (`UsuarioDto`) |
| `PUT` | `/api/usuarios/{id}` | Update user | `UsuarioUpdateDto` | `204 No Content` |
| `DELETE` | `/api/usuarios/{id}` | Delete user | - | `204 No Content` |

---

### 3.7. Dashboard (`/api/dashboard`) — Require Authorization

| HTTP Method | Route | Description | Response DTO |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/dashboard/metrics` | Returns total counts per OS status, revenue, and active counts | `200 OK` (`DashboardMetricsDto`) |

---

## 4. Problem Details Error Response Contract (RFC 7807)

When an error occurs, the server returns a standard RFC 7807 `ProblemDetails` payload:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "A Ordem de Serviço deve ter pelo menos um serviço para aguardar aprovação.",
  "instance": "/api/ordem-servico/a3b4c5d6-e7f8-4901-8234-56789abcdef0/enviar-para-aprovacao"
}
```
