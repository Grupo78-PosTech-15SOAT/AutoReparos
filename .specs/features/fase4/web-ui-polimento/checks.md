# Polimento de UI do Web checks

Mutations: 3
Build: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build`
Plan: `.specs/features/fase4/web-ui-polimento/plan.md`

28 checks in 6 slices · 0 one-way doors · 0 open, of which 0 block

## Checks

### S1 - Fundação compartilhada

**C1** - `app-id-badge` mostra `#` mais os 8 primeiros caracteres, com o id completo no `title`, e um id de 6 caracteres sai inteiro (AC 1)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/id-badge/id-badge.component.spec.ts --filter "IdBadge corte"`

**C2** - Clicar em copiar grava o id completo na área de transferência e mostra "Copiado" por 1,5 s (AC 2)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/id-badge/id-badge.component.spec.ts --filter "IdBadge copia"`

**C3** - `formatarDataHora` relativo: 30 s → "agora", 5 min → "há 5 min", 3 h → "há 3 h", 2 dias no mesmo ano → "dd/MM HH:mm", outro ano → "dd/MM/yyyy HH:mm"; vazio ou inválido → "—" (AC 3, AC 4)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/utils/data-hora.utils.spec.ts --filter "formatarDataHora"`

**C4** - `moeda`: 1234.5 → "R$ 1.234,50", 0 → "R$ 0,00", null → "—" (AC 5)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/pipes/moeda.pipe.spec.ts --filter "MoedaPipe"`

**C5** - `getStatusOsInfo` devolve rótulo pt-BR e classe diferente do fallback para os 10 estados de Saga e os 5 de Execução, e `STATUS_OS` continua com 7 (AC 6)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/utils/status-os.utils.spec.ts --filter "estados extras"`

**C6** - Com `buscaSempre`, a busca aparece com 2 opções e acha por rótulo, descrição e palavra-chave sem caixa nem acento ("abc1d23", "gol", "joao") (AC 7)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/custom-select/custom-select.component.spec.ts --filter "CustomSelect busca"`

**C7** - Ao abrir, o painel fica em `document.body`; clique fora do gatilho e do painel fecha; clique na busca não fecha; destruir aberto remove o painel do `body` (AC 8, AC 9)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/custom-select/custom-select.component.spec.ts --filter "CustomSelect painel"`

**C8** - `styles.css` define `.btn-ghost`, `.btn-sm`, `.btn-icon`, `.filter-bar .form-group { margin-bottom: 0 }` e `--accent-green/red/cyan/yellow`, e `.card-panel` não tem `backdrop-filter` (AC 10)
Proof: `python3 scripts/specs/assert_web_ui.py --check estilos`

**C9** - `OsConhecidasService` devolve a união deduplicada de atalho local, Sagas (Administrador) e fila, com placa, modelo, marca e cliente; 403 da fila e 404 de OS são ignorados; falha de veículo ou cliente deixa os campos vazios (AC 11, AC 12, AC 13)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/services/os-conhecidas.service.spec.ts --filter "OsConhecidas agregacao"`

**C10** - Duas OS com o mesmo veículo geram um único `GET` do veículo (AC 14)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/services/os-conhecidas.service.spec.ts --filter "OsConhecidas cache"`

### S2 - Console de Sagas

**C11** - O filtro "Parada há" oferece "Qualquer tempo", "15 min", "1 h", "6 h" e "24 h", o Estado tem rótulos pt-BR, e escolher "1 h" + "Atualizar" pede `paradaHaMinutos=60` (AC 15, AC 16)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage filtros"`

**C12** - Cada linha mostra o id curto da OS, o rótulo do estado e a hora relativa (AC 17)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage tabela"`

**C13** - "Detalhes" abre o drawer com as abas Etapas, Comandos e Registros (Etapas primeiro); a aba Registros lista `em` formatado, evento e descrição; fechar esconde o drawer e mantém a tabela (AC 18, AC 19, AC 20)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage drawer"`

**C14** - O bloco de retentativa aparece só em `AguardandoInsumos` (AC 21)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage retentativa"`

