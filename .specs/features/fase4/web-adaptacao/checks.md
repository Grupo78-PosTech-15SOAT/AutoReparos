# Adaptação do Web às APIs dos serviços - checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx -m:1`; `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx -m:1`; `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web build`
Plan: `.specs/features/fase4/web-adaptacao/plan.md`

31 checks · 7 slices · 2 one-way doors · 0 open questions, 0 blocking

Provas do Web: `W <spec>` abrevia `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include <spec>` (Angular `unit-test`, vitest); provas de infraestrutura usam `python scripts/specs/assert_web_adaptacao.py --check <nome>` (script novo, escrito na construção como `assert_web_extraction.py`).

## Checks

### S1 - Camada HTTP e roteamento

**C1** - Todo endpoint de `api-endpoints.ts` começa em `API_URL` + `/api/v1` (ou `/api/auth/login`) e nenhum aponta para URL de serviço (AC 1)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/config/api-endpoints.spec.ts --filter "ApiEndpoints"`

**C2** - O Nginx da imagem do Web encaminha cada um dos 5 prefixos ao serviço da sua variável, devolve 404 para prefixo desconhecido e `GET /` responde 200 (AC 2)
Proof: `python scripts/specs/assert_web_adaptacao.py --check proxy`

**C3** - O compose do pai sobe OS, Cobrança, Execução, Cadastros e Web, e uma chamada autenticada a `/api/v1/fila-execucao` pelo Web responde 200 (AC 3)
Proof: `python scripts/specs/assert_web_adaptacao.py --check compose`

**C4** - O `errorInterceptor` mostra a mensagem do catálogo para cada `{codigo}` conhecido nos status 401, 403, 404, 409, 422 e 503, e a mensagem genérica por status para código desconhecido (AC 4)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/error.interceptor.spec.ts --filter "ErrorInterceptor"`

**C5** - O `jwtInterceptor` envia o token só às rotas da Surface, o login usa `POST /api/auth/login` e um 401 leva ao login (AC 5)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/jwt.interceptor.spec.ts --filter "JwtInterceptor"`

### S2 - Leituras novas no backend

**C6** - `GET /ordens-servico` pagina, ordena por `atualizadaEm` decrescente, filtra por status e recusa filtro inválido com 422 `filtro_invalido` (AC 6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ListaOsPaginadaEFiltrada`

**C7** - `GET /ordens-servico/{id}/consulta` é público, devolve status e histórico sem `clienteId` nem `veiculoId` e responde 404 para id desconhecido (AC 7)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ConsultaPublicaNaoExpoeClienteNemVeiculo`

**C8** - `GET /orcamentos?ordemServicoId=` devolve o orçamento com os itens e o snapshot de preço e responde 404 para OS sem orçamento (AC 8)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~ConsultaOrcamentoPorOs`

**C9** - `GET /pagamentos/{id}` devolve status, valor e `checkoutUrl` quando houver, 404 para id desconhecido, e exige papel de operador (Surface)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~ConsultaPagamentoPorId`

**C10** - `POST /orcamentos/{id}/link-aprovacao` gera token de pelo menos 128 bits, guarda só o hash com validade de 7 dias, devolve o token uma única vez e um novo pedido invalida o anterior (AC 9)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~LinkDeAprovacaoGuardaSoOHash`

**C11** - `GET /orcamentos/publico?token=` devolve o resumo do orçamento e responde 404 `token_invalido` para token desconhecido, expirado ou malformado (Surface, AC 11)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~ResumoPublicoPorToken`

**C12** - `POST /orcamentos/publico/aprovacao` aprova o orçamento e grava `orcamento.aprovado.v1` no Outbox na mesma transação, e o token deixa de valer (AC 10)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~AprovacaoPublicaGravaOutboxEInvalidaToken`

**C13** - Duas aprovações simultâneas com o mesmo token aprovam o orçamento uma só vez: uma responde 200 e a outra 409 `orcamento_nao_pendente` (AC 10)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~AprovacaoPublicaConcorrenteAprovaUmaVez`

**C14** - O token nunca aparece no log nem na resposta de erro das rotas públicas, e "inexistente" e "expirado" são indistinguíveis (AC 11)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TokenNaoVazaNoLogNemNoErro`

### S3 - Ordens de serviço e Kanban

