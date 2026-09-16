# Roteiro do Vídeo Demonstrativo — 15 minutos

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica
> **Entrega:** Tech Challenge FIAP SOAT — Fase 3 — Grupo 78
> **Duração máxima:** 15 minutos (limite da banca)
> **Documentos de apoio:** [RFC-003](../architecture/RFC-003-serverless-client-authentication.md) · [ADR-002](../architecture/ADR-002-data-isolation-and-zero-trust-claims.md) · [ADR-003](../architecture/ADR-003-end-to-end-observability-strategy.md) · [Modelo de dados](../architecture/database-selection-and-data-model.md)

---

## Antes de gravar — checklist de 10 minutos

Cada item abaixo já falhou pelo menos uma vez em validação. Conferir antes evita
interromper a gravação no meio.

| # | Verificação | Comando / onde |
|---|---|---|
| 1 | Docker Engine respondendo | `docker info` deve retornar em segundos, não travar |
| 2 | Stack no ar | `docker compose up -d` e `curl -i http://localhost:8080/health` → `200` |
| 3 | Grafana com os 4 dashboards | `http://localhost:3000` (admin/admin) → pasta *AutoReparos* |
| 4 | **Tráfego contínuo rodando** | ver script abaixo — sem isso os painéis aparecem vazios ou mentem |
| 5 | Token válido em mãos | `POST /api/auth/login` com o usuário de seed |
| 6 | New Relic recebendo (se for mostrar) | `.env` com a license key e `OTEL_VENDOR_CONFIG_ARG` |
| 7 | Abas abertas e logadas | GitHub Actions, Grafana, Jaeger, New Relic |

### O gerador de tráfego (deixe rodando em um terminal oculto)

Este é o item mais importante da lista. As métricas são exportadas em ciclos de
**60 segundos**, e as expressões dos painéis usam janela de `[5m]`. Sem tráfego
contínuo, os painéis ficam vazios — e o painel de taxa de erro chega a exibir **100%**,
porque as requisições bem-sucedidas saem da janela antes dos erros.

```bash
TOKEN="<token do login>"
while true; do
  curl -s -o /dev/null -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8080/api/clientes?PageNumber=1&PageSize=10"
  curl -s -o /dev/null -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8080/api/ordem-servico/kanban"
  sleep 2
done
```

Inicie **pelo menos 10 minutos antes** de gravar o bloco 5, para que a janela de 5
minutos esteja cheia quando os painéis entrarem em tela.

---

## [00:00 – 02:00] Apresentação e arquitetura

**Objetivo:** situar a banca no problema e na topologia, sem entrar em código.

| Tempo | Conteúdo | Tela |
|---|---|---|
| 00:00 | Nome do grupo, integrantes e RMs | Slide de capa |
| 00:25 | O problema: oficina com controle manual, sem histórico nem visibilidade de estoque | Slide |
| 00:50 | A evolução nas 3 fases: DDD → nuvem/observabilidade → serverless e multi-repo | Slide |
| 01:15 | **Os 4 repositórios no GitHub**, um por responsabilidade | Organização no GitHub |
| 01:40 | Diagrama de componentes: Portal → API Gateway → {Lambda, EKS} → RDS | Diagrama |

**Fale enquanto mostra a organização no GitHub:**

> "A Fase 3 exigiu desacoplar o que era um repositório único em quatro repositórios
> independentes, cada um com seu próprio ciclo de vida e sua própria esteira de CI. O
> repositório pai orquestra os quatro por submódulos Git, o que permite clonar o
> ecossistema inteiro com um comando e ainda assim versionar cada peça separadamente."

**Não gaste tempo aqui.** Este bloco é contexto; os pontos avaliados estão nos blocos 3
a 6. Se estourar, corte da apresentação pessoal, não da topologia.

---

## [02:00 – 05:00] CI/CD e deploy automatizado

**Objetivo:** provar que a esteira existe, roda e é a que publica.

| Tempo | Conteúdo | Tela |
|---|---|---|
| 02:00 | Aba *Actions* de um dos repositórios, com execuções verdes no histórico | GitHub Actions |
| 02:30 | Abrir uma execução e percorrer os estágios: build → testes → terraform → deploy | Detalhe do run |
| 03:15 | **Os testes na esteira**: abrir o log do job e mostrar o total | Log do job |
| 04:00 | `terraform plan`/`apply` no log da infraestrutura | Log do job |
| 04:30 | Fechar dizendo o que o merge em `main` dispara automaticamente | GitHub Actions |

