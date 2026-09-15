# Plano Detalhado de Implementação — Subfase 1: Telemetria de Negócio & Métricas Customizadas (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** Instrumentação de Métricas de Negócio em .NET 10 via `System.Diagnostics.Metrics` e EF Core Interceptor  
> **Repositório Alvo:** [`submodules/AutoReparos.App`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.App)  
> **Documento Mestre:** [`../plano_execucao_track_b_observabilidade.md`](../plano_execucao_track_b_observabilidade.md)  
> **Status:** Concluído & Auditado  

---

## 1. Contexto de Negócio & Justificativa Técnica

### 1.1. O Problema das Métricas Operacionais
Em sistemas legados de oficinas mecânicas, métricas de volume de atendimento, tempo de permanência de veículos e taxas de falha em notificações são frequentemente computadas através de queries analíticas periódicas (batch) executadas diretamente contra as tabelas transacionais do banco de dados relacional.
- **Sobrecarga de I/O e CPU:** Consultas com agregações temporais pesadas (`GROUP BY`, `DATE_TRUNC`) competem por locks e buffers com as operações transacionais de escrita (criação de OS, baixa de estoque).
- **Latência na Tomada de Decisão:** Informações em batch não oferecem visibilidade em tempo real para alarmes operacionais imediatos.
- **Risco de Inconsistência:** Transações abortadas podem ser incorretamente computadas se o log não for transacionalmente consistente.

### 1.2. A Abordagem Baseada em Eventos do EF Core ChangeTracker
Para atender aos requisitos mandatórios da Fase 3 com máxima eficiência, a equipe implementou uma solução orientada a eventos usando **`System.Diagnostics.Metrics`** do .NET 10 e um **`SaveChangesInterceptor`** no Entity Framework Core:
- **Zero Overhead Transacional:** As métricas são capturadas a partir das alterações rastreadas na memória pelo `ChangeTracker` do EF Core durante o ciclo do `DbContext`.
- **Garantia de Commit Real:** As métricas de volume e duração só são despachadas para o `Meter` no método `SavedChangesAsync`, isto é, **estritamente após o commit bem-sucedido no PostgreSQL**. Transações que falharem (`SaveChangesFailedAsync`) têm seus registros em buffer descartados imediatamente, eliminando métricas fantasmas.
- **Centralização Arquitetural:** Em vez de poluir mais de 9 casos de uso individuais com chamadas a contadores de telemetria, toda a lógica de emissão reside em um único componente de infraestrutura (`OrdemServicoMetricsInterceptor`).

---

## 2. Matriz de Decisões Técnicas Alinhadas

| Dimensão | Decisão Adotada | Racional Técnico | Alternativa Descartada |
|:---|:---|:---|:---|
| **Instrumentação de Métricas** | `System.Diagnostics.Metrics` nativo do .NET 10 | Padrão oficial e de alta performance do .NET, com suporte nativo a OpenTelemetry SDK sem dependência de bibliotecas de terceiros no Domain/Application. | Bibliotecas proprietárias (ex: Datadog DogStatsD ou New Relic Agent SDK) que causariam acoplamento vendor-lock-in. |
| **Ponto de Captura de Transições** | `SaveChangesInterceptor` do EF Core | Intercepta estados `EntityState.Added` e `EntityState.Modified` da entidade `OrdemServico` de maneira transparente a todos os use cases. | Disparo manual dentro de cada Use Case (`CriarOSUseCase`, `AprovarOSUseCase`, etc.), sujeito a esquecimento humano em novas rotas. |
| **Garantia Anti-Phantom Metrics** | Separação entre `Capturar()` em `SavingChanges` e `Emitir()` em `SavedChanges` | Evita registrar volume de OS criada se o commit falhar por violação de constraint ou erro de rede. | Emitir o incremento do contador antes de salvar no banco de dados. |
| **Métricas de Integração Externa** | Bloco try-catch explícito no `NotificacaoService.cs` | Captura tanto exceções de rede (`HttpRequestException`, `TimeoutException`) quanto códigos de erro HTTP retornados pela API do SendGrid (`401`, `429`, `500`). | Interceptor genérico de HttpClient que misturaria chamadas do SendGrid com outras integrações internas. |

