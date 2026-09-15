# Plano Detalhado — Subfase 4: Arquitetura Formal & Modelo de Dados (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** Documentos RFC-003, ADR-002, ADR-003, Justificativa do Modelo de Dados e Diagramas  
> **Status:** Concluído & Auditado  

---

## 1. Documentos Elaborados & Racional Técnico

1. **RFC-003 — Autenticação Serverless de Clientes via CPF e E-mail:**
   - Desacoplamento do ASP.NET Identity para evitar poluição da base com clientes finais.
   - Emissão de JWT efêmero (1 hora) com `Role: Cliente`.
   - Comparativo com alternativas descartadas (Cognito, Magic Link, Autorizador nativo).
2. **ADR-002 — Isolamento de Dados e Política Zero-Trust:**
   - Titular extraído exclusivamente de claims do token validado (`sub`/`NameIdentifier`).
   - Bloqueio BOLA (Broken Object Level Authorization).
   - Consultas com filtros que não pertencem ao cliente retornam lista vazia `[]` com HTTP 200 (sem vazamento de metadados).
3. **ADR-003 — Estratégia de Observabilidade Ponta a Ponta com OpenTelemetry:**
   - Arquitetura baseada em W3C TraceContext, SDK nativo .NET 10 e OTel Collector.
   - Racional da decisão vendor-agnostic e validação com falha real.
4. **Seleção de Banco de Dados e Modelo de Dados (`database-selection-and-data-model.md`):**
   - Justificativa formal do PostgreSQL 16 (Transações ACID, integridade referencial com `Restrict`/`Cascade`, `timestamp with time zone`).
   - Comparativo com MongoDB e MySQL.
   - Modelo ER em Mermaid e dicionário das 8 tabelas do sistema.
5. **Diagrama de Sequência End-to-End (`sequence_portal_auth_and_query.md`):**
   - Fluxo completo da autenticação na Lambda e consumo de rotas no cluster EKS.
