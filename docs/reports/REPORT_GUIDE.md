# 📄 Relatório LaTeX: Compilação e Estrutura Interna (`projeto_autoreparos_fase2.tex`)

Este documento descreve como compilar o arquivo **LaTeX** `projeto_autoreparos_fase2.tex` para gerar o **PDF** e explica resumidamente a sua arquitetura de design e componentes internos.

---

## 🛠️ Como Compilar o Arquivo `.tex` e Gerar o PDF

Para gerar o PDF a partir do arquivo `projeto_autoreparos_fase2.tex`, você pode utilizar qualquer uma das abordagens abaixo:

### 1. Linha de Comando (Linux / macOS / WSL)

Certifique-se de ter uma distribuição TeX instalada (como **TeX Live** ou **MiKTeX** com suporte ao `pdflatex` ou `latexmk`):

```bash
# Instalação rápida de dependências no Ubuntu/Debian (se necessário):
sudo apt update && sudo apt install texlive-latex-extra texlive-lang-portuguese

# Compilação via pdflatex (executar 2 vezes para ajustar referências e dimensões do TikZ):
pdflatex projeto_autoreparos_fase2.tex
pdflatex projeto_autoreparos_fase2.tex
```

### 2. VS Code

1. Instale a extensão **LaTeX Workshop** no VS Code.
2. Abra o arquivo `projeto_autoreparos_fase2.tex`.
3. Utilize o atalho `Ctrl + Alt + B` (ou clique no ícone do TeX na barra lateral) para compilar. O PDF será gerado automaticamente na mesma pasta.

### 3. Overleaf

1. Acesse o [Overleaf](https://www.overleaf.com/).
2. Crie um novo projeto e faça o upload do arquivo `projeto_autoreparos_fase2.tex`.
3. Defina o compilador para **pdfLaTeX** e clique em **Recompile**.

---

## 🎨 Como o Arquivo está Projetado Internamente

O relatório foi desenvolvido em **LaTeX puro** com foco em design moderno, apresentação visual executiva e diagrama vetorial embarcado, projetado para caber em **folha única (A4)** de alta qualidade.

### 🧩 Estrutura dos Componentes:

1. **Configuração da Página & Tipografia**:
   - `geometry`: Margens ajustadas para `1.5cm` para otimizar espaço de exibição em página única.
   - `helvet`: Fonte sem serifa moderna como padrão (`\familydefault{\sfdefault}`).
   - `\pagestyle{empty}`: Desabilita cabeçalhos e numeração de página para layout limpo.

2. **Paleta de Cores Personalizada**:
   - `primary` (`#1E3A8A`): Azul institucional escuro para títulos e bordas de destaque.
   - `secondary` (`#0D9488`): Verde azulado para links e acentos visuais.
   - `accent` (`#EF4444`): Vermelho para alertas de métricas e escalabilidade (HPA).
   - `k8sblue` (`#326CE5`): Azul oficial do Kubernetes para destacar o cluster.

3. **Painel Superior e Caixa de Informações (`tcolorbox`)**:
   - Utiliza a biblioteca `tcolorbox` com estilo customizado (`elegantbox`) dividida em duas colunas (`minipage`):
     - **Coluna Esquerda**: Links públicos do repositório GitHub e vídeo de apresentação.
     - **Coluna Direita**: Lista dos integrantes do Grupo 78 e RMs.

4. **Diagrama de Arquitetura em Nuvem (`TikZ`)**:
   - Todo o diagrama de arquitetura foi desenhado usando a biblioteca nativa `tikz` (sem imagens externas rasterizadas).
   - **Nós e Fluxo de Dados**:
     - *Cliente/Navegador* $\rightarrow$ *Nginx Ingress Controller* (AWS ALB/NLB).
     - *API Service (ClusterIP)* $\rightarrow$ Distribuição para *API Pods (Réplicas 1 e 2)*.
     - *Horizontal Pod Autoscaler (HPA)* $\rightarrow$ Monitoramento de métrica de CPU/Memória (> 80%).
     - *PostgreSQL StatefulSet Pod* $\rightarrow$ Conexão de dados com *Persistent Volume Claim (PVC)*.
   - **Agrupamento em Camadas (`backgrounds` & `fit`)**:
     - Caixas tracejadas delimitando o **Cluster Kubernetes (EKS / Kind)** e a **AWS Cloud VPC**.

5. **Resumo Executivo da Fase 2 (`tcolorbox`)**:
   - Quadro descritivo destacando os 5 pilares implementados: *IaC (Terraform)*, *Containerização & Helm*, *Escalabilidade (HPA)*, *Persistência (StatefulSet)* e *CI/CD (GitHub Actions)*.
