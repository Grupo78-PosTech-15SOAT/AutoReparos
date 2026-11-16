#!/usr/bin/env python3
"""Provas de deploy-servicos (fase 4): imagem, IaC e chart de cada serviço.

Uso: python3 scripts/specs/assert_deploy.py --check <nome>

Cada check é uma função registrada em CHECKS que devolve (erros, mensagem de sucesso).
"""
import argparse
import glob
import json
import os
import shutil
import subprocess
import sys

from _repositorios import caminho_repositorio, reportar

SERVICOS_IMPLANTADOS = ["OS", "Cobranca", "Execucao", "Cadastros"]
PORTA = "8080"
USUARIOS_ROOT = {"", "root", "0"}


def executar(comando, diretorio=None):
    """Roda o comando e devolve (código de saída, saída combinada)."""
    processo = subprocess.run(comando, cwd=diretorio, capture_output=True, text=True, encoding="utf-8")
    return processo.returncode, (processo.stdout + processo.stderr).strip()


def ultimas_linhas(texto, quantidade=20):
    return "\n".join(texto.splitlines()[-quantidade:])


def porta_configurada(variaveis):
    """Diz se o ASP.NET Core da imagem escuta em PORTA, pelas variáveis de ambiente da imagem."""
    urls = variaveis.get("ASPNETCORE_URLS")
    if urls:
        return any(url.rstrip("/").endswith(f":{PORTA}") for url in urls.split(";"))
    portas = variaveis.get("ASPNETCORE_HTTP_PORTS", "")
    return PORTA in [p.strip() for p in portas.split(";")]


def checar_imagem(servico):
    repositorio = caminho_repositorio(servico)
    dockerfile = os.path.join(repositorio, "Dockerfile")
    if not os.path.isfile(dockerfile):
        return [f"{servico}: Dockerfile ausente na raiz do repositório"], ""
    if shutil.which("docker") is None:
        return [f"{servico}: docker não encontrado no PATH"], ""

    tag = f"autoreparos-{servico.lower()}:assert-deploy"
    codigo, saida = executar(["docker", "build", "-f", dockerfile, "-t", tag, repositorio])
    if codigo != 0:
        return [f"{servico}: docker build falhou\n{ultimas_linhas(saida)}"], ""
    codigo, saida = executar(["docker", "image", "inspect", tag])
    if codigo != 0:
        return [f"{servico}: docker image inspect falhou\n{saida}"], ""

    config = json.loads(saida)[0].get("Config") or {}
    erros = []
    expostas = config.get("ExposedPorts") or {}
    if f"{PORTA}/tcp" not in expostas:
        erros.append(f"{servico}: porta {PORTA} não exposta (ExposedPorts: {sorted(expostas)})")
    variaveis = dict(item.split("=", 1) for item in config.get("Env") or [] if "=" in item)
    if not porta_configurada(variaveis):
        erros.append(f"{servico}: o ASP.NET Core da imagem não escuta em {PORTA} "
                     f"(ASPNETCORE_URLS={variaveis.get('ASPNETCORE_URLS')!r}, "
                     f"ASPNETCORE_HTTP_PORTS={variaveis.get('ASPNETCORE_HTTP_PORTS')!r})")
    usuario = (config.get("User") or "").strip()
    if usuario.split(":")[0] in USUARIOS_ROOT:
        erros.append(f"{servico}: a imagem roda como root (Config.User={usuario!r})")
    return erros, f"OK dockerfile-{servico.lower()}: {tag} expõe {PORTA} e roda como {usuario}"


def checar_iac_formatado():
    erros = []
    verificados = []
    for servico in SERVICOS_IMPLANTADOS:
        repositorio = caminho_repositorio(servico)
        terraform = os.path.join(repositorio, "infra", "terraform")
        if os.path.isdir(terraform):
            verificados.append(f"{servico}/infra/terraform")
            if shutil.which("terraform") is None:
                erros.append(f"{servico}: terraform não encontrado no PATH")
            else:
                codigo, saida = executar(["terraform", "fmt", "-check", "-recursive", terraform])
                if codigo != 0:
                    erros.append(f"{servico}: terraform fmt -check falhou\n{saida}")
        for chart in sorted(glob.glob(os.path.join(repositorio, "deploy", "helm", "*", ""))):
            nome = os.path.basename(os.path.normpath(chart))
            verificados.append(f"{servico}/deploy/helm/{nome}")
            if shutil.which("helm") is None:
                erros.append(f"{servico}: helm não encontrado no PATH")
                continue
            codigo, saida = executar(["helm", "lint", chart])
            if codigo != 0:
                erros.append(f"{servico}: helm lint de {nome} falhou\n{ultimas_linhas(saida)}")
    alvo = ", ".join(verificados) if verificados else "nenhum diretório de IaC presente"
    return erros, f"OK iac-formatado: {alvo}"


CHECKS = {
    "dockerfile-os": lambda: checar_imagem("OS"),
    "iac-formatado": checar_iac_formatado,
}


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", required=True, choices=sorted(CHECKS))
    args = parser.parse_args()
    erros, sucesso = CHECKS[args.check]()
    return reportar(erros, sucesso)


if __name__ == "__main__":
    sys.exit(main())
