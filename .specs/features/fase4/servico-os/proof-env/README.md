# Ambiente de prova do serviço OS (C5)

A prova C5 (`ServiceDatabaseIsolation`) exige banco, API e identidades reais. Estes scripts montam tudo localmente:
Postgres 16 em Docker (`os-proof-pg`, porta 55432), um emissor JWT de teste (`mock_issuer.py`, porta 8099) e a API
do OS publicada (porta 5083). As credenciais sao descartaveis e so valem localmente.

```bash
pip install pyjwt cryptography
dotnet tool install --global dotnet-ef
bash .specs/features/fase4/servico-os/proof-env/run_proofs.sh --mutations .specs/features/fase4/servico-os/mutations.json
```

Notas:
- `Authentication__RequireHttpsMetadata=false` so e usado aqui; em producao o padrao continua HTTPS.
- Deixe a arvore git limpa antes de rodar: o gate registra `Tree: dirty` e sai com codigo 1 caso contrario.
- Os outros servicos (Billing e Execution) nao precisam deste ambiente: usam Testcontainers e exigem apenas Docker.
