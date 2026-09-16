# ADR-002 — Isolamento de Dados e Política Zero-Trust no Portal do Cliente

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica
> **Fase:** Tech Challenge FIAP SOAT — Fase 3
> **Status:** Aceito e implementado
> **Data da decisão:** Sprint 2 da Fase 3
> **Documentos relacionados:** [RFC-003](./RFC-003-serverless-client-authentication.md), [ADR-003](./ADR-003-end-to-end-observability-strategy.md), [Diagrama de sequência](./diagrams/sequence_portal_auth_and_query.md)

---

## Contexto

Com a autenticação serverless da [RFC-003](./RFC-003-serverless-client-authentication.md),
o portal passa a expor duas rotas de leitura ao cliente final:

- `GET /api/clientes/meus-veiculos`
- `GET /api/ordem-servico/minhas-os` (com alias `GET /api/ordens-servico/minhas-os`)

Essas rotas rodam no **mesmo processo ASP.NET Core** que serve as rotas operacionais da
oficina e consultam **as mesmas tabelas** (`Veiculos`, `OrdensServico`). O risco é
direto e clássico: se o identificador do titular vier da requisição — querystring, rota
ou corpo — qualquer cliente autenticado troca um GUID e lê o histórico de outro. É a
falha nº 1 do OWASP API Security (*Broken Object Level Authorization*).

O agravante específico deste domínio é que a **placa** é um identificador público de
fato: fica visível no para-brisa. Um filtro por placa não pode ser tratado como
informação secreta.

## Decisão

**A identidade do titular dos dados é extraída exclusivamente das claims do JWT
validado. Nenhum identificador de titular é aceito por parâmetro de requisição.**

Três mecanismos concretos implementam isso.

### 1. `ClienteId` vem da claim, nunca da requisição

`AutoReparos.API/Controllers/PortalClienteController.cs` é o único ponto de entrada das
rotas do portal, e lê o titular do `ClaimsPrincipal`:

```csharp
private static Guid? ObterClienteId(ClaimsPrincipal user)
{
    var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

    return Guid.TryParse(idStr, out var id) ? id : null;
}
```

Claim ausente ou malformada resulta em `401 Unauthorized` antes de qualquer acesso a
dados. As assinaturas dos use cases (`ExecuteAsync(Guid clienteId, ...)`) recebem esse
GUID já resolvido — **não existe sobrecarga que aceite um `clienteId` vindo do
transporte**, de modo que o isolamento não depende de um desenvolvedor futuro lembrar
de aplicá-lo.

### 2. Segregação por política de autorização

Declaradas em `AutoReparos.API/DependencyInjectionAPI.cs`:

| Política | Roles aceitas | Rotas |
|---|---|---|
| `OperadorOficina` | `Administrador`, `Atendente`, `Mecanico` | Todo o CRUD operacional, Kanban, fila, dashboard, usuários, insumos, serviços, veículos |
| `ClientePolicy` | `Cliente` | Apenas `meus-veiculos` e `minhas-os` |

Os dois conjuntos de roles são **disjuntos**. Um token emitido pela Lambda carrega
`Role: Cliente` e falha a `OperadorOficina` com `403 Forbidden`; um token de operador
não satisfaz a `ClientePolicy`. A separação é declarativa no mapeamento dos endpoints
(`RequireAuthorization(...)`), não espalhada em `if`s dentro dos handlers.

### 3. Zero Data Leakage no filtro por placa

Este é o ponto sutil. `ObterMinhasOrdensServicoUseCase` aceita uma placa opcional para
filtrar as OS. Ao receber uma placa que **não pertence** ao cliente autenticado, ele
**retorna lista vazia com `HTTP 200`** — não `403`, não `404`:

```csharp
var veiculo = await veiculoRepository.GetByPlaca(placa.Trim());

// Regra Zero-Trust: se o veículo não existe ou não pertence a este cliente,
// retorna lista vazia imediatamente
if (veiculo == null || veiculo.ClienteId != clienteId)
{
    return Enumerable.Empty<MinhaOrdemServicoDto>();
}
```

