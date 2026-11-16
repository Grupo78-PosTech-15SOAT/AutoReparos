#!/usr/bin/env python3
"""Verifica que o comando de compensação de orçamento está documentado como
`cobranca.cancelar-orcamento.v1` nos quatro artefatos de contrato (AC 7) e que o
nome antigo `billing.orcamento.cancelar.v1` aparece apenas marcado como legado.

Uso (a partir da raiz do repositório):
    python scripts/specs/verificar_eventos_portugues.py
"""
import re
import sys
import unicodedata
from pathlib import Path

NOVO = "cobranca.cancelar-orcamento.v1"
LEGADO = "billing.orcamento.cancelar.v1"
TERMOS_LEGADO = ("legad", "legacy", "antig", "alias")

ARQUIVOS = (
    Path(".specs/features/fase4/especificacao/comunicacao/fase4_spec_comunicacao_microsservicos.md"),
    Path(".specs/features/fase4/saga-mensageria/contracts.md"),
    Path("submodules/AutoReparos.OS/README.md"),
    Path("submodules/AutoReparos.Cobranca/README.md"),
)


def normalizar(texto: str) -> str:
    sem_acento = unicodedata.normalize("NFKD", texto).encode("ascii", "ignore").decode("ascii")
    return sem_acento.lower()


def verificar_arquivo(raiz: Path, relativo: Path) -> list[str]:
    caminho = raiz / relativo
    if not caminho.is_file():
        return [f"{relativo}: arquivo não encontrado"]

    conteudo = caminho.read_text(encoding="utf-8")
    problemas = []
    if NOVO not in conteudo:
        problemas.append(f"{relativo}: não contém o tipo novo `{NOVO}`")

    for numero, linha in enumerate(conteudo.splitlines(), start=1):
        if LEGADO in linha and not any(termo in normalizar(linha) for termo in TERMOS_LEGADO):
            problemas.append(
                f"{relativo}:{numero}: menciona `{LEGADO}` sem marcá-lo como entrada/alias legado"
            )
    return problemas


def main() -> int:
    raiz = Path(__file__).resolve().parent.parent.parent
    problemas = []
    for arquivo in ARQUIVOS:
        problemas.extend(verificar_arquivo(raiz, arquivo))

    if problemas:
        for problema in problemas:
            print(f"[eventos-portugues] FALHA: {problema}", file=sys.stderr)
        return 1

    print(
        f"[eventos-portugues] OK: {len(ARQUIVOS)} documentos de contrato nomeiam `{NOVO}` "
        f"e marcam `{LEGADO}` apenas como entrada legada"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())

