# Roteiro do vídeo — Fase 4 (alvo 13 a 14 min, limite 15 min)

> Base: [spec do vídeo e do documento](../../../.specs/features/fase4/especificacao/entregaveis/fase4_spec_video_e_documento_portal.md) § 4.1.
> Feedback da Fase 3 a responder: vídeo curto, implementação não mostrada, dashboards e proteção de branches não mostrados.
> Marque cada item ao ensaiar. Itens 🔲 dependem do ambiente AWS no ar ou de uma decisão do grupo.

## Antes de gravar (checklist)

| # | Verificação | Como |
|---|---|---|
| 1 | Perfil AWS na máquina de quem grava | `aws login --profile autoreparos` e `aws sts get-caller-identity --profile autoreparos` mostrando a conta `198903849859` 🔲 |
| 2 | Cluster acessível | `aws eks update-kubeconfig --name AutoReparosEKS --region us-east-1 --profile autoreparos` e `kubectl get pods -A` com os namespaces `os`, `cobranca`, `execucao`, `cadastros` e `autoreparos` 🔲 |
| 3 | Serviços no ar | `scripts/smoke/rodar.sh` passa (C42 `aws-credenciais`, health dos 4 serviços) 🔲 |
| 4 | Gateway responde | `https://8afyzjqfk2.execute-api.us-east-1.amazonaws.com` com token válido 🔲 |
| 5 | Dados de demonstração | cliente, veículo e insumo com saldo; flag para provocar falha preparada |
| 6 | Mercado Pago em modo teste | pagamento com um **comprador de teste criado no painel** (não o vendedor); o webhook automático ainda não chega sozinho, então usar "Simular notificação" no painel com o id real do pagamento |
| 7 | Grafana por port-forward | o serviço é ClusterIP: `kubectl port-forward -n autoreparos svc/<grafana> 3000:80`; a senha de admin está no SSM em `/autoreparos/production/plataforma/grafana-admin-password` 🔲 |
| 8 | Tráfego contínuo | gerador rodando 10 min antes do bloco de monitoramento |
| 9 | Abas logadas | GitHub (Actions e Settings → Branches), SonarCloud, Grafana, Jaeger, Postman |
| 10 | Zoom do terminal e do navegador aumentado | narrar o que está na tela |
| 11 | Ambiente estável | sem merge na `main` de nenhum repositório durante a gravação (cada merge dispara um deploy que espera aprovação no Environment `production`) |

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
- **Deploy:** o job `deploy` espera a aprovação do Environment `production` (revisores: José e Enrico); mostrar essa aprovação no vídeo. Se o `/health` da versão nova falhar, o Helm volta sozinho para a anterior e o job fica vermelho (rollback automático).
- **Pagamento:** a notificação do Mercado Pago precisa ser simulada no painel com o id real do pagamento; dizer isso em uma frase em vez de esperar o webhook.
- **Rastreamento:** o tracing mostra as chamadas de cada serviço. A propagação do contexto de trace entre mensagens SQS não foi entregue (decisão registrada); dizer isso em uma frase se a banca perguntar.

## Depois de gravar

- Editar, publicar como **não listado** e abrir o link em janela anônima.
- Conferir a duração (12 a 15 min) e preencher o link no PDF e no README do pai.
- Só então destruir o ambiente AWS (`destroy-aws.yml`).
