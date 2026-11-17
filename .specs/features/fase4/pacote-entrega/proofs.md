# fase4/pacote-entrega proofs

**HEAD**: f86cd95
**Tree**: clean
**Result**: 9 pass, 0 fail; mutations 4 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `python scripts/specs/assert_entrega.py --check repos` | count unknown | - | pass |
| C2 | `python scripts/specs/assert_entrega.py --check saga` | count unknown | - | pass |
| C3 | `python scripts/specs/assert_entrega.py --check api` | count unknown | - | pass |
| C4 | `python scripts/specs/assert_entrega.py --check bdd` | count unknown | - | pass |
| C5 | `walk - abrir o link do vídeo, conferir plataforma, visibilidade e duração` | - | - | pass (Walked) |
| C6 | `walk - conferir cada segmento no vídeo entregue` | - | - | pass (Walked) |
| C7 | `walk - conferir cada seção do PDF e a lista de pendências antes de submeter` | - | - | pass (Walked) |
| C8 | `walk - registrar a autorização antes do envio` | - | - | pass (Walked) |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx -m:1` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| reserva (etapa 5) compensada antes da execução (2): ordem inversa quebrada | `submodules/AutoReparos.OS/AutoReparos.OS.Domain/SagaOS.cs:617` | yes |
| OS sem insumos também envia execucao.reservar-insumos.v1 | `submodules/AutoReparos.OS/AutoReparos.OS.Domain/SagaOS.cs:464` | yes |
| pagamento recusado não é registrado no histórico da Saga | `submodules/AutoReparos.OS/AutoReparos.OS.Domain/SagaOS.cs:230` | yes |
| execução concluída não sincroniza a OS para Concluida | `submodules/AutoReparos.OS/AutoReparos.OS.Domain/SagaOS.cs:203` | yes |
