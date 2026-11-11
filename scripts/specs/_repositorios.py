"""Dados e utilitários comuns às provas de infra-ci-observabilidade."""
import json
import os
import re
import subprocess

ORG = "Grupo78-PosTech-15SOAT"
SERVICOS = ["OS", "Cobranca", "Execucao", "Cadastros", "Web"]
JOBS_OBRIGATORIOS = ["build", "test", "quality"]
RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


def caminho_repositorio(servico):
    return os.path.join(RAIZ, "submodules", f"AutoReparos.{servico}")


def nome_completo(servico):
    return f"{ORG}/AutoReparos.{servico}"


def gh_api(rota):
    """Chama `gh api` e devolve (status, json). Status 0 é sucesso."""
    processo = subprocess.run(
        ["gh", "api", "-H", "Accept: application/vnd.github+json", rota],
        capture_output=True, text=True, encoding="utf-8",
    )
    try:
        corpo = json.loads(processo.stdout) if processo.stdout.strip() else None
    except json.JSONDecodeError:
        corpo = None
    return processo.returncode, corpo


def ler_workflow(servico):
    caminho = os.path.join(caminho_repositorio(servico), ".github", "workflows", "ci-cd.yml")
    if not os.path.isfile(caminho):
        return None
    with open(caminho, encoding="utf-8") as arquivo:
        return arquivo.read()


def bloco(texto, chave, indentacao):
    """Devolve as linhas do bloco YAML `chave:` na indentação dada, ou None."""
    linhas = texto.splitlines()
    prefixo = " " * indentacao
    padrao = re.compile(rf"^{prefixo}{re.escape(chave)}:\s*(#.*)?$")
    for indice, linha in enumerate(linhas):
        if padrao.match(linha):
            corpo = []
            for seguinte in linhas[indice + 1:]:
                if seguinte.strip() and not seguinte.startswith(prefixo + " "):
                    break
                corpo.append(seguinte)
            return "\n".join(corpo)
    return None


def jobs(texto):
    corpo = bloco(texto, "jobs", 0)
    if corpo is None:
        return {}
    nomes = re.findall(r"^  ([A-Za-z0-9_-]+):\s*$", corpo, re.MULTILINE)
    return {nome: bloco(corpo, nome, 2) for nome in nomes}


def reportar(erros, sucesso):
    for erro in erros:
        print(f"FALHA {erro}")
    if erros:
        print(f"{len(erros)} falha(s)")
        return 1
    print(sucesso)
    return 0
