# Integridade de Submódulos, CI/CD e Documentação Técnica

> **Objetivo:** Estabelecer regras mandatórias para evitar regressões, quebras de pipeline e divergências documentais identificadas em code reviews automatizados (Copilot AI e SonarCloud).

---

## 1. Regras de Submódulos e CI/CD

### 1.1 Checkout Recursivo Mandatório
- Em qualquer workflow do GitHub Actions (`.github/workflows/*.yml`) que envolva compilação, testes ou deploy de repositórios baseados em submódulos, o step de checkout **deve obrigatoriamente** incluir `submodules: recursive`:
  ```yaml
  - name: Checkout Code
    uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
    with:
      submodules: recursive
  ```
- **Proibido:** Fazer checkout raso/padrão sem inicializar submódulos quando o arquivo `.slnx` ou scripts dependerem de caminhos dentro de `submodules/`.

### 1.2 Resolução de Caminhos de Submódulos em Scripts e Testes
- Pipelines de CI/CD, comandos `dotnet test`, e scripts nunca devem apontar para caminhos legados da raiz do repositório pai.
- Sempre aponte para o caminho canônico do submódulo:
  - Backend: `submodules/AutoReparos.App/AutoReparos.Domain.Tests/...`
  - Lambda: `submodules/AutoReparos.AuthLambda/tests/...`
- Ao usar `--no-build`, certifique-se de que a etapa anterior de compilação compilou a solução agregada (`AutoReparos.slnx`) contendo todos os projetos que serão executados.

### 1.3 Contexto de Build do Docker para Submódulos
- Quando o `Dockerfile` residir em um submódulo, execute `docker build` especificando explicitamente o arquivo `-f` e a pasta do submódulo como contexto:
  ```bash
  docker build -f submodules/AutoReparos.App/AutoReparos.API/Dockerfile submodules/AutoReparos.App
  ```
- **Proibido:** Apontar o contexto do Docker para a raiz caso o `Dockerfile` espere o layout de pastas relativo do submódulo.

### 1.4 Protocolo HTTPS Obrigatório no `.gitmodules`
- As URLs no arquivo `.gitmodules` devem usar sempre o protocolo **HTTPS** público:
  ```ini
  [submodule "submodules/AutoReparos.App"]
      path = submodules/AutoReparos.App
      url = https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.App.git
  ```
- **Proibido:** Utilizar URLs SSH (`git@github.com:...`) no repositório pai, pois exigem chaves SSH privadas pré-configuradas e quebram runners de CI/CD e clones de desenvolvedores sem chave cadastrada.

### 1.5 Fixação de Hashes SHA Completos nas GitHub Actions (SonarCloud)
- Toda e qualquer action referenciada em `uses:` deve ser fixada com o **hash SHA completo de 40 caracteres**, acompanhada de um comentário com a versão semântica de referência:
  ```yaml
  uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
  uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
  ```
- **Proibido:** Utilizar tags mutáveis como `@v4`, `@v3`, `@main` ou `@latest`, o que viola as regras de segurança estática do SonarCloud.

### 1.6 Padrão Canônico para Workflows de CI dos Submódulos (Baseado no `deploy.yml`)
Todos os arquivos de CI dos submódulos (`submodules/*/.github/workflows/ci.yml`) devem seguir rigorosamente o padrão sintático e estrutural do pipeline principal ([`.github/workflows/deploy.yml`](../../.github/workflows/deploy.yml)):

1. **Controle de Concorrência**:
   ```yaml
   concurrency:
     group: ${{ github.workflow }}-${{ github.ref }}
     cancel-in-progress: true
   ```
2. **Gatilhos e Filtros de Arquivos**:
   - Sempre cobrir as branches `main` e `develop`.
   - Ignorar arquivos de documentação e gitignore para economizar minutos de runner:
     ```yaml
     paths-ignore:
       - '**.md'
       - '.gitignore'
       - '.dockerignore'
     ```
3. **Nomenclatura Padrão**:
   - `name: CI - <NomeDoSubmodulo>`
   - Jobs e steps nomeados com clareza e verbos de ação em Português (`Checkout Code`, `Setup .NET Core`, `Restore Dependencies`, `Build Application`, `Run Unit Tests`).