A escolha é deliberada. Um `403` para "placa de terceiro" e um `404` para "placa
inexistente" seriam **respostas distintas**, e essa distinção é, por si só, um
vazamento: bastaria varrer placas para descobrir quais veículos a oficina atende e quem
tem serviço em aberto. Ao colapsar os dois casos na mesma resposta vazia, a API não
confirma nem nega a existência do registro.

A consulta principal também é filtrada na origem — `ordemServicoRepository.GetAll` é
sempre chamado com `clienteId` — de modo que o isolamento acontece **na cláusula
`WHERE`**, não por filtragem do resultado em memória.

---

## Consequências

### Positivas

- Manipular parâmetros da requisição não muda o titular dos dados retornados: o único
  caminho para ler dados de outro cliente é forjar um JWT, o que exige o segredo de
  assinatura.
- Nenhuma resposta da API distingue "não é seu" de "não existe", eliminando enumeração
  por placa.
- O isolamento fica concentrado em dois arquivos auditáveis
  (`PortalClienteController` e o use case), em vez de espalhado por controllers.
- As políticas disjuntas dão à banca uma demonstração direta: o mesmo endpoint
  operacional responde `200` para o token de operador e `403` para o token de cliente.

### Negativas e riscos aceitos

- **`ValidateIssuer = false` e `ValidateAudience = false`** na configuração do
  `JwtBearer`. Qualquer token assinado com o segredo compartilhado é aceito,
  independentemente de quem o emitiu. Enquanto Lambda e backend compartilham o mesmo
  segredo HMAC (RFC-003, seção 5), a validação de emissor não acrescentaria garantia
  real — mas ela **passa a ser necessária** no momento em que um terceiro serviço passar
  a assinar com a mesma chave. Deve ser endereçada junto com a migração para RS256.
- **Lista vazia é ambígua para o usuário legítimo.** Um cliente que digita a própria
  placa com um erro de digitação recebe a mesma resposta vazia de quem tentou espiar
  dados alheios, sem nenhuma pista de diagnóstico. É o preço da não-enumeração;
  mitiga-se na UI, oferecendo a lista de placas do próprio cliente em vez de um campo
  livre.
- **`take: 100` fixo** na consulta de OS do cliente. Suficiente para o histórico real de
  um cliente de oficina, mas é um limite silencioso: um cliente com mais de 100 ordens
  não veria as mais antigas. Deve virar paginação explícita antes de um uso produtivo.
- A verificação de titularidade da placa custa **uma consulta adicional** a `Veiculos`
  antes da consulta principal. O custo é aceitável e o índice único
  `IX_Veiculos_Placa` cobre o acesso.

---

## Verificação

O comportamento é coberto pela suíte automatizada do `AutoReparos.App`.

**Testes de integração** (`AutoReparos.IntegrationTests/Features/PortalCliente/PortalClienteIntegrationTests.cs`,
HTTP real contra PostgreSQL em Testcontainers):

| Cenário | Esperado | Teste |
|---|---|---|
| Token de cliente em `meus-veiculos` | `200` com apenas os veículos do titular | `GetMeusVeiculos_ComTokenCliente_DeveRetornarApenasVeiculosDoCliente` |
| Token de cliente em `minhas-os` | `200` com apenas as OS do titular | `GetMinhasOrdensServico_ComTokenCliente_DeveRetornarApenasOrdensDoCliente` |
| Filtro por placa de terceiro | `200` com `[]` | `GetMinhasOrdensServico_FiltrandoPlacaDeOutroCliente_DeveRetornarListaVazia` |
| Token de cliente em rota operacional | `403 Forbidden` | `RotasOperacionais_ComTokenCliente_DeveRetornar403Forbidden` |
| Requisição sem token nas rotas do portal | `401 Unauthorized` | `EndpointsPortal_SemToken_DeveRetornar401Unauthorized` |

**Testes de unidade do use case** (`ObterMinhasOrdensServicoUseCaseTests`,
`ObterMeusVeiculosUseCaseTests`): `clienteId` vazio, placa inexistente, placa de outro
cliente, placa própria e consulta sem filtro — confirmando que a resposta para *placa
inexistente* e *placa de terceiro* é a mesma lista vazia.

**Lacuna conhecida:** não há teste automatizado para o caso de token válido com claim de
identificador ausente ou malformada. O caminho existe em `ObterClienteId` e retorna
`401`, mas hoje só é verificado por inspeção de código.
