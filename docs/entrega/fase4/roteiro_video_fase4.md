# Roteiro do vídeo — Fase 4 (alvo 13 a 14 min, limite 15 min)

> Base: [spec do vídeo e do documento](../../../.specs/features/fase4/especificacao/entregaveis/fase4_spec_video_e_documento_portal.md) § 4.1.
> Feedback da Fase 3 a responder: vídeo curto, implementação não mostrada, dashboards e proteção de branches não mostrados.
> Marque cada item ao ensaiar. Itens 🔲 dependem do ambiente AWS no ar ou de uma decisão do grupo.

## Antes de gravar (checklist)

| # | Verificação | Como |
|---|---|---|
| 1 | Serviços no ar | `scripts/smoke/rodar.sh` passa (C42 `aws-credenciais`, health dos 4 serviços) 🔲 |
| 2 | Gateway responde | `https://8afyzjqfk2.execute-api.us-east-1.amazonaws.com` com token válido 🔲 |
| 3 | Dados de demonstração | cliente, veículo e insumo com saldo; flag para provocar falha preparada |
| 4 | Mercado Pago em modo teste | painel de testes aberto; comprador de teste pronto; webhook com `422` na simulação |
| 5 | Tráfego contínuo | gerador rodando 10 min antes do bloco de monitoramento |
| 6 | Abas logadas | GitHub (Actions e Settings → Branches), SonarCloud, Grafana, Jaeger, Postman |
| 7 | Zoom do terminal e do navegador aumentado | narrar o que está na tela |

## Blocos

| Tempo | Bloco | Mostrar | Requisito |
|---|---|---|---|
| 00:00–01:00 | Abertura | Grupo 78, objetivo da fase, diagrama geral | — |
| 01:00–02:30 | Divisão e tecnologias | OS, Cobrança, Execução e Cadastros; PostgreSQL por serviço, DynamoDB da Saga, SQS; organização no GitHub | R45 a R47 |
| 02:30–04:00 | Implementação | `SagaOS` (máquina de estados), uma compensação, Outbox/idempotência por `eventId` | Feedback F3 |
| 04:00–07:00 | **Fluxo completo da OS** | abrir OS, orçamento, aprovação, reserva de insumos, execução, pagamento no sandbox, histórico | R38 |
| 07:00–09:00 | **Saga e falhas** | cenários do BDD (`AutoReparos.OS.BddTests`); cancelamento com compensação em ordem inversa; falta de saldo em `AguardandoInsumos`; console de Sagas e reprocessamento da DLQ | R39 |
| 09:00–11:00 | **Monitoramento** | Grafana (dashboards da Saga e dos serviços), Jaeger por serviço, logs no Loki | R41 |
| 11:00–13:30 | **Deploy automatizado com testes** | proteção da `main` e um PR bloqueado; pipeline (build, test, quality, imagem, Helm) e rollout; cobertura no SonarCloud | R40 |
| 13:30–14:00 | Encerramento | checklist dos requisitos | — |

## O que dizer em cada ponto sensível

- **Pagamento recusado:** hoje só registra no histórico e deixa a Saga aguardando; não compensa. Mostrar como está, sem prometer reembolso automático.
- **Falta de saldo na reserva:** a Saga fica em `AguardandoInsumos` com alerta; só o cancelamento compensa, e compensa apenas o orçamento.
- **Rastreamento:** o tracing mostra as chamadas de cada serviço. A propagação do contexto de trace entre mensagens SQS não foi entregue (decisão registrada); dizer isso em uma frase se a banca perguntar.

## Depois de gravar

- Editar, publicar como **não listado** e abrir o link em janela anônima.
- Conferir a duração (12 a 15 min) e preencher o link no PDF e no README do pai.
- Só então destruir o ambiente AWS (`destroy-aws.yml`).