4. **Hashes SHA Canônicos Fixados**:
   | Action | Commit SHA Pinned | Versão Semântica |
   | :--- | :--- | :--- |
   | `actions/checkout` | `11bd71901bbe5b1630ceea73d27597364c9af683` | `# v4.2.2` |
   | `actions/setup-dotnet` | `67a3573c9a986a3f9c594539f4ab511d57bb3ce9` | `# v4.3.1` |
   | `actions/setup-node` | `49933ea5288caeca8642d1e84afbd3f7d6820020` | `# v4.4.0` |
   | `actions/upload-artifact` | `ea165f8d65b6e75b540449e92b4886f43607fa02` | `# v4.6.2` |
   | `actions/download-artifact` | `d3f86a106a0bac45b974a628896c90dbdf5c8093` | `# v4.3.0` |
   | `aws-actions/configure-aws-credentials` | `ff717079ee2060e4bcee96c4779b553acc87447c` | `# v4.0.2` |
   | `hashicorp/setup-terraform` | `b9cd54a3c349d3f38e8881555d616ced269862dd` | `# v3.1.2` |
   | `azure/setup-helm` | `1a275c3b69536ee54be43f2070a358922e12c8d4` | `# v4.3.1` |
   | `aws-actions/amazon-ecr-login` | `062340a7de7a9da2b919efc70d2a5638c4be725c` | `# v2.0.1` |

---

## 2. Regras de Limpeza e Migração Multi-Repo

### 2.1 Eliminação Completa de Código Legado Duplicado
- Ao migrar ou extrair código de pastas locais para submódulos git:
  - Remova **completamente** os diretórios locais antigos da raiz do repositório pai (`AutoReparos.API`, `AutoReparos.Domain`, `k8s/`, `infra/`, etc.).
  - Remova diretórios de build residuais (`bin/`, `obj/`) para evitar conflitos de compilação ou poluição do git status.
  - Remova suites de testes legadas da raiz que instanciem código migrado.
- **Proibido:** Manter código duplicado em transição "parcial", o que induz desenvolvedores e pipelines a compilarem arquivos obsoletos.

### 2.2 Sincronia entre `.gitmodules` e Documentação
- Todo repositório ou submódulo documentado no `README.md` ou em diagramas de arquitetura deve estar ativamente registrado e funcional no arquivo `.gitmodules`.

---

## 3. Regras de Integridade e Fidelidade da Documentação

### 3.1 Fidelidade de Esquema de Banco de Dados (ERD)
- Diagramas ERD (Mermaid) em documentações devem refletir **com precisão cirúrgica** o mapeamento real do EF Core e do PostgreSQL:
  - Se entidades forem separadas (ex: `ORDEM_SERVICO_SERVICOS` e `ORDEM_SERVICO_INSUMOS`), o diagrama **não deve** inventar tabelas genéricas (`ORDEM_SERVICO_ITENS`).
  - Chaves estrangeiras e identificadores devem respeitar o tipo real da entidade (ex: `ResponsavelId: string` para Identity User em vez de UUID).

### 3.2 Consistência Numérica e Somatórios de Métricas
- Ao citar contagens de testes ou métricas quantitativas no `README.md` ou relatórios:
  - O total anunciado deve ser estritamente igual à soma das partes detalhadas (ex: `113 Domínio + 123 Aplicação + 40 Lambda + 67 Integração = 343 testes`).
  - Nunca declare números arbitrários ou desatualizados que contradigam o resultado real de `dotnet test`.

### 3.3 Organização das Especificações Técnicas e Planos de Entrega
- Todas as especificações técnicas (`spec_*.md`) e planos de execução (`plano_*.md`) devem ser organizados em subdiretórios sob `docs/specs/fase<N>/<entregavel>/`:
  - Exemplo: `docs/specs/fase3/submodulos-multirepo/`, `docs/specs/fase3/auth-lambda-portal/`, `docs/specs/fase3/cloud-iac/`, `docs/specs/fase3/observabilidade/`, `docs/specs/fase3/gap-analysis/`.
  - Specs e planos da mesma entrega/task devem residir juntos na mesma subpasta.
- Documentos normativos e padrões de conduta da IA pertencem a `.agents/rules/` e `.agents/knowledge/`, não a `docs/specs/`.

### 3.4 Prevenção de Links Quebrados e Diretórios Fantasmas
- Antes de commitar documentação que referencie arquivos de arquitetura (RFCs, ADRs, especificações):
  - Inspecione a árvore de diretórios do repositório para certificar-se de que o arquivo ou diretório citado existe fisicamente (ex: referenciar `docs/specs/fase3/...` em vez de caminhos planos ou pastas inexistentes).
  - Nunca crie referências a caminhos fictícios.