---

## 3. Especificação Técnica da Implementação

### 3.1. Definição Central de Métricas (`AutoReparosMetrics.cs`)
Localização: `submodules/AutoReparos.App/AutoReparos.Application/Shared/Metrics/AutoReparosMetrics.cs`

A classe declara o meter `AutoReparos.BusinessMetrics` e expõe instrumentos tipados:
1. `notificacoes.falhas` (`Counter<long>`): Contabiliza falhas no envio de e-mails para clientes. Tags: `canal` (email), `tipo` (orcamento, atualizacao_status), `motivo` (nome da exceção), `status_code` (código HTTP SendGrid).
2. `ordens_servico.criadas` (`Counter<long>`): Contabiliza ordens de serviço geradas. Tag: `status`.
3. `ordens_servico.transicoes_status` (`Counter<long>`): Registra cada mudança de estado da OS no Kanban operacional. Tag: `status` de destino.
4. `ordens_servico.tempo_diagnostico` (`Histogram<double>` em segundos): Duração entre `DiagnosticoIniciadoEm` e `EnvioAprovacaoEm`.
5. `ordens_servico.tempo_execucao` (`Histogram<double>` em segundos): Duração entre `IniciadoEm` e `FinalizadoEm`.
6. `ordens_servico.tempo_permanencia` (`Histogram<double>` em segundos): Duração total entre `CriadoEm` e `EntregueEm`.

### 3.2. Interceptor de Ordens de Serviço (`OrdemServicoMetricsInterceptor.cs`)
Localização: `submodules/AutoReparos.App/AutoReparos.Infra/Data/Interceptors/OrdemServicoMetricsInterceptor.cs`

Fluxo de execução:
1. **`SavingChangesAsync`**: Varre `ChangeTracker.Entries<OrdemServico>()`.
   - Se `State == Added`: Adiciona à lista `_pendentes` com `Criada = true` e o status inicial (`Recebida`).
   - Se `State == Modified`: Avalia a propriedade `Status`. Se modificada, calcula os tempos correspondentes:
     - `AguardandoAprovacao`: Calcula tempo de diagnóstico (`EnvioAprovacaoEm - DiagnosticoIniciadoEm`).
     - `Finalizada`: Calcula tempo de execução (`FinalizadoEm - IniciadoEm`).
     - `Entregue`: Calcula tempo de permanência total (`EntregueEm - CriadoEm`).
2. **`SavedChangesAsync`**: Itera sobre os itens em `_pendentes`, incrementa os contadores e registra as amostras nos histogramas correspondentes. Limpa a lista.
3. **`SaveChangesFailedAsync`**: Limpa `_pendentes` sem emitir nenhuma métrica.

### 3.3. Tratamento Resiliente no Serviço de Notificações (`NotificacaoService.cs`)
Localização: `submodules/AutoReparos.App/AutoReparos.Infra/Services/NotificacaoService.cs`

Nos métodos `EnviarOrcamento` e `EnviarAtualizacaoStatus`:
- Envolve a chamada `client.SendEmailAsync(msg)` em bloco `try-catch`.
- Se a resposta HTTP for diferente de sucesso (`!response.IsSuccessStatusCode`):
  - Registra log de aviso: `LogWarning`.
  - Incrementa `AutoReparosMetrics.NotificacoesFalhas` com tag `status_code = (int)response.StatusCode`.
- Se ocorrer exceção (`catch (Exception ex)`):
  - Registra log de erro: `LogError`.
  - Incrementa `AutoReparosMetrics.NotificacoesFalhas` com tag `motivo = ex.GetType().Name`.
- A falha de notificação **não interrompe a transação da OS**, garantindo que o fluxo mecânico não trave por indisponibilidade temporária de e-mail.

---

## 4. Matriz de Testes Automatizados & Validação Empírica

### 4.1. Suíte de Testes Unitários de Métricas
Localização: `submodules/AutoReparos.App/AutoReparos.Application.Tests/Shared/OrdemServicoMetricsInterceptorTests.cs`