### S3 - Fila de Execução

**C15** - O card mostra placa e modelo quando o veículo resolve e o id curto da OS quando falha (AC 22, AC 23)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucao veiculo"`

**C16** - Ações por estado: Fila (etapas, falha, sem concluir), EmExecucao (etapas, concluir, falha), Falhou (só reexecutar para Administrador), Concluida e Cancelada (nenhuma) (AC 24)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucao acoes por estado"`

**C17** - "Reportar falha" mostra o motivo e "Confirmar falha", desabilitado com motivo vazio; cada etapa mostra tipo, descrição, responsável e `registradaEm` formatado (AC 25, AC 26)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucao falha e etapas"`

### S4 - Orçamento e Pagamento

**C18** - O seletor lista as OS conhecidas com código, placa e modelo no rótulo e cliente e estado na descrição, `Recebida` primeiro, e filtra por placa sem máscara e por modelo (AC 27, AC 28)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage seletor OS"`

**C19** - UUID colado fora da lista oferece "Usar OS #xxxxxxxx", que carrega e seleciona a OS; 404 mostra "OS não encontrada para o código informado." (AC 29, AC 30)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage UUID colado"`

**C20** - Escolher a OS preenche o cliente, mostra nome, placa e modelo e envia esse `clienteId`; `ordemServicoId` na rota ou na query pré-seleciona a OS; valores saem em `moeda` (AC 31, AC 32, AC 33)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage cliente da OS"`

**C21** - O botão do checkout diz "Abrir checkout" sem a URL no texto e o Pagamento mostra valores em `moeda` (AC 33, AC 34)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/pagamento/pages/pagamento-page/pagamento-page.component.spec.ts --filter "PagamentoPage checkout"`

### S5 - Navegação, Kanban, Detalhe e Home

**C22** - O navbar tem um único item "Ordens de Serviço" para `/ordens-servico`, nenhum "Fila Kanban", e o item fica ativo em `/ordens-servico/<id>` (AC 35, AC 36)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/navbar/navbar.component.spec.ts --filter "Navbar ordens de servico"`

**C23** - O Kanban tem o título "Ordens de Serviço" e cada card mostra placa e modelo e o link "Detalhes" para `/ordens-servico/<id>` (AC 37)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.spec.ts --filter "OsKanban cards"`

**C24** - O Detalhe da OS mostra o link "Ordens de Serviço" e o estado da execução com rótulo pt-BR (AC 38)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.spec.ts --filter "OsDetalhe navegacao"`

**C25** - A Home não tem "Kanban" nos links e aponta os links de OS para `/ordens-servico` (AC 39)
Proof: `python3 scripts/specs/assert_web_ui.py --check home`

### S6 - Consistência das demais telas

**C26** - Clientes, Veículos e Usuários usam botões de ícone com `aria-label` "Editar <nome>"/"Excluir <nome>" sem `btn-danger` sólido na linha; nenhum componente de feature redefine `.btn-sm`, `.form-control`, `.form-group`, `.form-label` ou `.card-panel` nem usa `number:'1.2-2'`; Aprovação usa os botões globais e id-badge no `pagamentoId`; a Nova OS mostra a placa nas opções de veículo (AC 40, AC 41, AC 42, AC 43)
Proof: `python3 scripts/specs/assert_web_ui.py --check consistencia`

**C27** - Os arquivos novos e alterados da feature têm pelo menos 80% de linhas cobertas e o build passa (AC 1 a AC 43, regra de cobertura da D-70)
Proof: `python3 scripts/specs/assert_web_ui.py --check cobertura`