**C15** - "Nova OS" chama `POST /ordens-servico` com cliente e veículo, abre o detalhe da OS criada e mostra o erro 422 no formulário (AC 12)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.spec.ts --filter "OsNovaPage"`

**C16** - O Kanban mostra uma coluna para cada um dos 7 status da OS, alimentada por `GET /ordens-servico?status=`, com estado vazio e carregando (AC 13)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.spec.ts --filter "OsKanbanPage"`

**C17** - O detalhe da OS mostra status, histórico, orçamento e estado da execução (AC 14)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.spec.ts --filter "OsDetalhePage"`

**C18** - "Cancelar OS" pede motivo, chama `POST .../cancelamento`, mostra o estado devolvido e, em 409 `saga_em_andamento` ou `cancelamento_indisponivel`, mostra a mensagem do código e mantém a OS (AC 15)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/components/os-cancelamento/os-cancelamento.component.spec.ts --filter "OsCancelamento"`

**C19** - A consulta pública busca pelo código da OS, mostra status e histórico e não oferece busca por placa nem CPF (AC 16)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/consulta-publica-page/consulta-publica-page.component.spec.ts --filter "ConsultaPublicaPage"`

### S4 - Orçamento e pagamento

**C20** - A tela de orçamento lista serviços e insumos do catálogo com preço e saldo, soma o total e envia `POST /orcamentos` com os itens, mostrando o valor devolvido (AC 17)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage"`

**C21** - "Gerar link de aprovação" chama o endpoint e mostra o link para copiar, uma única vez, com a validade (AC 18)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/components/link-aprovacao/link-aprovacao.component.spec.ts --filter "LinkAprovacao"`

**C22** - A página do link mostra o resumo, "Aprovar" chama a aprovação pública e confirma, e token inválido mostra "Link inválido ou expirado" (AC 19)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.spec.ts --filter "AprovacaoOrcamentoPage"`

**C23** - A tela de pagamento mostra o pagamento, "Iniciar checkout" abre a URL devolvida e o administrador solicita reembolso com valor e referência (AC 20)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/pagamento/pages/pagamento-page/pagamento-page.component.spec.ts --filter "PagamentoPage"`

### S5 - Execução e catálogo

**C24** - A fila de execução filtra por status e mecânico e executa registrar etapa (2 tipos), concluir, marcar falha e reexecutar, mostrando `execucao_estado_invalido` em 409 (AC 21)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucaoPage"`

**C25** - Serviços e insumos usam só `GET/POST /servicos`, `GET/POST /insumos` e `POST /insumos/{id}/entradas`, mostram saldo disponível e reservado e não oferecem editar nem excluir (AC 22)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/insumos/pages/insumos-page/insumos-page.component.spec.ts --filter "InsumosPage"`
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/servicos/pages/servicos-page/servicos-page.component.spec.ts --filter "ServicosPage"`

### S6 - Console de Sagas

**C26** - O console lista Sagas com filtro de estado e de parada, abre o detalhe com etapas, comandos e registros e, em `AguardandoInsumos`, "Retentar reserva" exige motivo e chama o endpoint (AC 23)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage lista e retenta"`

**C27** - A retentativa com 409 `saga_em_andamento` ou `retentativa_indisponivel` mostra a mensagem do código e mantém a Saga (AC 24)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage conflito"`

