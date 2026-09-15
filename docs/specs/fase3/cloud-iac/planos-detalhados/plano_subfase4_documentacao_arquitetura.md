# Plano Detalhado de Implementação - Subfase 4: Documentação Formal de Arquitetura (RFCs & ADRs)

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3 (Track A - Infraestrutura Cloud & Arquitetura)  
> **Diretório Alvo de Entrega:** [`docs/architecture/`](../../../../architecture)  
> **Documento Mestre:** [`../plano_execucao_track_a_cloud_iac.md`](../plano_execucao_track_a_cloud_iac.md)  
> **Sessão de Alinhamento:** Validação via `/grill-me` (14/09/2026)  
> **Responsável:** Arquiteto de Software & Engenheiro de Infraestrutura Cloud  
> **Status:** Aprovado para Execução  

---

## 1. Contexto de Negócio & Justificativa

### 1.1. Comunicação Técnica e Racional de Engenharia
Sistemas corporativos maduros requerem mais do que código funcional; demandam rastreabilidade formal das decisões arquiteturais que orientam sua evolução.
- **Racionalização de Custos e Disponibilidade:** Cada escolha técnica (como terceirizar a persistência para o AWS RDS ou adotar um API Gateway HTTP v2 com VPC Link) envolve trade-offs de custo, segurança, resiliência e manutenção operacional (*toil*).
- **Atendimento aos Critérios da Banca FIAP SOAT:** A banca examinadora avalia explicitamente a fundamentação teórica e técnica da segregação em 4 repositórios, a justificativa da substituição do StatefulSet por banco gerenciado e a topologia de borda adotada.
- **Governança a Longo Prazo:** Os documentos em formato **RFC** (*Request for Comments*) e **ADR** (*Architecture Decision Record*) servem como contratos de conhecimento duradouros para novas equipes e auditorias.

---

## 2. Matriz de Decisões Técnicas Alinhadas (Sessão Grill-Me)

A tabela a seguir consolida as 5 decisões arquiteturais pactuadas para a elaboração dos documentos formais:

| # | Dimensão Documental | Decisão Alinhada | Racional & Impacto Técnico |
|:---:|:---|:---|:---|
| **1** | **Nível de Profundidade & Formato** | **Documentação Técnica Aprofundada em Português** | Padrão formal de engenharia com diagramas Mermaid estruturais e de sequência (autenticação e fluxo de API), tabelas completas de CIDR/rede, matriz RACI e análise quantitativa de custos no padrão Free Tier. |
| **2** | **Papel do Repositório Pai (RFC-001)** | **Abordagem Híbrida Orquestradora** | O repositório pai (`AutoReparos`) atua como Hub Unificado de Orquestração, Governança e Documentação (via submódulos Git e docker-compose local), enquanto os 4 repositórios satélites mantêm total autonomia de versionamento, esteiras de CI/CD e ciclo de vida em nuvem. |
| **3** | **Estratégia do Banco (RFC-002)** | **Análise Multidimensional Completa** | Tabela matricial comparativa detalhando SLA (99.95% vs ~99.0%), RPO/RTO, Backup Point-in-Time Recovery (PITR) até o segundo, Criptografia KMS, segredos gerenciados via Secrets Manager, redução drástica de *toil* e alinhamento FinOps Free Tier (`db.t4g.micro`, 20 GiB gp3). |
| **4** | **Decisão de Borda (ADR-001)** | **Comparativo Quádruplo Formal** | Contraste estruturado entre: (1) HTTP API v2 + VPC Link Privado, (2) REST API v1, (3) Ingress NLB exposto diretamente na internet pública e (4) Application Load Balancer (ALB) público, fundamentando latência p99, custo ($1.00/milhão), complexidade TLS e segurança *Zero-Trust*. |
| **5** | **Índice Geral (`README.md`)** | **Hub Visual Integrado** | Diagrama Mermaid macro de componentes de nuvem conectando Borda ➔ API Gateway ➔ VPC Link ➔ NLB Interno ➔ Ingress Nginx ➔ Pods .NET 10 ➔ RDS PostgreSQL, tabela de portas/protocolos, visão dos 4 repositórios e navegação direta com links estritamente relativos. |

---

## 3. Estrutura dos Documentos Formais a Elaborar

### 3.1. Mapa de Entregáveis em `docs/architecture/`