**Ao mostrar os testes, diga o número e mostre a soma:**

> "São **354 testes** automatizados: 113 de domínio, 40 da Lambda, 134 de aplicação e 67
> de integração rodando contra um PostgreSQL real em Testcontainers. Todos passando."

**Se a esteira não estiver verde no dia:** mostre um run anterior bem-sucedido e diga
com transparência que o run atual está em andamento. É melhor do que exibir vermelho
sem explicação.

---

## [05:00 – 07:30] Autenticação serverless com CPF

**Objetivo:** demonstrar a função serverless — requisito explícito da fase.

> ⚠️ **Escolha a variante antes de gravar.** A variante A depende do Terraform do Track A
> estar aplicado na AWS. Se não estiver, use a variante B sem hesitar: o que a banca
> avalia é a lógica serverless e o JWT efêmero, e ambos são idênticos nos dois caminhos.

### Variante A — na AWS (preferencial)

| Tempo | Conteúdo |
|---|---|
| 05:00 | Console AWS: a Lambda `AutoReparos.AuthLambda` e a rota `POST /auth/cliente` no API Gateway v2 |
| 05:40 | Chamada real via Postman/curl com CPF e e-mail de um cliente cadastrado → `200` + JWT |
| 06:20 | Colar o token em `jwt.io` e mostrar as claims, com destaque para `role: Cliente` e `exp` |
| 06:50 | Os três caminhos de erro (abaixo) |

```bash
curl -i -X POST https://<api-gateway>/auth/cliente \
  -H "Content-Type: application/json" \
  -d '{"cpf":"<cpf-cadastrado>","email":"<email-cadastrado>"}'
```

### Variante B — local (contingência)

Idêntica, trocando a URL por `http://localhost:8080`. Diga em uma frase:

> "A função roda em Lambda na AWS, atrás do API Gateway; para esta demonstração estou
> chamando o mesmo código localmente."

### Os três erros — grave sempre, em qualquer variante

Esta é a parte que diferencia a demonstração, porque mostra decisão de projeto:

| Entrada | Resposta | O que dizer |
|---|---|---|
| CPF `111.111.111-11` | `400 CPF inválido` | "Validado por módulo 11 **antes** de qualquer consulta — um CPF inválido nunca chega ao banco" |
| CPF válido + e-mail errado | `401 Cliente não localizado ou dados divergentes` | "Mensagem **idêntica** à de CPF inexistente, para não virar um oráculo de enumeração de cadastro" |
| Cliente inativado | `403 Cadastro inativo` | "A Lambda lê a coluna `InativoEm` e bloqueia no login" |

---

## [07:30 – 10:00] Rotas protegidas e isolamento Zero-Trust

**Objetivo:** provar que um cliente só enxerga os próprios dados — o ponto mais forte
tecnicamente de toda a demonstração.

| Tempo | Conteúdo | Resultado esperado |
|---|---|---|
| 07:30 | `GET /api/clientes/meus-veiculos` com o token do cliente | `200` com os veículos **daquele** cliente |
| 08:00 | `GET /api/ordem-servico/minhas-os` | `200` com as OS do titular |
| 08:30 | **Filtrar por placa de outro cliente** | `200` com `[]` |
| 09:10 | `GET /api/clientes` (rota operacional) com o mesmo token | **`403 Forbidden`** |
| 09:40 | Mostrar o código de `PortalClienteController` | O `ClienteId` vem da claim |

**O momento de 08:30 precisa ser explicado, senão parece um bug:**

> "Filtrei por uma placa que não é deste cliente e a API devolveu lista vazia com
> **200** — não 403, não 404. É deliberado: um 403 significaria 'existe, mas não é seu',
> e um 404 'não existe'. Essa diferença já seria um vazamento, porque permitiria varrer
> placas e descobrir quais veículos a oficina atende. Colapsando os dois casos na mesma
> resposta, a API não confirma nem nega a existência do registro."

**Em 09:40, mostre o trecho em tela:**

```csharp
var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
```

> "O identificador do titular vem exclusivamente da claim do token assinado. Não existe
> parâmetro na requisição capaz de alterá-lo — trocar um GUID na URL não muda nada."

Se sobrar tempo, cite que há **5 testes de integração** cobrindo exatamente estes
cenários, inclusive o `403` e a lista vazia.

---

## [10:00 – 13:00] Dashboards de monitoramento

**Objetivo:** os 3 painéis mandatórios, ao vivo, com dados reais.

> Confirme que o gerador de tráfego está rodando há pelo menos 10 minutos.