**C28** - As opções de veículo da Nova OS trazem a placa formatada no rótulo (`ABC-1D23 · modelo (marca)`) (AC 43; acrescentado pelo achado do Verifier sobre C26)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.spec.ts --filter "OsNovaPage opcoes de veiculo"`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| formatos relativos, AC 3 (5 exemplos) | agora C3 · min C3 · h C3 · mesmo ano C3 · outro ano C3 | - |
| entradas inválidas de data, AC 4 (2) | vazio C3 · inválido C3 | - |
| moeda, AC 5 (3 exemplos) | 1234.5 C4 · 0 C4 · null C4 | - |
| estados extras, AC 6 (15) | OrcamentoAprovado C5 · ReservandoInsumos C5 · AguardandoInsumos C5 · ExecucaoLiberada C5 · ExecucaoConcluida C5 · IntervencaoRequerida C5 · PagamentoCapturado C5 · Compensando C5 · Compensada C5 · CompensacaoFalhou C5 · Fila C5 · EmExecucao C5 · Concluida C5 · Falhou C5 · Cancelada C5 | - |
| campos da busca, AC 7 (3) | rótulo C6 · descrição C6 · palavra-chave C6 | - |
| fontes de OS conhecidas, AC 11 (3) | atalho C9 · sagas C9 · fila C9 | - |
| falhas toleradas, AC 12-13 (4) | fila 403 C9 · OS 404 C9 · veículo C9 · cliente C9 | - |
| opções de parada, AC 15 (5) | Qualquer tempo C11 · 15 min C11 · 1 h C11 · 6 h C11 · 24 h C11 | - |
| abas do drawer, AC 18 (3) | Etapas C13 · Comandos C13 · Registros C13 | - |
| estados da execução, AC 24 (5) | Fila C16 · EmExecucao C16 · Falhou C16 · Concluida C16 · Cancelada C16 | - |
| telas com ações na linha, AC 40 (3) | Clientes C26 · Veículos C26 · Usuários C26 | - |
| startup config: `LOCALE_ID` pt-BR (1 assembly) | `app.config.ts` C27 (build) | - |

## Swept

- validation: C17 (motivo vazio desabilita), C19 (UUID colado)
- failure modes: C9, C15, C19
- idempotency: n/a - só leituras e as mesmas ações de antes
- authorization: existing - `roleGuard` nas rotas e `AuthService.hasRole` para Sagas da OS e "Reexecutar" (C9, C16)
- concurrency: C10 (requisição de veículo compartilhada)
- data lifecycle: existing - `OsJornadaStore` só com ids e limpo no logout (D-71)
- dependency failure: C9, C15
- state transitions: C16
- observability: n/a - telemetria do Web inalterada

## Handoff

- **Settled mid-build:** none
- **Nota de processo:** `check_commit.py` exige `type(scope):`; os commits seguem a AD-002 (`<gitmoji> <Descrição>`), que prevalece
- **Dispatched:** Batch Worker A (S2, S3, S5: Sagas, Fila, Kanban, Navbar, Home, Detalhe) e Batch Worker B (S4, S6: Orçamento, Pagamento, Aprovação, cadastros, catálogo, Nova OS) a partir do Web `8d4a114` (S1), sem commit; ambos `returned` (A: 57 testes; B: 52 testes e 3 specs novos de Clientes, Veículos e Usuários); Verifier independente no pai `5e7454f` / Web `cae9e60`
- **Boundary:** C1-C27 fechados no Web `cae9e60` (483 testes, cobertura agregada 97,19%, build ok); achados do Verifier fechados no Web `0857905` e no pai `9132e76` (486 testes, C28 acrescentado, 29 provas e 3/3 mutações mortas)
- **Gate:** `validate_verification.py` recusa só porque a árvore do pai está suja (ponteiros de submódulo: Web nesta feature, OS, Cobrança, Execução e Cadastros de outras sessões); o ponteiro do Web espera o push e a decisão do José
- **Abandoned:** none
- **Ajustes durante o build:** Usuários ganhou edição de nome e perfil (reuso de `UsuarioService.atualizar`) porque o AC 40 pede "Editar <nome>"; `assert_web_ui.py` passou a tratar como redefinição só a regra cujo seletor é a própria classe (seletor descendente não conta)
- **Walked:** feito pelo José em produção em 17/11 ("Já conferi está certo"): conferiu Sagas (drawer), Fila, Orçamento (seletor aberto sobre os catálogos), Kanban e um cadastro
