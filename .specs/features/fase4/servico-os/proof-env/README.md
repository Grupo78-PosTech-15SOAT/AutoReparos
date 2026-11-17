# Ambiente de prova do serviço OS (C5)

A prova C5 (`ServiceDatabaseIsolation`) exige banco, API e identidades reais. Estes scripts montam tudo localmente:

| Componente | Detalhe |
| --- | --- |
| PostgreSQL 16 | Contêiner Docker `os-proof-pg` (`postgres:16-alpine`), porta `55432`, database `os_proof` |
| Emissor JWT de teste | [`mock_issuer.py`](./mock_issuer.py): discovery e JWKS em `http://localhost:8099`, tokens RS256 |
| API do OS | Publicada pelo [`harness.sh`](./harness.sh) e servida na porta `5083` |

As credenciais são descartáveis e só valem localmente.

Requisitos: Git Bash (ou bash), Docker, .NET 10 SDK, `dotnet-ef` e Python 3.

```bash
pip install pyjwt cryptography
dotnet tool install --global dotnet-ef
bash .specs/features/fase4/servico-os/proof-env/run_proofs.sh --mutations .specs/features/fase4/servico-os/mutations.json
```

Notas:

- `Authentication__RequireHttpsMetadata=false` só é usado aqui; em produção o padrão continua HTTPS.
- A API de prova roda de um publish: se o código mudou, encerre o processo na porta `5083` antes de rodar de novo.
- Deixe a árvore git limpa antes de rodar: o gate registra `Tree: dirty` e sai com código 1 caso contrário.
- Os outros serviços (Cobrança e Execução) não precisam deste ambiente: os testes de integração deles usam Testcontainers e exigem apenas Docker.
