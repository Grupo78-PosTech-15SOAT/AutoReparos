# Validação independente — Coerência de Negócio, Identidade Visual e Integrações do Web (`fase4/web-coerencia-negocio`)

**Data:** 2026-11-16  
**Verificador:** Subagente independente `Verifier` (`author != verifier`)  
**Branch Web:** `feature/web-coerencia-negocio` (`submodules/AutoReparos.Web`)  
**Decisões de referência:** D-70 / AD-075, D-71 / AD-076, D-69 / AD-074, D-03 / AD-028  
**Veredito:** PASS (0 achados em aberto; 18/18 checks e critérios EARS atendidos; 3/3 mutações mortas; build e testes 100% verdes com 94.62% de cobertura agregada)

---

## 1. Auditoria Formal do Plano e Contrato

O plano [plan.md](plan.md) e o contrato de verificação [checks.md](checks.md) foram auditados e validados contra os requisitos arquiteturais e de negócio do projeto:

- **Aprovação do Plano:** Aprovado formalmente pelo usuário (`Approved: 2026-11-16 - user: "Aprovo o plano revisado da web-coerencia-negocio com a D-71"`).
- **One-Way Doors:**
  - **Door 1 (`OsJornadaStore`):** Persiste exclusivamente IDs (`osId`, `clienteId`, `veiculoId`, `orcamentoId`, `pagamentoId`, `atualizadoEm`) na chave `autoreparos_os_jornada_v1`, limitado aos 100 registros mais recentes e limpo automaticamente no logout (`AuthService.logout`). Não armazena dados sensíveis (CPF, e-mail, nomes ou valores) no navegador.
  - **Door 2 (Remoção de `/consulta-publica`):** Rota, componente e todos os links de menu/navegação foram removidos do Angular, eliminando o redirecionamento `401 -> /login` para rotas anônimas inexistentes no backend.
- **Estrutura dos 4 Pilares nas 5 Fatias (S1 a S5):**
  1. *Código coerente:* Angular 19 Standalone, `ChangeDetectionStrategy.OnPush`, Signals reativos, `DestroyRef` / `takeUntilDestroyed` e separação estrita em `core/`, `features/` e `shared/`.
  2. *Identidade visual padronizada:* Reutilização de `<app-page-container>`, `<app-modal-dialog>`, `<app-alert-banner>`, `<app-id-badge>`, `<app-status-badge>`, `<app-custom-select>`, tokens CSS centralizados de `styles.css` e eliminação de CSS duplicado de modais ou cores hexadecimais fixas (`#ED145B`, `#18181C`).
  3. *Integrações reais sem mock:* Payloads alinhados 1:1 aos 4 microsserviços (`OS`, `Cobranca`, `Execucao`, `Cadastros`) e à `AuthLambda`, incluindo mapeamento de `precoUnitario`, estoques de insumos, etapas com descrição obrigatória, paginação e 29 códigos de erro de domínio.
  4. *Permissões e qualidade:* Restrições visuais alinhadas às policies de backend (`Administrador` para catálogo, bloqueio de `Mecanico` em entradas de estoque, tratamento de `403` na fila como acesso restrito) e suíte com cobertura de linhas ≥ 80% (agregado de 94.62%).

---

## 2. Resumo de Execução dos Gates e Provas

Todos os comandos de validação, compilação, testes e conformidade executaram com sucesso:

| Gate / Verificação | Comando | Resultado | Evidência |
| --- | --- | --- | --- |
| Suíte unitária Web | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false` | PASS | 25 arquivos de teste, 364 testes executados, 364 aprovados (0 falhas) |
| Build de produção Web | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build` | PASS | Bundle de produção gerado em `dist/app-temp`, 0 erros |
| Validador de coerência de negócio | `python3 scripts/specs/assert_web_coerencia.py --check all` | PASS | `consulta-publica`: PASS<br>`visual`: PASS<br>`contratos`: PASS<br>`cobertura`: PASS (94.62% linhas agregadas, todos os alterados ≥ 80%) |
| Retrocompatibilidade com adaptacao | `python3 scripts/specs/assert_web_adaptacao.py --check proxy && python3 scripts/specs/assert_web_adaptacao.py --check node` | PASS | Proxy com 4 grupos de prefixos (/api/v1/); setup do Vitest no Node 25+ |
| Validador TLC do plano | `python3 .agents/skills/tlc-spec-lean/scripts/validate_plan.py fase4/web-coerencia-negocio` | PASS | 0 erro(s), 0 aviso(s) |
| Validador TLC dos checks | `python3 .agents/skills/tlc-spec-lean/scripts/validate_checks.py fase4/web-coerencia-negocio` | PASS | 0 erro(s), 2 aviso(s) benignos de seletor em suíte de script |
| Integridade de links | `python3 scripts/specs/verificar_links.py` | PASS | 81 arquivos, 312 links, 0 problemas |

---

## 3. Sensor de Discriminação (Mutações)

