# Especificação Fase 4 — Vídeo de Demonstração e Documento do Portal

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Entregáveis da Fase 4 → *Vídeo de demonstração* e *Documentação entregue no portal*  
> **Requisitos cobertos (índice):** R37 a R47  
> **Repositórios afetados:** `AutoReparos` (pai — `docs/entrega/`, `docs/reports/`)  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Rascunho inicial

---

## 1. Critério

**Vídeo de demonstração**

> - Upload no YouTube ou Vimeo (público ou não listado) de até **15 minutos**;
> - Demonstrando:
>   - Fluxo completo de uma OS passando pelos microsserviços;
>   - Execução do Saga Pattern e tratamento de falhas;
>   - Deploy automatizado de pelo menos um microsserviço com validação de testes;
>   - Monitoramento e rastreamento dos fluxos distribuídos.

**Documentação entregue no portal — PDF com:**

> - Nome e identificação dos participantes;
> - Links dos repositórios;
> - Link do vídeo de até 15 minutos;
> - Diagrama geral da arquitetura final com microsserviços, bancos e comunicação;
> - Descrição da estratégia escolhida para o Saga Pattern (orquestração/coreografia);
> - Justificativa da divisão dos microsserviços e tecnologias utilizadas.

---

## 2. Estado atual

| Item | Situação |
|---|---|
| Roteiro de vídeo | [`docs/entrega/roteiro_video_demonstracao_15min.md`](../../../../../docs/entrega/roteiro_video_demonstracao_15min.md) (Fase 3), com checklist pré-gravação e gerador de tráfego para os dashboards |
| Template do portal | [`docs/entrega/template_entrega_portal_fiap.md`](../../../../../docs/entrega/template_entrega_portal_fiap.md) (Fase 3), com capa, integrantes (5 nomes e RMs), repositórios e resumo |
| Relatório LaTeX/PDF | [`docs/reports/fase3/`](../../../../../docs/reports/fase3) e guia em [`docs/reports/REPORT_GUIDE.md`](../../../../../docs/reports/REPORT_GUIDE.md) |
| Diagramas | Mermaid no README do pai e em `docs/architecture/` (arquitetura da Fase 3) |

### 2.1 Feedback da Fase 3 sobre o vídeo

> "Um ponto que ficaram devendo é a apresentação do vídeo: ficou muito curto e não mostra adequadamente a implementação da solução, dashboards, proteção de branches."

| Problema apontado | Resposta na Fase 4 |
|---|---|
| Vídeo muito curto | Roteiro cronometrado com alvo de **13 a 14 minutos**, ensaiado antes |
| Implementação não mostrada | Bloco dedicado a código: orquestrador da Saga, uma compensação, um consumidor idempotente |
| Dashboards não mostrados | Bloco de observabilidade ao vivo com tráfego real (Grafana, Jaeger, Loki) |
| Proteção de branches não mostrada | Tela de configuração da `main` + PR bloqueado por teste ou quality gate |

---

## 3. Lacunas

1. Não há roteiro da Fase 4.
2. Não há template de PDF da Fase 4 nem diagrama da arquitetura final.
3. A justificativa da divisão e das tecnologias ainda não está escrita.

---

## 4. Proposta

### 4.1 Roteiro (≤ 15 min, alvo 13–14 min)