A suíte utiliza um `MeterListener` real acoplado ao `AutoReparosMetrics.Meter` com a anotação `[Collection("BusinessMetrics", DisableParallelization = true)]` para prevenir race conditions de telemetria entre testes paralelos:

| Caso de Teste | Cenário Avaliado | Resultado Esperado |
|:---|:---|:---:|
| `Deve_ContabilizarOrdemCriada_QuandoUmaNovaOrdemEPersistida` | Criação e persistência de nova OS via `SaveChanges()`. | `ordens_servico.criadas` == 1 com tag `status=Recebida`. |
| `Deve_ContabilizarTransicaoDeStatus_QuandoOrdemEMovida` | Transição de `Recebida` para `EmDiagnostico`. | `ordens_servico.transicoes_status` == 1 com tag `status=EmDiagnostico`. |
| `Deve_IgnorarModificacao_QuandoStatusNaoMuda` | Alteração apenas na observação da OS. | Nenhuma métrica de transição ou criação emitida. |
| `Deve_RegistrarTempoDiagnostico_QuandoEnviadaParaAprovacao` | OS com carimbos válidos movida para `AguardandoAprovacao`. | Amostra registrada no histograma `ordens_servico.tempo_diagnostico`. |
| `Deve_RegistrarCicloCompleto_AteEntrega` | Ciclo completo: Recebida ➔ Diagnóstico ➔ Aprovação ➔ Execução ➔ Finalizada ➔ Entregue. | Amostras únicas em `tempo_diagnostico`, `tempo_execucao` e `tempo_permanencia`. |
| `Deve_DescartarPendencias_QuandoCommitFalha` | Simulação de erro em `SaveChangesFailedAsync`. | Nenhuma métrica emitida e lista `_pendentes` esvaziada. |

### 4.2. Resultados da Execução
```bash
dotnet test submodules/AutoReparos.App/AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj --filter "FullyQualifiedName~OrdemServicoMetricsInterceptorTests"
# Resultado: 6 passed, 0 failed, 0 skipped. Duração: 240 ms.
```

---

## 5. Análise de Riscos, Mitigações e Procedimento de Rollback

| Risco Técnico Identificado | Severidade | Probabilidade | Mitigação Arquitetural | Procedimento de Rollback |
|:---|:---:|:---:|:---|:---|
| Concorrência de escrita na lista interna `_pendentes` | Alta | Baixa | Instanciação de novo `OrdemServicoMetricsInterceptor` por escopo do `DbContext` em `DependencyInjectionInfra.cs` (`options.AddInterceptors(new OrdemServicoMetricsInterceptor())`). | Se detectado memory leak, converter `Pendente` para estrutura imutável no `HttpContext.Items`. |
| Falha silenciosa do SendGrid sem incrementar métricas | Média | Baixa | Tratamento duplo em `NotificacaoService.cs` checando tanto `response.IsSuccessStatusCode` quanto capturando `Exception`. | Reverter para log estruturado simples e reavaliar tags do SDK SendGrid. |
| Overhead de gravação em alta volumetria de transições | Baixa | Média | Histogramas e contadores utilizam structs leves em memória nativa do runtime .NET 10 sem alocação de heap. | N/A (Impacto insignificante comprovado em testes de carga). |

---

## 6. Critérios de Aceite & Definition of Done (DoD)

- [x] Classe `AutoReparosMetrics` declarada sob o meter `AutoReparos.BusinessMetrics` com todos os 5 instrumentos mandatórios.
- [x] `OrdemServicoMetricsInterceptor` registrado nas opções do `AppDbContext` e emitindo métricas estritamente após commits reais.
- [x] Tratamento de falhas SendGrid instrumentado em `NotificacaoService.cs` com tags `motivo`, `canal` e `status_code`.
- [x] Suíte de testes unitários do interceptor com 6 cenários aprovada com 100% de sucesso.
- [x] Zero chamadas manuais a métricas espalhadas nos use cases de aplicação (Clean Architecture preservada).
