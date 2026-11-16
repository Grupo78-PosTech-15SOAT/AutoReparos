#!/usr/bin/env python3
"""Provas C13 e C24 de deploy-servicos: avalia políticas IAM no simulador da AWS, sem criar recursos.

Uso: python3 scripts/specs/simular_iam.py --caso <irsa|deploy>

As políticas ficam em arquivos `.json.tftpl` dos repositórios, renderizados pelo Terraform com
`templatefile()`. Para que este script as avalie sem Terraform, os modelos usam só interpolação
simples `${nome}` (sem diretivas `%{ }` nem funções). Cada caso é uma tupla
(rótulo, caminho do modelo, variáveis, ação, ARN do recurso, permitido?, contexto) devolvida por
`casos()` em `_iam_irsa.py` ou `_iam_deploy.py`; `contexto` é um dict chave -> lista de valores
(ex.: {"iam:PermissionsBoundary": [arn]}) ou None.
"""
import argparse
import json
import os
import re
import subprocess
import sys

from _repositorios import RAIZ, reportar
import _iam_deploy
import _iam_irsa

CONTA = "198903849859"
REGIAO = "us-east-1"
PERFIL = os.environ.get("AWS_PROFILE", "autoreparos")
CASOS = {"irsa": _iam_irsa.casos, "deploy": _iam_deploy.casos}


def renderizar(caminho, variaveis):
    """Lê o modelo (relativo à raiz do pai) e substitui cada ${nome}; recusa sobras."""
    with open(os.path.join(RAIZ, caminho), encoding="utf-8") as arquivo:
        texto = arquivo.read()
    for nome, valor in variaveis.items():
        texto = texto.replace("${" + nome + "}", valor)
    sobras = sorted(set(re.findall(r"\$\{([^}]+)\}", texto)))
    if sobras:
        raise ValueError(f"{caminho}: variáveis sem valor: {sobras}")
    json.loads(texto)
    return texto


def simular(politica, acao, recurso, contexto=None):
    """Devolve a EvalDecision do simulador (allowed, implicitDeny ou explicitDeny)."""
    comando = ["aws", "iam", "simulate-custom-policy", "--profile", PERFIL, "--output", "json",
               "--policy-input-list", politica, "--action-names", acao, "--resource-arns", recurso]
    for chave, valores in (contexto or {}).items():
        comando += ["--context-entries",
                    json.dumps([{"ContextKeyName": chave, "ContextKeyValues": valores,
                                 "ContextKeyType": "string"}])]
    processo = subprocess.run(comando, capture_output=True, text=True, encoding="utf-8")
    if processo.returncode != 0:
        raise RuntimeError(processo.stderr.strip() or processo.stdout.strip())
    return json.loads(processo.stdout)["EvaluationResults"][0]["EvalDecision"]


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--caso", required=True, choices=sorted(CASOS))
    args = parser.parse_args()
    casos = CASOS[args.caso]()
    if not casos:
        return reportar([f"nenhum caso definido para --caso {args.caso}"], "")
    erros = []
    for rotulo, caminho, variaveis, acao, recurso, permitido, contexto in casos:
        try:
            decisao = simular(renderizar(caminho, variaveis), acao, recurso, contexto)
        except (OSError, ValueError, RuntimeError) as erro:
            erros.append(f"{rotulo}: {erro}")
            continue
        if (decisao == "allowed") != permitido:
            esperado = "permitido" if permitido else "negado"
            erros.append(f"{rotulo}: esperado {esperado}, simulador respondeu {decisao} ({acao} em {recurso})")
        else:
            print(f"ok {rotulo}: {decisao}")
    return reportar(erros, f"OK simular_iam --caso {args.caso}: {len(casos)} casos")


if __name__ == "__main__":
    sys.exit(main())