| Tempo | Bloco | O que mostrar | Requisito |
|---|---|---|---|
| 00:00–01:00 | Abertura | Grupo, objetivo da fase, diagrama geral | — |
| 01:00–02:30 | Divisão e tecnologias | Os 3 serviços de negócio + Cadastros e o que cada um faz; PostgreSQL por serviço e DynamoDB da Saga; SQS; repositórios no GitHub | Justificativa |
| 02:30–04:00 | Implementação | Código do orquestrador (máquina de estados), um handler de compensação, Outbox/idempotência | Feedback F3 |
| 04:00–07:00 | **Fluxo completo da OS** | Abrir OS → orçamento (Cobrança) → aprovação → reserva de insumos → execução (fila, etapas, conclusão) → pagamento no sandbox do Mercado Pago → entrega; histórico da OS | R38 |
| 07:00–09:00 | **Saga e falhas** | Recusa do orçamento e cancelamento com reserva (compensações automáticas); falha de liberação com o Execução fora do ar (prazo técnico e compensação da reserva); console de Sagas e reprocessamento da DLQ; mensagem duplicada descartada; histórico com as compensações | R39 |
| 09:00–11:00 | **Monitoramento e rastreamento** | Trace distribuído no Jaeger (grafo de serviços), busca por `ordem_servico.id`, dashboards da Saga e dos serviços no Grafana, logs no Loki, um alerta | R41 + feedback F3 |
| 11:00–13:30 | **Deploy automatizado com testes** | Proteção da `main` (tela de configurações); PR com teste quebrado bloqueado; PR válido mesclado → pipeline (build, testes, Sonar, imagem, Helm) → `kubectl rollout status`; cobertura no SonarCloud | R40 + feedback F3 |
| 13:30–14:00 | Encerramento | Checklist dos requisitos atendidos | — |

Boas práticas (lições da Fase 3):

- Gravar em blocos e editar; ensaiar com cronômetro.
- Dados de demonstração e flags de falha preparados antes ([spec da Saga § 4.7](../saga/fase4_spec_saga_pattern.md#49-como-provocar-falhas-para-testes-e-para-o-vídeo)).
- Gerador de tráfego rodando pelo menos 10 minutos antes do bloco de observabilidade.
- Abas já abertas e logadas: GitHub, SonarCloud, Mercado Pago (painel de testes), Grafana, Jaeger, Swagger/Postman.
- Aumentar o zoom do terminal e do navegador; narrar o que está na tela.
- Publicar como "não listado" e testar o link em uma janela anônima.

### 4.2 Documento do portal (PDF)

Criar `docs/entrega/fase4/template_entrega_portal_fiap.md` (e, se o grupo mantiver o LaTeX, `docs/reports/fase4/`) com:

| Seção | Conteúdo | Origem |
|---|---|---|
| Capa | Curso, turma 15SOAT, grupo 78, fase 4, data | Template da Fase 3 |
| Participantes | Nome e RM dos 5 integrantes | Template da Fase 3 (já pronto) |
| Repositórios | Links de todos os repositórios (serviços, Web, Lambda, plataforma, pai) | [Spec de separação](../microsservicos/fase4_spec_separacao_microsservicos.md) |
| Vídeo | Link do YouTube/Vimeo | Após a gravação |
| Arquitetura final | Diagrama com microsserviços, bancos e comunicação (REST e mensageria) | Spec de separação § 4.3, atualizado com as decisões finais |
| Saga | Estratégia (orquestração), diagrama de estados, tabela de compensações | [Spec da Saga](../saga/fase4_spec_saga_pattern.md) |
| Justificativa | Por que essa divisão; por que cada banco; por que essa mensageria e essas bibliotecas | Specs de separação, bancos e comunicação |

---

## 5. Passos para a mudança

1. Criar `docs/entrega/fase4/` com o roteiro (§ 4.1) e o template do PDF (§ 4.2).
2. Exportar os diagramas finais (Mermaid → PNG/SVG) para o PDF.
3. Escrever a justificativa da divisão e das tecnologias depois de fechar as decisões do índice.
4. Ensaio geral com o ambiente AWS no ar; ajustar o tempo de cada bloco.
5. Gravar, editar, publicar e testar o link.
6. Preencher o link do vídeo no PDF e no README do pai; gerar o PDF final e conferir os 6 itens obrigatórios.

---

## 6. Critérios de aceite

- [ ] Vídeo entre 12 e 15 minutos, público ou não listado, cobrindo os 4 itens do PDF e os 3 pontos do feedback da Fase 3.
- [ ] PDF com os 6 itens obrigatórios.
- [ ] Links do PDF testados (repositórios públicos ou com acesso para `soat-architecture`).