```
docs/architecture/
├── README.md                                             # Índice da arquitetura e Diagrama de Componentes Cloud (Mermaid)
├── RFC-001-cloud-architecture-and-repo-segregation.md    # Segregação em 4 repositórios e Topologia AWS VPC Multi-AZ
├── RFC-002-managed-database-strategy-rds.md             # Estratégia RDS PostgreSQL 16 vs StatefulSet in-cluster
└── ADR-001-adoption-aws-api-gateway.md                  # Decisão de adoção do AWS API Gateway HTTP v2 com VPC Link
```

---

## 4. Especificações Detalhadas de Conteúdo por Documento

### 4.1. `RFC-001-cloud-architecture-and-repo-segregation.md`
- **Título:** RFC 001 - Arquitetura Cloud AWS e Segregação em 4 Repositórios Git Autônomos
- **Status:** Proposto / Aprovado
- **Conteúdo Mandatório:**
  1. **Contexto & Problema:** Limitações do repositório monolítico na Fase 2 (acoplamento de esteiras de CI/CD, conflitos de branch, blast radius ampliado).
  2. **Arquitetura Multi-Repo (Abordagem Híbrida Orquestradora):**
     - Detalhamento dos 4 repositórios autônomos (`AutoReparos.App`, `AutoReparos.AuthLambda`, `AutoReparos.Infra.Database`, `AutoReparos.Infra.K8s`).
     - Papel do repositório pai (`AutoReparos`) como Hub Unificado de Orquestração, Governança e Documentação integrada via submódulos Git e docker-compose local de desenvolvimento.
  3. **Topologia de Nuvem AWS VPC Multi-AZ:**
     - Tabela completa de subnets e blocos CIDR (VPC `10.0.0.0/16`, Subnets públicas `10.0.1.0/24` e `10.0.2.0/24`, Subnets privadas EKS `10.0.3.0/24` e `10.0.4.0/24`, Subnets de banco de dados).
     - Roteamento com Internet Gateway e NAT Gateway para tráfego de saída das subnets privadas.
  4. **Diagramas Mermaid:**
     - Diagrama de Topologia de Rede e Isolamento de Subnets.
     - Diagrama de Sequência do Fluxo de Requisições da Borda ao Backend.
  5. **Matriz de Responsabilidade (RACI):** Divisão formal de atribuições entre aplicação, serverless, dados e infraestrutura.
  6. **Políticas de Governança Git:** Proteção da branch `main`, exigência de PR com status check verde e aprovação obrigatória.

### 4.2. `RFC-002-managed-database-strategy-rds.md`
- **Título:** RFC 002 - Estratégia de Banco de Dados Gerenciado: Adoção do AWS RDS PostgreSQL 16 vs StatefulSet in-cluster
- **Status:** Proposto / Aprovado
- **Conteúdo Mandatório:**
  1. **Histórico da Fase 2:** Utilização de StatefulSet com Persistent Volume Claims (PVC) no Kubernetes e suas limitações operacionais.
  2. **Análise Comparativa Multidimensional (Tabela Matricial):**
     - **Disponibilidade & SLA:** 99.95% no RDS vs ~99.0% dependente do ciclo de vida dos nós EKS no StatefulSet.
     - **Backup e Recuperação (RPO & RTO):** Snapshots automatizados contínuos com Point-in-Time Recovery (PITR) até o segundo exato (RPO < 5 min, RTO < 15 min) vs rotinas manuais sujeitas a corrupção.
     - **Segurança & Criptografia:** Criptografia em repouso via chave gerenciada KMS e credenciais persistidas no AWS Secrets Manager.
     - **Manutenção e Patches:** Janelas automáticas de atualização de SO e minor versions pela AWS sem parada prolongada.
     - **Sobrecarga Operacional (Toil):** Eliminação de tarefas repetitivas de administração de banco e gestão de volumes EBS.
  3. **Alinhamento FinOps (Padrão Free Tier / Custo Zero):** Dimensionamento otimizado em `db.t4g.micro`, 20 GiB gp3, Single-AZ com snapshot sob demanda.

