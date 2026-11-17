#!/usr/bin/env python3
"""Provas do pacote-entrega (C1 a C4): conteúdo dos repositórios, README da Saga, API e BDD.

Uso: python scripts/specs/assert_entrega.py --check <repos|saga|api|bdd>
"""
import argparse
import os
import re
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[2]
SERVICOS = ["OS", "Cobranca", "Execucao", "Cadastros"]


def repo(nome):
    return RAIZ / "submodules" / f"AutoReparos.{nome}"


def readme(nome):
    arquivo = repo(nome) / "README.md"
    return arquivo.read_text(encoding="utf-8") if arquivo.exists() else ""


def arquivos(nome, padrao):
    ignorar = {"bin", "obj", "node_modules", ".git", "TestResults"}
    achados = []
    for base, pastas, nomes in os.walk(repo(nome)):
        pastas[:] = [p for p in pastas if p not in ignorar]
        achados += [Path(base) / n for n in nomes if re.search(padrao, n, re.I)]
    return achados


def checar_repos(erros):
    for nome in SERVICOS:
        base = repo(nome)
        if not arquivos(nome, r"^Dockerfile$"):
            erros.append(f"{nome}: sem Dockerfile")
        if not (base / "deploy").is_dir():
            erros.append(f"{nome}: sem pasta deploy/")
        if not (base / ".github" / "workflows" / "ci-cd.yml").exists():
            erros.append(f"{nome}: sem ci-cd.yml")
        texto = readme(nome)
        if not re.search(r"^#+ .*arquitetura", texto, re.I | re.M):
            erros.append(f"{nome}: README sem seção de arquitetura")
        if not re.search(r"^#+ .*cobertura", texto, re.I | re.M):
            erros.append(f"{nome}: README sem seção de cobertura")
        elif not (re.search(r"\b80\s*%", texto) and "sonarcloud.io" in texto.lower()):
            erros.append(f"{nome}: seção de cobertura sem o limite de 80% ou sem o link do SonarCloud")


def checar_saga(erros):
    os_texto = readme("OS")
    if not (re.search(r"orquestr", os_texto, re.I) and re.search(r"justific|por que|motivo", os_texto, re.I)):
        erros.append("OS: README não justifica a Saga orquestrada")
    for nome in ("Cobranca", "Execucao"):
        if not re.search(r"saga", readme(nome), re.I):
            erros.append(f"{nome}: README não descreve o papel na Saga")
    if not re.search(r"API_URL", readme("Web")):
        erros.append("Web: README não descreve o API_URL")


def checar_api(erros):
    for nome in ("OS", "Cobranca", "Execucao"):
        if not (arquivos(nome, r"postman_collection\.json$") or arquivos(nome, r"^openapi.*\.(json|ya?ml)$")):
            erros.append(f"{nome}: sem collection Postman nem arquivo OpenAPI versionado")


# AC 8: os seis cenários nomeados (D-63, D-73), conferidos pelo título no resultado do dotnet test.
CENARIOS_BDD = (
    "Pagamento aprovado conclui a Saga e marca a OS como paga",
    "OS só de mão de obra pula a reserva e libera a execução direto",
    "Pagamento recusado não dispara compensação e deixa a Saga aguardando",
    "Falha na reserva de insumos aguarda saldo e o cancelamento compensa só o orçamento",
    "Liberação recusada pelo Execução compensa a reserva e o orçamento em ordem inversa",
    "Cancelamento pelo atendente com insumos reservados compensa em ordem inversa",
)


def checar_bdd(erros):
    projeto = repo("OS") / "AutoReparos.OS.BddTests"
    if not projeto.is_dir():
        erros.append("OS: projeto AutoReparos.OS.BddTests inexistente")
        return
    if not list(projeto.rglob("*.feature")):
        erros.append("OS.BddTests: nenhum arquivo .feature")
    ambiente = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en")
    with tempfile.TemporaryDirectory() as pasta:
        exec_ = subprocess.run(["dotnet", "test", str(projeto), "--nologo", "-v", "q",
                                "--logger", "trx;LogFileName=bdd.trx", "--results-directory", pasta],
                               capture_output=True, text=True, env=ambiente)
        trx = list(Path(pasta).rglob("*.trx"))
        resultados = {}
        if trx:
            ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
            for r in ET.parse(trx[0]).getroot().iterfind(".//t:UnitTestResult", ns):
                resultados[r.get("testName")] = r.get("outcome")
    if not resultados:
        erros.append("OS.BddTests: nenhum cenário executado (sem resultado do dotnet test)")
        return
    for titulo in CENARIOS_BDD:
        if resultados.get(titulo) != "Passed":
            erros.append(f"OS.BddTests: cenário '{titulo}' {resultados.get(titulo, 'não executado')}")
    falhas = [n for n, o in resultados.items() if o != "Passed"]
    if falhas or exec_.returncode:
        erros.append(f"OS.BddTests: cenários sem sucesso {falhas} (código {exec_.returncode})")


CHECAGENS = {"repos": checar_repos, "saga": checar_saga, "api": checar_api, "bdd": checar_bdd}


def main():
    argumentos = argparse.ArgumentParser()
    argumentos.add_argument("--check", required=True, choices=sorted(CHECAGENS))
    erros = []
    CHECAGENS[argumentos.parse_args().check](erros)
    for erro in erros:
        print(f"FAIL {erro}")
    print("PASS" if not erros else f"{len(erros)} erro(s)")
    return 1 if erros else 0


if __name__ == "__main__":
    sys.exit(main())
