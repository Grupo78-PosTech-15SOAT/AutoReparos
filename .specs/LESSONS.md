# LESSONS - auto-maintained by scripts/lessons.py

> Machine-owned. Do NOT hand-edit. Changes are overwritten on the next `lessons.py` write.
> Canonical state lives in `.specs/lessons.json`. Edit lessons only via the script.
> promote_threshold=2 distinct features · window_days=45 · quarantine_threshold=2

## Confirmed (load these at Plan/Checks)

Corroborated across multiple features. Safe to apply as guidance.

_none_

## Candidates (under observation - do NOT load as guidance yet)

Seen once or not yet corroborated. Tracked, not trusted.

### L-001 - Autorização por papel: teste cada papel aceito (Administrador, Atendente, Mecanico), não só um; remover um papel da política deve falhar um teste.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `api/auth` · harmful: 0
- features: fase4/cadastros
- evidence: C2 ConsultaCadastro (api/auth)
- last seen: 2026-11-12T19:23:21Z

### L-002 - Validação de JWT: além da chave errada, teste token assinado com a chave certa e emissor estrangeiro; sem isso ValidateIssuer=false passa despercebido.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `api/auth` · harmful: 0
- features: fase4/cadastros
- evidence: C3 ValidateIssuer (api/auth)
- last seen: 2026-11-12T19:23:21Z

### L-003 - Comparações de identidade (e-mail, CPF): teste caixa diferente e espaços nas pontas, porque os chamadores reais normalizam de formas distintas.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `application/clientes` · harmful: 0
- features: fase4/cadastros
- evidence: C5 AutenticarClienteUseCase (application/clientes)
- last seen: 2026-11-12T19:23:21Z

### L-004 - Idempotência por eventId exige teste de entregas simultâneas com atraso no envio; Existe-enviar-Registrar sem trava duplica e-mails e só o índice único protege o registro.
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `application/notificacoes` · harmful: 0
- features: fase4/cadastros
- evidence: C7 ProcessarEventoNotificacaoUseCase (application/notificacoes)
- last seen: 2026-11-12T19:23:21Z

### L-005 - Consumidor SQS com backoff em linha: defina VisibilityTimeout cobrindo as esperas e leia uma mensagem por vez, senão a reentrega quebra o 'uma vez por eventId' e a DLQ.
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `infrastructure/sqs` · harmful: 0
- features: fase4/cadastros
- evidence: C8 SqsFilaNotificacoes (infrastructure/sqs)
- last seen: 2026-11-12T19:23:21Z

### L-006 - Critérios de rede ('fora do cluster') precisam dizer qual camada prova o quê: aplicação prova a chave, o chart prova a entrada; NetworkPolicy depende do CNI e API Gateway com catch-all expõe qualquer rota do ALB.
- signal: `spec_precision_gap` · recurrence: 1 feature(s) · scope: `infra/rede` · harmful: 0
- features: fase4/cadastros
- evidence: C6 AC6 (infra/rede)
- last seen: 2026-11-12T19:23:22Z

### L-007 - Renomear banco gerenciado recria a instância: o plano deve dizer se o rename vale no RDS ou só no ambiente local antes de virar critério.
- signal: `spec_precision_gap` · recurrence: 1 feature(s) · scope: `infra/database` · harmful: 0
- features: fase4/cadastros
- evidence: plan Door 1 cadastros_db (infra/database)
- last seen: 2026-11-12T19:23:22Z

### L-008 - No Windows, check_proofs roda em cmd.exe: sem python/node no PATH e sem '!' ou aspas simples; use só dotnet, git, grep e helm, e defina DOTNET_CLI_UI_LANGUAGE=en com decodificação tolerante.
- signal: `gate_fail` · recurrence: 1 feature(s) · scope: `tooling` · harmful: 0
- features: fase4/cadastros
- evidence: check_proofs Windows (tooling)
- last seen: 2026-11-12T19:23:22Z

### L-009 - Permissions boundary compartilhado entre serviços precisa se limitar ao serviço (tag do principal); prove com política Allow * sob o boundary no simulador do IAM.
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: verification.md R1 ALTA boundary (iam)
- last seen: 2026-11-16T16:12:50Z

### L-010 - Lambda com Function URL AWS_IAM: negue lambda:InvokeFunction sem lambda:InvokedViaFunctionUrl, senão um evento forjado contorna a identidade do chamador.
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: verification.md R1 InvokeFunction (iam)
- last seen: 2026-11-16T16:12:50Z

### L-011 - Condição de job com função de status (always, failure, cancelled) desliga o gate implícito do needs; a prova precisa exigir needs.<job>.result == 'success' explícito.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `ci` · harmful: 0
- features: fase4/deploy-servicos
- evidence: C18/AC 15 (D-60) (ci)
- last seen: 2026-11-16T16:12:50Z

### L-012 - Isolamento do state do Terraform por serviço precisa de caso negativo no simulador (ler e gravar o state de outro serviço), não só o positivo.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: C24/porta 2-3 (iam)
- last seen: 2026-11-16T16:12:50Z

### L-013 - Conferir nome de fila: case a chave do produtor com a fila que o consumidor de fato lê, não só com alguma fila declarada por ele.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `deploy` · harmful: 0
- features: fase4/deploy-servicos
- evidence: C35/porta 4 (deploy)
- last seen: 2026-11-16T16:12:50Z

### L-014 - Mudança de decisão no meio do build vira D-xx no índice e § 6.4, plano volta para revisão e o check novo é acrescentado, nunca reescrito.
- signal: `spec_deviation` · recurrence: 1 feature(s) · scope: `specs` · harmful: 0
- features: fase4/deploy-servicos
- evidence: verification.md R1 BAIXA protocolo (specs)
- last seen: 2026-11-16T16:12:51Z

### L-015 - Chaves de tag de role IAM não distinguem caixa: denies sobre aws:TagKeys usam StringEqualsIgnoreCase e negue sts:TagSession para a tag de sessão não sobrepor a da role.
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: verification.md R2 ALTA tag caixa (iam)
- last seen: 2026-11-16T16:12:51Z

### L-016 - Deny com ForAnyValue em aws:TagKeys precisa de caso com várias chaves na mesma requisição, senão ForAllValues passa despercebido.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: R2 C39/porta 2 (iam)
- last seen: 2026-11-16T16:12:51Z

### L-017 - Política IAM com Resource por prefixo de serviço precisa de casos negativos nas roles de outro serviço e na própria role de deploy.
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `iam` · harmful: 0
- features: fase4/deploy-servicos
- evidence: R2 C24/C39 porta 2 (iam)
- last seen: 2026-11-16T16:12:51Z

### L-018 - Ao fechar um achado sobre uma forma (always()), cubra todas as formas equivalentes (failure(), cancelled(), success() || failure()).
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `ci` · harmful: 0
- features: fase4/deploy-servicos
- evidence: R2 C18/AC 15 (D-60) (ci)
- last seen: 2026-11-16T16:12:51Z

## Quarantined (failed when applied - ignore)

A confirmed lesson that recurred alongside failure. Kept for the maintainer to review.

_none_
