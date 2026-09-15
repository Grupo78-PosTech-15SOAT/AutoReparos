# 📄 Relatórios Técnicos e Executivos FIAP (Fase 2 e Fase 3)

Este diretório contém os documentos de entrega e relatórios técnicos em formato **LaTeX (`.tex`)** e **PDF compilado** do projeto **AutoReparos** para a pós-graduação em Software Architecture da **FIAP** (Grupo 78 - Turma 15SOAT).

---

## 📁 Estrutura de Pastas

```
docs/reports/
├── REPORT_GUIDE.md                     # Guia de compilação e especificações dos relatórios
├── fase2/                              # Entrega da Fase 2 (Kubernetes, Helm, HPA e CI/CD)
│   ├── projeto_autoreparos_fase2.tex   # Código-fonte LaTeX da Fase 2
│   └── projeto_autoreparos_fase2.pdf   # PDF compilado executivo (folha única)
└── fase3/                              # Entrega da Fase 3 (Serverless, RDS, Multi-Repo e Observabilidade)
    ├── projeto_autoreparos_fase3.tex   # Código-fonte LaTeX da Fase 3
    └── projeto_autoreparos_fase3.pdf   # PDF compilado corporativo (2 páginas)
```

---

## 🛠️ Como Compilar os Arquivos `.tex` e Gerar os PDFs

### Opção 1: Via Docker (Recomendado — Sem Instalar TeX Live Localmente)

Você pode compilar qualquer um dos relatórios utilizando o contêiner oficial `danteev/texlive`:

```bash
# Compilação da Fase 2:
docker run --rm -v $(pwd)/docs/reports/fase2:/workspace -w /workspace danteev/texlive \
  bash -c "pdflatex projeto_autoreparos_fase2.tex && pdflatex projeto_autoreparos_fase2.tex && rm -f *.aux *.log *.out"

# Compilação da Fase 3:
docker run --rm -v $(pwd)/docs/reports/fase3:/workspace -w /workspace danteev/texlive \
  bash -c "pdflatex projeto_autoreparos_fase3.tex && pdflatex projeto_autoreparos_fase3.tex && rm -f *.aux *.log *.out"
```

### Opção 2: Linha de Comando Local (Linux / macOS / WSL)

Requer uma distribuição TeX instalada (como **TeX Live** com `pdflatex` ou `xelatex` e pacote `fontawesome5`):

```bash
# Instalação rápida de dependências no Ubuntu/Debian:
sudo apt update && sudo apt install texlive-latex-extra texlive-lang-portuguese texlive-fonts-extra

# Compilar Fase 2:
cd docs/reports/fase2
pdflatex projeto_autoreparos_fase2.tex
pdflatex projeto_autoreparos_fase2.tex

# Compilar Fase 3:
cd ../fase3
pdflatex projeto_autoreparos_fase3.tex
pdflatex projeto_autoreparos_fase3.tex
```

### Opção 3: VS Code (LaTeX Workshop)

1. Instale a extensão **LaTeX Workshop** no VS Code.
2. Abra o arquivo `.tex` desejado (`./fase2/projeto_autoreparos_fase2.tex` ou `./fase3/projeto_autoreparos_fase3.tex`).
3. Pressione `Ctrl + Alt + B` para compilar. O PDF correspondente será atualizado no mesmo diretório.

### Opção 4: Overleaf

1. Acesse o [Overleaf](https://www.overleaf.com/).
2. Crie um novo projeto e faça o upload dos arquivos da pasta desejada.
3. Configure o compilador para **pdfLaTeX** e clique em **Recompile**.

---

## 🎨 Comparativo dos Relatórios por Fase

| Aspecto | Fase 2 (`./fase2/`) | Fase 3 (`./fase3/`) |
| :--- | :--- | :--- |
| **Tema** | Arquitetura Cloud, CI/CD, Containerização e Kubernetes | Operação Corporativa, Serverless, RDS Gerenciado e Segregação Multi-Repo |
| **Extensão** | 1 página A4 executiva | 2 páginas A4 corporativas de alta densidade |
| **Arquitetura TikZ** | Ingress, Pods API .NET, HPA e PostgreSQL StatefulSet com PVC | AWS API Gateway v2, Lambda Serverless, EKS, RDS Multi-AZ e OTel/New Relic |
| **Escopo Repositórios** | Repositório Monolítico / Central | 4 Repositórios Autônomos + 1 Repositório Orquestrador Pai |
| **Observabilidade** | Métricas básicas de pod no HPA | OpenTelemetry Collector, 3 Dashboards Grafana e APM New Relic |
| **Validação de Testes** | Testes de integração locais | 354 testes aprovados (Domain, AuthLambda, Application, Testcontainers) |
| **Governança** | Deploy automatizado via Helm | Branch `main` protegida, PR obrigatório e usuário `soat-architecture` liberado |

---

## 🧩 Detalhamento do Relatório da Fase 3 (`./fase3/projeto_autoreparos_fase3.tex`)

O documento foi projetado em **LaTeX puro** com vetores nativos em **TikZ** e caixas visuais da biblioteca `tcolorbox`, dividido estrategicamente em 2 páginas:

### Página 1: Identificação, Entregáveis Oficiais e Topologia de Nuvem
1. **Cabeçalho Institucional:** Identificação do curso, turma (15SOAT) e Grupo 78.
2. **Quadro de Repositórios Oficiais:** Links diretos para os 4 repositórios segregados (`AutoReparos.App`, `AutoReparos.AuthLambda`, `AutoReparos.Infra.Database`, `AutoReparos.Infra.K8s`) e repositório Hub.
3. **Confirmação do Avaliador:** Registro formal da adição do usuário `soat-architecture` como colaborador.
4. **Link do Vídeo Demonstrativo:** Link para a gravação no Google Drive de até 15 minutos.
5. **Diagrama de Arquitetura Cloud (TikZ):** Diagrama vetorial mostrando o fluxo do Cliente $\rightarrow$ API Gateway v2 $\rightarrow$ Lambda (Auth) / VPC Link $\rightarrow$ EKS Pods $\rightarrow$ RDS PostgreSQL 16 $\rightarrow$ OTel $\rightarrow$ Grafana, Jaeger e New Relic.
6. **Roteiro Cronometrado de 15 Minutos:** Tabela dividida em 6 blocos com os objetivos demonstrados em cada janela de tempo.

### Página 2: Defesa Arquitetural, Requisitos, Qualidade e Execução
1. **Matriz de Conformidade com o Edital:** Tabela mapeando os 8 requisitos mandatórios da Fase 3 com status "Atendido" e soluções implementadas.
2. **Decisões Arquiteturais (RFCs e ADRs):** Resumo técnico da RFC-001, RFC-002, RFC-003, ADR-001, ADR-002, ADR-003 e Modelo de Dados Relacional.
3. **Observabilidade e Dashboards Mandatórios:** Especificação dos 3 dashboards Grafana (Volume de OS, Tempo Médio por Status, Falhas nas Integrações) e correlação de logs JSON (`TraceId`/`SpanId`).
4. **Suíte de Testes Automatizados:** Tabela consolidando os 354 testes automatizados executados sem falhas (Domain: 113, AuthLambda: 40, Application: 134, IntegrationTests: 67).
5. **Guia Rápido de Execução Local:** Comandos de clonagem recursiva de submódulos e bootstrap do Docker Compose local com os endpoints dos serviços.
