#!/usr/bin/env python3
"""Verifica links relativos e âncoras nos arquivos Markdown das especificações.

Uso (a partir da raiz do repositório):
    python3 scripts/specs/verificar_links.py                # padrão: .specs/features
    python3 scripts/specs/verificar_links.py <pasta> [...]  # outras pastas

Sai com código 1 se algum link apontar para arquivo inexistente ou para uma
âncora que não existe no arquivo de destino. Links http(s) e mailto são ignorados.
"""
import glob
import os
import re
import sys
import urllib.parse

LINK = re.compile(r"\[[^\]]*\]\(([^)\s]+)\)")
TITULO = re.compile(r"^#{1,6}\s+(.*)$", re.M)
BLOCO_CODIGO = re.compile(r"```.*?```", re.S)


def ancora(titulo: str) -> str:
    """Converte um título no identificador gerado pelo GitHub."""
    texto = titulo.replace("`", "").strip().lower()
    texto = re.sub(r"[^\w\- ]", "", texto, flags=re.U)
    return texto.replace(" ", "-")


def ancoras_do_arquivo(caminho: str, cache: dict) -> set:
    if caminho not in cache:
        with open(caminho, encoding="utf-8") as arquivo:
            conteudo = BLOCO_CODIGO.sub("", arquivo.read())
        cache[caminho] = {ancora(t) for t in TITULO.findall(conteudo)}
    return cache[caminho]


def verificar(pastas) -> int:
    cache, problemas, total = {}, 0, 0
    arquivos = sorted(
        f
        for pasta in pastas
        for f in glob.glob(os.path.join(pasta, "**", "*.md"), recursive=True)
        if "tlc-spec-lean" not in f.split(os.sep)
    )
    for origem in arquivos:
        with open(origem, encoding="utf-8") as arquivo:
            conteudo = BLOCO_CODIGO.sub("", arquivo.read())
        for url in LINK.findall(conteudo):
            if url.startswith(("http://", "https://", "mailto:")):
                continue
            total += 1
            caminho, _, alvo_ancora = url.partition("#")
            caminho = urllib.parse.unquote(caminho)
            destino = os.path.normpath(os.path.join(os.path.dirname(origem), caminho)) if caminho else origem
            if not os.path.exists(destino):
                print(f"ARQUIVO INEXISTENTE  {origem}: {url}")
                problemas += 1
                continue
            if alvo_ancora and destino.endswith(".md"):
                if urllib.parse.unquote(alvo_ancora) not in ancoras_do_arquivo(destino, cache):
                    print(f"ÂNCORA INEXISTENTE   {origem}: {url}")
                    problemas += 1
    print(f"{len(arquivos)} arquivos, {total} links, {problemas} problema(s)")
    return 1 if problemas else 0


if __name__ == "__main__":
    sys.exit(verificar(sys.argv[1:] or [os.path.join(".specs", "features")]))
