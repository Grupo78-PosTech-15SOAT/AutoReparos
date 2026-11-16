# Pacote acadêmico de entrega - checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx -m:1`
Plan: `.specs/features/fase4/pacote-entrega/plan.md`

8 checks · 2 slices · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Evidências nos repositórios

**C1** - Cada repositório de serviço (OS, Cobrança, Execução e Cadastros) tem `Dockerfile`, manifests em `deploy/`, `ci-cd.yml`, README com seção de arquitetura e seção de cobertura com o limite de 80% e o link do SonarCloud (AC 1)
Proof: `python scripts/specs/assert_entrega.py --check repos`

**C2** - O README do OS justifica a Saga orquestrada e os de Cobrança e Execução apontam o papel de cada um nela; o README do Web descreve a aplicação e o `API_URL` (AC 1)
Proof: `python scripts/specs/assert_entrega.py --check saga`

**C3** - OS, Cobrança e Execução têm Swagger/OpenAPI exposto ou uma collection Postman versionada no repositório (AC 1)
Proof: `python scripts/specs/assert_entrega.py --check api`

**C4** - A suíte `AutoReparos.OS.BddTests` executa pelo menos 3 cenários Reqnroll (fluxo completo, pagamento recusado e falha na reserva) e todos passam; zero cenários executados reprova (AC 8, D-63)
Proof: `python scripts/specs/assert_entrega.py --check bdd`

### S2 - Vídeo, PDF e submissão

**C5** - O vídeo entregue está público ou não listado no YouTube ou Vimeo e dura no máximo 15 minutos (AC 2)
Proof: walk - abrir o link do vídeo, conferir plataforma, visibilidade e duração

**C6** - O vídeo mostra o fluxo completo da OS, a Saga com falha, o deploy automatizado de um serviço com testes passando, o monitoramento e o tracing, o código do orquestrador, de uma compensação e da idempotência e a proteção de branch bloqueando um PR (AC 3, AC 4)
Proof: walk - conferir cada segmento no vídeo entregue

**C7** - O PDF final tem participantes, links dos repositórios e do vídeo, diagrama de serviços, bancos e comunicação, estratégia e justificativa da Saga e justificativa da divisão e das tecnologias; entrada ausente fica listada como pendente e a submissão não ocorre (AC 5, AC 6)
Proof: walk - conferir cada seção do PDF e a lista de pendências antes de submeter

**C8** - A submissão ao portal só acontece depois da revisão da equipe e da autorização explícita registrada em chat (AC 7)
Proof: walk - registrar a autorização antes do envio

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| conteúdo do repositório (5) | Dockerfile C1 · manifests C1 · CI/CD C1 · README com arquitetura e cobertura C1 · Swagger/Postman C3 | - |
| README (3) | Saga justificada C2 · papel na Saga C2 · Web C2 | - |
| BDD (3) | fluxo completo C4 · pagamento recusado C4 · falha na reserva C4 | - |
| vídeo (6) | plataforma C5 · visibilidade C5 · até 15 min C5 · fluxo, Saga e falha C6 · deploy com testes C6 · monitoramento e tracing C6 | - |
| PDF (7) | participantes C7 · links C7 · vídeo C7 · diagrama C7 · Saga C7 · divisão C7 · tecnologias C7 | - |
| salvaguardas (2) | entrada ausente C7 · autorização C8 | - |

## Swept

- validation: C1, C2, C3, C4
- failure modes: C4, C7, C8
- idempotency: n/a - o pacote é montado uma vez e revisado antes da única submissão
- authorization: C8
- concurrency: n/a - pacote montado como um único release revisado
- data lifecycle: C1, C7
- dependency failure: C5, C6
- state transitions: C7, C8
- observability: C6

## Handoff

- **Settled mid-build:** 16/11 - D-63 "No OS, com harness (Recommended)"; plano aprovado ("Aprovo e envie")
- **Dispatched:** none
- **Boundary:** checks derivados do plano aprovado em 16/11; build não iniciado
- **Abandoned:** checks de 10/11 (scripts `assert_delivery_contents.py`, `assert_final_pdf.py` e `assert_package_readiness.py` nunca existiram)
- **Walked:** none - C5 a C8 exigem o vídeo e o PDF finais
