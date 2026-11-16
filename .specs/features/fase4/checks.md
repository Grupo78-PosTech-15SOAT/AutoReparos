# Fase 4 checks

Mutations: 3
Build: none - iniciativa de especificação; os comandos de build serão definidos nos planos dos microsserviços
Plan: `.specs/features/fase4/plan.md`

17 checks · 4 slices · 0 one-way doors · 2 open questions, 0 blocking

## Checks

### S1 - Microsserviços e responsabilidades

**C1** - A entrega contém no mínimo 3 microsserviços independentes, cada um com repositório, infraestrutura e banco próprios (AC 1)
Proof: walk - inspecionar os 3 repositórios, respectivos diretórios IaC e configurações de banco

**C2** - A integração entre serviços ocorre por APIs ou mensagens; nenhum serviço lê ou grava diretamente no banco de outro serviço (AC 2)
Proof: walk - revisar conexões, permissões e chamadas entre os 3 serviços

**C3** - OS Service permite abrir, atualizar e consultar status e histórico da OS; Billing gera e envia orçamento para aprovação, registra/verifica pagamento e atualiza o status da OS; Execução mantém fila, diagnóstico e reparos e comunica a finalização ao OS Service (AC 3)
Proof: walk - executar o fluxo funcional ponta a ponta e consultar cada operação e histórico

### S2 - Persistência, comunicação e consistência distribuída

**C4** - A arquitetura inclui ao menos um banco SQL e um NoSQL entre os serviços (AC 4)
Proof: walk - inspecionar os recursos e configurações de persistência implantados

**C5** - Integrações definem APIs REST síncronas onde necessário e AWS SQS para eventos e comunicação assíncrona (AC 5)
Proof: walk - revisar contratos REST e publicar/consumir eventos em SQS no fluxo principal

**C6** - A Saga persistida é orquestrada pelo OS Service, mantém estado em DynamoDB e usa SQS para comandos/eventos; README documenta a escolha e justificativa (AC 6)
Proof: walk - acompanhar estado da Saga em DynamoDB e revisar a justificativa no README

**C7** - Falha em cada etapa compensável da Saga executa a compensação aplicável e deixa o estado final consistente (AC 7)
Proof: walk - injetar falha em cada etapa compensável e verificar efeitos e estado persistido

**C8** - Billing Service cria pagamento via Mercado Pago Checkout Pro e atualiza o estado por Webhook validado (AC 8)
Proof: walk - realizar checkout de teste e processar Webhook válido e inválido

### S3 - Testes, qualidade e entrega contínua

**C9** - Cada um dos 3 serviços possui testes unitários e cobertura de linhas mínima de 80%, aplicada como gate por serviço (AC 9)
Proof: walk - executar os testes e verificar relatório e gate de cobertura de cada serviço

**C10** - Existe ao menos um teste BDD completo que percorre o fluxo distribuído principal (AC 10)
Proof: walk - executar o cenário Reqnroll ponta a ponta

**C11** - A CI executa SonarQube ou ferramenta equivalente e publica o resultado de qualidade (AC 11)
Proof: walk - executar a pipeline e inspecionar a etapa e o resultado de qualidade

**C12** - A pipeline de cada repositório executa build, testes, qualidade e deploy automatizado em Kubernetes para alterações elegíveis (AC 12)
Proof: walk - executar pipeline de cada serviço e verificar sucesso das quatro etapas

**C13** - A branch `main` de cada repositório exige Pull Request e checagens automáticas antes do merge (AC 13)
Proof: walk - inspecionar proteção de branch e checagens obrigatórias nos 3 repositórios

### S4 - Observabilidade e pacote de entrega

**C14** - O fluxo de OS é monitorado e rastreado entre serviços com ferramentas reutilizadas/adaptadas da Fase 3 (AC 14)
Proof: walk - processar uma OS e localizar métricas, logs e trace correlacionado entre serviços

**C15** - Cada repositório entregue contém código-fonte, Dockerfile, manifestos Kubernetes, pipeline CI/CD, evidência de cobertura no README, documentação de arquitetura e Swagger ou collection Postman atualizada (AC 15)
Proof: walk - inventariar os sete itens em cada repositório entregue

**C16** - O vídeo publicado no YouTube ou Vimeo é público ou não listado, tem até 15 minutos e demonstra fluxo completo de OS, Saga e falha, deploy de ao menos um serviço com validação de testes, monitoramento e rastreamento distribuído (AC 16)
Proof: walk - abrir o vídeo publicado, conferir duração/visibilidade e percorrer cada demonstração requerida

**C17** - O PDF final submetido ao portal contém participantes, links dos repositórios e vídeo, diagrama de serviços/bancos/comunicação, estratégia e justificativa da Saga e justificativas da divisão e tecnologias (AC 17)
Proof: walk - conferir PDF submetido e seus sete elementos requeridos

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| arquitetura e responsabilidades, AC 1-3 (3) | repositórios/infra/bancos C1 · isolamento de dados C2 · operações dos três serviços C3 | - |
| persistência/comunicação/Saga/pagamento, AC 4-8 (5) | SQL+NoSQL C4 · REST+SQS C5 · orquestração/estado/README C6 · compensação C7 · Mercado Pago C8 | - |
| testes/qualidade/entrega, AC 9-13 (5) | cobertura por serviço C9 · BDD C10 · qualidade CI C11 · pipeline/deploy C12 · proteção main C13 | - |
| observabilidade e entregáveis, AC 14-17 (4) | rastreabilidade C14 · pacote de repositório C15 · vídeo C16 · PDF C17 | - |
| decisões de ciclo de vida confirmadas no plano (6) | pagamento somente após conclusão C3 · pagamento antes da entrega C3 · nova tentativa manual sem cobrança automática C8 · recuperação para frente após captura C8 · reembolso com aprovação e auditoria C8 · recusa/cancelamento explícitos sem expiração automática C3 | - |
| infraestrutura confirmada no plano (4) | AWS/EKS e observabilidade C1,C14 · staging/produção nos alvos aprovados C12 · recursos exclusivos por serviço C1 · RDS PostgreSQL com database/usuário isolados por serviço C1 | - |
| regras operacionais confirmadas no plano (2) | falha de execução requer intervenção/reexecução manual C3 · Saga usa OS Service+DynamoDB+SQS C6 | - |

## Swept

- validation: C2, C8
- failure modes: C7, C8, C12
- idempotency: C8 - Webhooks repetidos e tentativas de cobrança serão detalhados na spec Billing
- authorization: C2, C13
- concurrency: C5, C6 - ordenação/deduplicação serão especificadas por serviço
- data lifecycle: C6 - retenção e reconciliação serão detalhadas na spec Saga
- dependency failure: C7, C8
- state transitions: C3, C6, C7, C8
- observability: C14

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** checks derivados da iniciativa fase4; nenhuma implementação autorizada por este artefato
- **Abandoned:** none
- **Walked:** none - checks definem evidências de aceite futuras; não há aplicação Fase 4 implementada neste passo
