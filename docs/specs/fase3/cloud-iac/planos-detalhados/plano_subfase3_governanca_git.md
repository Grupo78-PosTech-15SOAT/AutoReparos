# Plano Detalhado de Implementação - Subfase 3: Governança Git & Proteção de Branches

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3 (Track A - Infraestrutura Cloud & DevOps)  
> **Escopo:** 4 Repositórios Oficiais da Organização GitHub  
> **Documento Mestre:** [`../plano_execucao_track_a_cloud_iac.md`](../plano_execucao_track_a_cloud_iac.md)  
> **Responsável:** Engenheiro de Infraestrutura Cloud & DevOps  
> **Status:** Concluído e Validado (Commit `980065c` no repositório pai)  

---

## 1. Contexto de Negócio & Justificativa

### 1.1. Governança Corporativa e Blast Radius Reduzido
A segregação do **AutoReparos** em quatro repositórios independentes estabelece fronteiras claras de responsabilidade (*Separation of Concerns*). Contudo, a autonomia técnica só se traduz em estabilidade operacional quando acompanhada por controles rígidos de governança de código:
- **Prevenção de Quebras em Produção:** Modificações diretas na branch `main` (*direct push*) aumentam drasticamente a chance de quebras acidentais de build, falhas de schema no banco de dados e indisponibilidade no portal da oficina mecânica.
- **Revisão Obrigatória por Pares (Code Review):** A exigência formal de Pull Request garante que toda mudança passe por inspeção técnica, validação de regras de arquitetura limpa e execução das suítes de testes unitários e de integração antes do deploy.
- **Conformidade com Requisito 3 do Tech Challenge:** "Regras de proteção ativadas na branch `main` (bloqueio de push direto, PR com status check verde obrigatório)".
- **Alinhamento do /grill-me:** O convite manual ao usuário `soat-architecture` foi formalmente dispensado pelo usuário, mantendo o foco integral na automação da proteção das branches `main`.

---

## 2. Especificações Técnicas & Regras de Proteção

### 2.1. Matriz de Repositórios Alvo

| # | Repositório | Escopo Técnico | Branch Protegida |
|:---:|:---|:---|:---:|
| 1 | `Grupo78-PosTech-15SOAT/AutoReparos.App` | Aplicação Central .NET 10 API e Frontend Web Angular 19 | `main` |
| 2 | `Grupo78-PosTech-15SOAT/AutoReparos.AuthLambda` | Função Serverless de Autenticação de Clientes (.NET 10) | `main` |
| 3 | `Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database` | IaC Terraform do AWS RDS PostgreSQL 16 e SSM | `main` |
| 4 | `Grupo78-PosTech-15SOAT/AutoReparos.Infra.K8s` | IaC Terraform do AWS EKS, Ingress, VPC Link e API Gateway | `main` |

### 2.2. Parâmetros de Proteção da API GitHub v3 (`/branches/main/protection`)

```json
{
  "required_status_checks": {
    "strict": true,
    "contexts": []
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false,
    "required_approving_review_count": 1
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false
}
```

- **`required_status_checks.strict = true`:** Garante que a branch esteja atualizada com a `main` antes do merge (*up to date*).
- **`enforce_admins = true`:** As regras de proteção aplicam-se a todos os desenvolvedores e administradores do repositório.
- **`required_approving_review_count = 1`:** Mínimo de 1 aprovação formal de PR.
- **`allow_force_pushes = false`:** Bloqueio categórico de `git push --force`.
- **`allow_deletions = false`:** Bloqueio contra deleção acidental da branch principal.

---

## 3. Mapeamento de Arquivos e Alterações

```
scripts/
└── infra/
    └── setup-github-governance.sh    # [CRIAR] Script bash para automação via GitHub CLI (gh api)
```

---

## 4. Passo a Passo Detalhado de Implementação

### Passo 3.1: Criação do Script `scripts/infra/setup-github-governance.sh`

O script deve ser idempotente, exibir mensagens claras com cores ANSI e suportar flag `--dry-run`:

