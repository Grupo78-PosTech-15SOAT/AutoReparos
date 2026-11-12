# Extração da aplicação Web - checks

Mutations: 5
Build: `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web build`; `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx -m:1`
Plan: `.specs/features/fase4/web/plan.md`

10 checks · 3 slices · 1 one-way door · 0 open questions, 0 blocking

## Checks

### S1 - Fonte única da aplicação

**C1** - `develop` do Web deriva de `91a1aa1` e conserva todos os arquivos rastreados da aplicação Angular de Cadastros em `3d71ba8`, salvo os 6 ajustes documentados no Coverage (AC 1)
Proof: `python scripts/specs/assert_web_extraction.py --check source`

**C2** - Cadastros não rastreia mais `AutoReparos.Web` nem referencia essa pasta em CI ou Compose (AC 2)
Proof: `python scripts/specs/assert_web_extraction.py --check cadastros`

**C3** - O pai fixa um commit de `origin/develop` do Web e `.gitmodules` declara `branch = develop` (AC 3)
Proof: `python scripts/specs/assert_web_extraction.py --check gitlink`

### S2 - Build e execução a partir do Web

**C4** - `yarn install --frozen-lockfile`, `yarn build` e `yarn test --watch=false` saem com código 0 na raiz do Web (AC 4)
Proof: `python scripts/specs/assert_web_extraction.py --check yarn`

**C5** - A imagem Web construída no novo repositório contém `index.html` e serve `/` e `/login` com HTTP 200 (AC 5)
Proof: `python scripts/specs/assert_web_extraction.py --check docker`

**C6** - `API_URL` definido é injetado como `window.__env.apiUrl`; sem valor, o padrão documentado permanece (AC 6)
Proof: `python scripts/specs/assert_web_extraction.py --check runtime-env`

**C7** - CI do Web dispara em PR para `main` e push em `develop`/`main`, com jobs `build`, `test`, `quality` e `SONAR_TOKEN` só via secret (AC 7)
Proof: `python scripts/specs/assert_web_extraction.py --check web-ci`

### S3 - Integração sem caminhos obsoletos

**C8** - Compose, workflows e script de imagem do pai usam `submodules/AutoReparos.Web`, sem caminho ao frontend antigo (AC 8)
Proof: `python scripts/specs/assert_web_extraction.py --check parent-paths`

**C9** - CI do Cadastros compila e testa só o backend, sem etapa Angular removida (AC 9)
Proof: `python scripts/specs/assert_web_extraction.py --check cadastros-ci`

**C10** - README do Web documenta instalação, build, teste, `API_URL`, `develop` e a adaptação futura de paths HTTP (AC 10)
Proof: `python scripts/specs/assert_web_extraction.py --check readme`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| fonte do projeto (2) | 98 caminhos rastreados de `3d71ba8`, dos quais 92 com bytes iguais e 6 ajustes documentados (`README.md`, `package.json`, `yarn.lock`, `angular.json`, `docker-entrypoint.sh` e `nginx.conf`; os dois últimos em `5d1aeae` para resolver o coletor OpenTelemetry sob demanda e permitir o smoke Docker) C1 · pasta removida do Cadastros C2 | - |
| branches/ponteiro (3) | `main` inicial preservada C1 · `develop` contém Angular C1 · gitlink do pai em `origin/develop` C3 | - |
| comandos locais (3) | instalação congelada C4 · build C4 · teste C4 | - |
| rotas de smoke (2) | `/` 200 C5 · `/login` 200 C5 | - |
| configuração de URL (2) | `API_URL` definido C6 · padrão sem `API_URL` C6 | - |
| gatilhos de CI (3) | PR `main` C7 · push `develop` C7 · push `main` C7 | - |
| jobs de CI (3) | `build` C7 · `test` C7 · `quality` C7 | - |
| consumidores do caminho Web no pai (5) | Compose C8 · `ci.yml` C8 · `deploy-app.yml` C8 · `provision-aws.yml` C8 · `build-push-images.sh` C8 | - |
| consumidores no Cadastros (3) | árvore rastreada C2 · workflow C2,C9 · Compose C2 | - |
| porta de propriedade (1) | Web na raiz da `develop`, sem cópia no Cadastros C1-C3 | - |

## Swept

- validation: C1,C3,C8 - arquivos e ponteiros concretos conferidos contra a origem.
- failure modes: C4,C5 - instalação, build, teste e imagem falham com saída não zero.
- idempotency: C1,C3 - reinicializar o submódulo mantém o mesmo commit e os mesmos arquivos.
- authorization: C7 - `SONAR_TOKEN` vem de secret; proteção efetiva da `main` pertence a `infra-ci-observabilidade`.
- concurrency: n/a - transferência de código sem escrita de dados compartilhados.
- data lifecycle: C1,C2 - origem removida somente depois da cópia íntegra.
- dependency failure: C4,C5 - falha de Yarn ou Docker torna o gate vermelho.
- state transitions: C1,C3 - `main` inicial -> `develop` com aplicação e ponteiro fixado.
- observability: C7 - job `quality` expõe falha ao CI; telemetria da aplicação não muda.

## Handoff

- **Settled mid-build:** user: "Aprovo o plano web" (11/11/2026); escopo: "Extração e integração do repositório (recomendado)".
- **Dispatched:** `web_batch` (Web `develop`, C1/C4-C7/C10) e `cadastros_batch` (Cadastros `feature/web-extraction`, C2/C9) enviados em 11/11/2026; `web_verifier` independente despachado em 12/11/2026 no HEAD `38fab61` (12/12 provas, 3/3 mutações mortas).
- **Boundary:** implementação em Web `5d1aeae`, Cadastros `961b056` e pai `38fab61`; gate mecânico inicial 12/12 e 3/3. O Verifier independente encontrou três achados (F1-C6 funcional, F2-C7 e F3-C5 de força das provas); correções e novo gate em andamento.
- **Abandoned:** none
- **Walked:** none - telas existentes não mudam nesta etapa.