**C28** - A rota e o menu do console não existem para quem não é Administrador (AC 25)
Proof: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/auth/role.guard.spec.ts --filter "RoleGuard"`

### S7 - Limpeza e qualidade

**C29** - O Web não contém rota, menu, serviço nem chamada para os 5 itens removidos: dashboard, entregar, status manual, itens soltos da OS e busca por placa/CPF (AC 26)
Proof: `python scripts/specs/assert_web_adaptacao.py --check legado`

**C30** - Os testes do Web passam com pelo menos 80% de cobertura de linhas no código novo ou alterado e `yarn build` sai com 0 (AC 27)
Proof: `python scripts/specs/assert_web_adaptacao.py --check cobertura`

**C31** - A suíte do Web passa em Node 25 ou 26 sem flag extra (AC 28)
Proof: `python scripts/specs/assert_web_adaptacao.py --check node`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| prefixos de roteamento (5) | `/ordens-servico`+`/admin/sagas` C2 · `/orcamentos`+`/pagamentos` C2 · `/execucoes`+`/fila-execucao`+`/servicos`+`/insumos` C2 · `/clientes`+`/veiculos`+`/usuarios`+`/api/auth` C2 · desconhecido 404 C2 | - |
| status da OS no Kanban (7) | `Recebida` C16 · `OrcamentoAprovado` C16 · `EmExecucao` C16 · `Concluida` C16 · `Paga` C16 · `IntervencaoRequerida` C16 · `Cancelada` C16 (table-driven, 7) | - |
| status HTTP tratados pelo interceptor (6) | 401 C4 · 403 C4 · 404 C4 · 409 C4 · 422 C4 · 503 C4 | - |
| códigos de erro do catálogo (7) | `saga_em_andamento` C4 · `orcamento_nao_pendente` C4 · `token_invalido` C4 · `filtro_invalido` C4 · `execucao_estado_invalido` C4 · `cancelamento_indisponivel` C4 · `retentativa_indisponivel` C4 | - |
| `GET /ordens-servico` statuses (4) | 200 C6 · 401 C6 · 403 C6 · 422 C6 | - |
| `GET /ordens-servico/{id}/consulta` statuses (2) | 200 C7 · 404 C7 | - |
| `GET /orcamentos?ordemServicoId=` statuses (5) | 200 C8 · 401 C8 · 403 C8 · 404 C8 · 422 C8 | - |
| `GET /pagamentos/{id}` statuses (4) | 200 C9 · 401 C9 · 403 C9 · 404 C9 | - |
| `POST /orcamentos/{id}/link-aprovacao` statuses (5) | 201 C10 · 401 C10 · 403 C10 · 404 C10 · 409 C10 | - |
| `GET /orcamentos/publico` statuses (2) | 200 C11 · 404 C11 | - |
| `POST /orcamentos/publico/aprovacao` statuses (3) | 200 C12 · 404 C12 · 409 C13 | - |
| regra do token (4 exemplos) | expirado C11 · repetido C12 · reaproveitado após novo link C10 · malformado C14 | - |
| paginação (4 exemplos) | `tamanho=10&pagina=3` com 25 OS C6 · `tamanho=100` C6 · `tamanho=101` C6 · `status` inexistente C6 | - |
| itens removidos do legado (5) | dashboard C29 · entregar C29 · status manual C29 · itens soltos C29 · busca por placa/CPF C29 | - |
| controles das telas novas (15) | Nova OS C15 · coluna do Kanban C16 · Cancelar OS C18 · Buscar por código C19 · Enviar orçamento C20 · Gerar link C21 · Aprovar C22 · Iniciar checkout C23 · Reembolsar C23 · Registrar etapa C24 · Concluir C24 · Falha C24 · Reexecutar C24 · Registrar entrada C25 · Retentar reserva C26 | - |
| door 1: contrato de URL do navegador (1) | C1 · C2 · C3 | - |
| door 2: rotas públicas (2) | consulta do OS C7 · orçamento por token C11, C12, C13, C14 | - |
| startup config: `API_URL` (2 assemblies) | imagem Docker C2 · compose C3 | - |

## Swept

- validation: C6 (paginação e filtro), C10 (token), C20 (itens do orçamento)
- failure modes: C4 (503 e códigos), C18, C24, C27
- idempotency: C13 (aprovação simultânea com o mesmo token), C10 (novo link invalida o anterior)
- authorization: C6, C8, C9, C10 (papéis nas rotas novas), C28 (guard do console), C14 (rotas públicas sem vazar)
- concurrency: C13
- data lifecycle: C10 (validade de 7 dias e uso único), C12 (token deixa de valer)
- dependency failure: C4 (503 com `dependencia_indisponivel`), C3 (serviço fora do ar no compose)
- state transitions: C12 (`Pendente` para `Aprovado`), C10 (409 fora de `Pendente`), C16 (7 status), C26 (retentar só em `AguardandoInsumos`)
- observability: C14 (o token não vai ao log); n/a para o restante - nenhum requisito de log ou métrica novo

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** checks derivados em 2026-11-13, build não iniciado
- **Abandoned:** none
- **Walked:** none - telas sem captura de referência; a prova de cada tela é o teste do componente