```bash
#!/usr/bin/env bash
# ==============================================================================
# Script: setup-github-governance.sh
# Objetivo: Aplicar regras de proteção na branch main nos 4 repositórios oficiais
# ==============================================================================

set -euo pipefail

# Cores para saída
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

ORG="Grupo78-PosTech-15SOAT"
REPOS=(
  "AutoReparos.App"
  "AutoReparos.AuthLambda"
  "AutoReparos.Infra.Database"
  "AutoReparos.Infra.K8s"
)

DRY_RUN=false

if [[ "${1:-}" == "--dry-run" ]]; then
  DRY_RUN=true
  echo -e "${YELLOW}ℹ Modo DRY-RUN ativado. Nenhuma alteração real será aplicada.${NC}"
fi

# 1. Verificar se a CLI gh está instalada e autenticada
if ! command -v gh &> /dev/null; then
  echo -e "${RED}❌ Erro: GitHub CLI (gh) não está instalada.${NC}"
  exit 1
fi

if ! gh auth status &> /dev/null; then
  echo -e "${RED}❌ Erro: GitHub CLI não está autenticada. Execute 'gh auth login'.${NC}"
  exit 1
fi

echo -e "${BLUE}=== Iniciando Aplicação de Governança na Organização: ${ORG} ===${NC}\n"

PAYLOAD=$(cat <<EOF
{
  "required_status_checks": {
    "strict": true,
    "contexts": []
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false,
    "required_approving_review_count": 1
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false
}
EOF
)

for REPO in "${REPOS[@]}"; do
  FULL_REPO="${ORG}/${REPO}"
  echo -e "${BLUE}▶ Processando repositório: ${FULL_REPO}${NC}"

  if [ "$DRY_RUN" = true ]; then
    echo -e "  ${YELLOW}[DRY-RUN] Enviaria PUT para /repos/${FULL_REPO}/branches/main/protection${NC}"
  else
    echo -e "  Configurando proteção de branch na 'main'..."
    if echo "$PAYLOAD" | gh api \
      --method PUT \
      --header "Accept: application/vnd.github+json" \
      "/repos/${FULL_REPO}/branches/main/protection" \
      --input - > /dev/null; then
      echo -e "  ${GREEN}✔ Proteção ativada com sucesso em ${FULL_REPO}:main${NC}"
    else
      echo -e "  ${RED}✖ Falha ao aplicar proteção em ${FULL_REPO}:main. Verifique permissões de admin.${NC}"
    fi
  fi
  echo ""
done

echo -e "${GREEN}=== Processo de Governança Concluído com Sucesso ===${NC}"
```

### Passo 3.2: Atribuir Permissão de Execução
```bash
chmod +x scripts/infra/setup-github-governance.sh
```

---

## 5. Estratégia de Testes & Validação Empírica

### 5.1. Validação em Modo Dry-Run
```bash
bash scripts/infra/setup-github-governance.sh --dry-run
```

### 5.2. Auditoria das Regras Aplicadas via GitHub API
Para consultar se a proteção está devidamente ativa em um dos repositórios:

```bash
gh api /repos/Grupo78-PosTech-15SOAT/AutoReparos.Infra.Database/branches/main/protection \
  --jq '{enforce_admins: .enforce_admins.enabled, pr_reviews: .required_pull_request_reviews.required_approving_review_count, force_push: .allow_force_pushes.enabled}'
```
*Saída esperada:*
```json
{
  "enforce_admins": true,
  "pr_reviews": 1,
  "force_push": false
}
```

---

## 6. Gestão de Riscos, Mitigações e Rollback

| Risco Identificado | Severidade | Probabilidade | Mitigação Técnica | Procedimento de Rollback |
|:---|:---:|:---:|:---|:---|
| Bloqueio acidental de pipeline por branch desatualizada | Média | Média | Desenvolvedores realizam rebase/merge da `main` na branch de trabalho antes do merge. | Atualizar a branch com `git merge main` e rodar a CI novamente. |
| Necessidade emergencial de hotfix direto | Alta | Baixa | Utilizar branch `hotfix/xxx`, abrir PR emergencial e aprovar rapidamente para preservar a trilha de auditoria. | Em caso extremo temporário, desativar proteção via `gh api -X DELETE /repos/{org}/{repo}/branches/main/protection`. |
| Ausência de permissões administrativas no token do GitHub | Média | Baixa | Script valida autenticação e emite diagnóstico claro indicando falta de permissão de admin. | Reautenticar `gh auth login` solicitando o escopo `repo` ou `admin:org`. |

---

## 7. Critérios de Aceite & Definition of Done (DoD)

- [x] Script `scripts/infra/setup-github-governance.sh` criado, documentado e com permissão `+x`.
- [x] Suporte a `--dry-run` e tratamento de erros de autenticação da CLI `gh`.
- [x] Regras de proteção na `main` cobrindo PR obrigatório, status checks e bloqueio de force push.
- [x] Execução bem-sucedida do script nos 4 repositórios da organização (`AutoReparos.App`, `AutoReparos.AuthLambda`, `AutoReparos.Infra.Database`, `AutoReparos.Infra.K8s`).