### 4.3. `ADR-001-adoption-aws-api-gateway.md`
- **Título:** ADR 001 - Adoção do AWS API Gateway HTTP API v2 com VPC Link Privado
- **Status:** Aceito
- **Contexto:** Necessidade de unificar as rotas públicas da aplicação sob um único ponto de entrada para o Portal do Cliente e API operacional.
- **Decisão:** Adotar **AWS API Gateway HTTP API v2** com **VPC Link Privado**.
- **Comparativo Quádruplo Formal de Alternativas:**
  1. *Alternativa 1: AWS API Gateway HTTP API v2 + VPC Link (Escolhida):* Menor latência (~10ms a menos que REST API), custo reduzido ($1.00 por milhão de requisições), integração nativa com Lambda (`AWS_PROXY`) e segurança Zero-Trust conectando diretamente ao NLB interno nas subnets privadas.
  2. *Alternativa 2: AWS API Gateway REST API (v1):* Descartada por custo ~70% superior ($3.50/milhão), maior complexidade de configuração e latência de processamento sem benefícios para o caso de uso.
  3. *Alternativa 3: Ingress Nginx / NLB Exposto Diretamente na Internet:* Descartada pelo risco crítico de segurança de expor nós do cluster na internet e complexidade de orquestrar a rota da Lambda.
  4. *Alternativa 4: Application Load Balancer (ALB) Público na Borda:* Descartada pelo custo fixo inicial (~$16 a $22/mês por ALB ativo) em desacordo com o padrão de custo zero, além de ausência de suporte nativo simples a payload v2 de Lambdas.
- **Consequências:** Tráfego centralizado, auditoria JSON estruturada no CloudWatch Logs, certificados TLS automáticos na borda e segurança perimetral blindada.

### 4.4. `docs/architecture/README.md` (Índice Geral & Hub Visual Integrado)
- **Hub Visual Integrado:** Sumário interativo de navegação para todas as RFCs e ADRs da plataforma.
- **Diagrama Mermaid de Componentes Cloud:** Fluxo completo Internet ➔ API Gateway ➔ VPC Link ➔ NLB Interno ➔ Ingress Nginx ➔ Pods .NET 10 ➔ RDS PostgreSQL 16.
- **Tabela de Portas, Protocolos e Roteamento:** Matriz relacionando endpoints, portas (8080, 5432, 4317/4318) e contratos de segurança.
- **Visão Executiva dos 4 Repositórios:** Quadro de alinhamento com a banca examinadora do Tech Challenge.

---

## 5. Passo a Passo Detalhado de Implementação

### Passo 4.1: Criar o Diretório `docs/architecture/`
Garantir a criação do diretório no repositório pai:
```bash
mkdir -p docs/architecture
```

### Passo 4.2: Redigir os Documentos
Gerar os 4 arquivos em formato Markdown padrão GitHub Flavored Markdown (GFM), observando com rigor:
- **Caminhos Estritamente Relativos:** Proibição de caminhos absolutos (`/home/...`, `C:\...`, `file:///...`). Links internos entre docs devem utilizar `./` ou `../`.
- **Diagramas Mermaid Válidos:** Sintaxe validada com nós entre aspas e tipos suportados (`flowchart TD`, `sequenceDiagram`).

---

## 6. Estratégia de Testes & Validação Empírica

Para assegurar a conformidade da documentação antes de consolidar a entrega:

```bash
# 1. Varredura rigorosa contra caminhos absolutos (/home/ ou file://)
grep -rn "/home/" docs/architecture/
grep -rn "file://" docs/architecture/
# Saída esperada: Nenhum resultado encontrado.

# 2. Verificação de integridade dos links relativos
# Garantir que todos os arquivos referenciados existam no sistema de arquivos
ls -la docs/architecture/*.md

# 3. Validação da renderização dos blocos Mermaid
# Conferir se a sintaxe Mermaid não contém erros de parsing
```

---

## 7. Gestão de Riscos, Mitigações e Rollback

| Risco Identificado | Severidade | Probabilidade | Mitigação Técnica | Procedimento de Rollback |
|:---|:---:|:---:|:---|:---|
| Quebra de links em outros repositórios ou submódulos | Baixa | Baixa | Utilizar referências relativas padronizadas a partir da raiz do repositório pai (`docs/architecture/`). | Ajustar caminhos relativos no arquivo afetado. |
| Inconsistência entre código Terraform e documentação | Média | Baixa | Validar que nomes de recursos, instâncias (`db.t4g.micro`) e rotas coincidam exatamente com os arquivos HCL dos submódulos. | Revisar e alinhar a documentação técnica com o código Terraform. |

---

## 8. Critérios de Aceite & Definition of Done (DoD)

- [x] Arquivo `docs/architecture/RFC-001-cloud-architecture-and-repo-segregation.md` concluído.
- [x] Arquivo `docs/architecture/RFC-002-managed-database-strategy-rds.md` concluído.
- [x] Arquivo `docs/architecture/ADR-001-adoption-aws-api-gateway.md` concluído.
- [x] Arquivo `docs/architecture/README.md` com diagrama Mermaid e índice consolidado.
- [x] 100% de conformidade com a regra de **Caminhos Estritamente Relativos** (Zero caminhos absolutos).