Conforme a diretiva TLC Spec Lean (`mutations: 3`), três mutações intencionais foram aplicadas em arquivos de produção alterados nesta entrega, comprovando que a suíte de testes detecta e falha imediatamente diante de regressões, sendo revertidas com precisão cirúrgica:

| # | Arquivo Alvo | Mutação Injetada | Teste de Detecção (Asserção Quebrada) | Resultado |
|---|---|---|---|---|
| **M1** | `src/app/core/store/os-jornada.store.ts:14` | Redução do limite de retenção de histórico: `MAX_REGISTROS = 50;` (original: `100`) | `OsJornadaStore > deve limitar a lista a 100 registros recentes`<br>`AssertionError: expected 50 to be 100` | **Morta (KILLED)** — Código restaurado com sucesso |
| **M2** | `src/app/features/usuarios/services/usuario.service.ts:28` | Alteração de enum de perfil numérico no payload: `'Mecanico': 99` (original: `3`) | `UsuarioService > criar mapeia role string para tipo numérico no backend`<br>`AssertionError: expected { tipo: 99 } to deeply equal { tipo: 3 }` | **Morta (KILLED)** — Código restaurado com sucesso |
| **M3** | `src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.ts:313` | Supressão de flag de restrição de execução: `this.acessoExecucaoRestrito.set(false);` (original: `!!fila.restrito`) | `OsKanbanPage > exibe banner de sem acesso à execução quando Atendente recebe 403 da fila`<br>`AssertionError: expected false to be true` | **Morta (KILLED)** — Código restaurado com sucesso |

---

## 4. Auditoria dos Checks (C1 a C18)

| Check | Descrição Resumida | AC Correspondente | Status |
|---|---|---|---|
| **C1** | Reutilização de `ModalDialogComponent` e `AlertBannerComponent` com tokens CSS | AC 1 | PASS |
| **C2** | `SagasPageComponent` e `AprovacaoOrcamentoPageComponent` padronizadas em `<app-page-container>` | AC 2 | PASS |
| **C3** | Remoção integral da rota, componente e links de `/consulta-publica` | AC 3 | PASS |
| **C4** | `ServicoService` com `precoUnitario` no GET/POST e descrição opcional | AC 4 | PASS |
| **C5** | `InsumoService` com `quantidadeDisponivel`, `quantidadeReservada`, `quantidadeInicial` e entradas | AC 5 | PASS |
| **C6** | Permissões em catálogo (`Administrador`) e entrada de insumo (bloqueio de `Mecanico`) | AC 6 | PASS |
| **C7** | `OrcamentoPageComponent` com agrupamento de itens únicos (`valor: 0`) e suporte avulso | AC 7 | PASS |
| **C8** | Registro de etapa na fila com descrição obrigatória e indicação visual de fallback | AC 8 | PASS |
| **C9** | Fila de execução com os 5 estados reais de `EstadoExecucao`, filtro `?mecanico=` e flags | AC 9 | PASS |
| **C10** | Detalhe da OS filtrando execução por `ordemServicoId` e tratamento silencioso de `403` | AC 10 | PASS |
| **C11** | Cadastro de veículos com `chassi` (17 chars) e `renavam` (11 dígitos) e edição restrita via `PUT` | AC 11 | PASS |
| **C12** | Abertura de OS (`OsNovaPageComponent`) buscando veículos do cliente via `GET /api/v1/veiculos?clienteId=` | AC 12 | PASS |
| **C13** | Diretiva de máscara alternando entre CPF (≤ 11) e CNPJ (12-14), e documento imutável | AC 13 | PASS |
| **C14** | Edição de usuário via `PUT /api/v1/usuarios/{id}` com `{ nomeCompleto, tipo }` numérico | AC 14 | PASS |
| **C15** | `OsJornadaStore` com retenção local de IDs, limpeza no logout e Kanban agregando atalho | AC 15 | PASS |
| **C16** | Detalhe da OS com badges e link de aprovação com apenas `orcamentoId`, e checkout em aprovação | AC 16 | PASS |
| **C17** | Mapeamento dos 29 códigos de erro de domínio e extração de `detalhe` e `erro` | AC 17 | PASS |
| **C18** | Cobertura ≥ 80% nos arquivos alterados/novos (94.62% agregado) e compilação limpa | AC 18 | PASS |

---

## 5. Integridade da Árvore e Conclusão

- A árvore de trabalho foi inspecionada após os testes de mutação: nenhum arquivo residual ou temporário foi deixado em `submodules/AutoReparos.Web`.
- Todos os arquivos editados encontram-se perfeitamente restaurados aos seus estados originais.
- Nenhuma referência ou link absoluto foi introduzido na documentação ou nas especificações técnicas.

**Veredito Final:** **PASS**. A feature `fase4/web-coerencia-negocio` atende com rigor a todos os critérios estabelecidos pelo protocolo TLC Spec Lean e está pronta para conclusão.