| Tempo | Painel | O que destacar |
|---|---|---|
| 10:00 | Grafana → pasta *AutoReparos*, 4 dashboards | "Provisionados automaticamente, não importados à mão" |
| 10:20 | **Volume Diário de OS** | Total em 30 dias, hoje, e a série por status |
| 11:10 | **Tempo Médio por Status** | Diagnóstico, execução e permanência total |
| 12:00 | **Erros e Falhas nas Integrações** | Falhas de notificação, 5xx por rota, latência de banco |

**Ao abrir a pasta, em 10:00:**

> "Os dashboards não são importados manualmente: os JSON vivem no repositório e são
> provisionados por ConfigMap no EKS e por bind mount no ambiente local — a mesma fonte
> para os dois ambientes, então eles não divergem."

**Em 12:00, sobre a origem das métricas de negócio:**

> "Estas métricas não são incrementadas dentro dos casos de uso. Elas saem de um
> interceptor do Entity Framework que lê as transições no momento da persistência, o
> que garante que nenhum caminho de código consiga mudar o status de uma OS sem
> contabilizar."

### Opcional, se tiver margem: forçar um erro ao vivo

Rende bem, mas custa ~40 segundos e exige esperar o ciclo de exportação. **Só faça se
estiver adiantado no tempo.**

```bash
docker compose stop postgres
# algumas requisições → 500
docker compose start postgres
```

Validado: **500 em 18/18 requisições**, com os painéis de 5xx reagindo.

> ⚠️ **Cuidado com o painel de taxa de erro.** Ele mede a razão dentro da janela de 5
> minutos. Com tráfego esparso ele exibe **100%** — correto aritmeticamente, mas parece
> uma falha total. Com o gerador rodando, ele se estabiliza em torno de 25%.

---

## [13:00 – 15:00] Logs, traces e APM

**Objetivo:** fechar mostrando a correlação ponta a ponta.

| Tempo | Conteúdo | Tela |
|---|---|---|
| 13:00 | Log JSON da API, com `TraceId` e `SpanId` em cada linha | `docker compose logs api` |
| 13:30 | Copiar um `TraceId` e buscá-lo no Jaeger | Jaeger |
| 14:00 | Abrir o trace: HTTP → EF Core → Npgsql, com as durações | Jaeger |
| 14:30 | **New Relic** recebendo os mesmos sinais | New Relic |
| 14:50 | Frase de encerramento | — |

**Em 13:00:**

> "Todo log sai em JSON estruturado com `TraceId` e `SpanId`, sem nenhuma instrumentação
> manual nos handlers — vem do `ActivityTrackingOptions` do ASP.NET Core."

**Em 14:30, o requisito de APM de mercado:**

> "Os mesmos sinais vão para o New Relic. A aplicação não sabe disso: ela exporta OTLP
> para o Collector, e é o Collector que decide o destino. Trocar New Relic por Datadog é
> mudar um bloco de configuração, sem tocar em uma linha de código."

**Encerramento (14:50):**

> "Quatro repositórios independentes com CI própria, autenticação serverless sem
> cadastro, isolamento Zero-Trust dos dados do cliente, três dashboards de negócio e
> rastreamento distribuído exportado de forma vendor-agnostic. Obrigado."

---

## Plano B por bloco

| Se falhar | Faça |
|---|---|
| Docker não sobe | Grave os blocos 1, 2 e 5 com material pré-gravado; deixe claro que é replay |
| Lambda na AWS indisponível | Variante B (local), com a frase de transição pronta |
| Painel vazio | Verifique o gerador de tráfego e amplie a janela do painel para 15 minutos |
| New Relic sem dados | Mostre Jaeger e Loki; cite que o Collector exporta para o vendor e mostre o log de envio |
| Estourando o tempo | Corte o bloco 1 para 1 minuto e o erro ao vivo do bloco 5 |

## Distribuição do tempo — resumo

| Bloco | Janela | Peso |
|---|---|---|
| 1. Apresentação e arquitetura | 00:00–02:00 | Contexto |
| 2. CI/CD | 02:00–05:00 | Requisito |
| 3. Autenticação serverless | 05:00–07:30 | **Requisito central** |
| 4. Rotas protegidas e Zero-Trust | 07:30–10:00 | **Requisito central** |
| 5. Dashboards | 10:00–13:00 | **Requisito central** |
| 6. Logs, traces e APM | 13:00–15:00 | **Requisito central** |

Os quatro blocos centrais somam 10 dos 15 minutos. Ao cortar, corte dos dois primeiros.
