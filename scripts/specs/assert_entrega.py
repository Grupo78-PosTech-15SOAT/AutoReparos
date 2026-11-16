#!/usr/bin/env python3
"""Provas do pacote-entrega (C1 a C4): conteúdo dos repositórios, README da Saga, API e BDD.

Uso: python scripts/specs/assert_entrega.py --check <repos|saga|api|bdd>
"""
import argparse
import os
import re
import subprocess
import sys
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


def checar_bdd(erros):
    projeto = repo("OS") / "AutoReparos.OS.BddTests"
    if not projeto.is_dir():
        erros.append("OS: projeto AutoReparos.OS.BddTests inexistente")
        return
    if not list(projeto.rglob("*.feature")):
        erros.append("OS.BddTests: nenhum arquivo .feature")
    ambiente = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en")
    exec_ = subprocess.run(["dotnet", "test", str(projeto), "--nologo", "-v", "q"],
                           capture_output=True, text=True, env=ambiente)
    saida = exec_.stdout + exec_.stderr
    resumo = re.search(r"Failed:\s*(\d+),\s*Passed:\s*(\d+)", saida)
    if not resumo:
        erros.append("OS.BddTests: nenhum cenário executado (sem resumo do dotnet test)")
    elif int(resumo.group(1)) or int(resumo.group(2)) < 4 or exec_.returncode:
        erros.append(f"OS.BddTests: falhas={resumo.group(1)} passaram={resumo.group(2)} (esperado 0 e pelo menos 4)")


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
