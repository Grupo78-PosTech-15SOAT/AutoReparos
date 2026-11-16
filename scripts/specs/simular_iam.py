#!/usr/bin/env python3
"""Provas C13, C24, C39 e C40 de deploy-servicos: avalia políticas IAM no simulador da AWS, sem criar recursos.

Uso: python3 scripts/specs/simular_iam.py --caso <irsa|deploy|boundary|state>

As políticas ficam em arquivos `.json.tftpl` dos repositórios, renderizados pelo Terraform com
`templatefile()`. Para que este script as avalie sem Terraform, os modelos usam só interpolação
simples `${nome}` e o escape `$${...}`, que o Terraform entrega como o texto `${...}` (variáveis de
política do IAM, como `${aws:PrincipalTag/Servico}`); sem diretivas `%{ }` nem funções. Cada caso é
uma tupla (rótulo, política, variáveis, ação, ARN do recurso, permitido?, contexto[, boundary])
devolvida por `casos()` em `_iam_irsa.py`, `_iam_deploy.py`, `_iam_boundary.py` ou `_iam_state.py`:
- política: caminho do modelo (relativo à raiz do pai) ou um dict com a política literal;
- contexto: dict chave -> lista de valores (tipo `string`) ou -> (tipo, lista de valores), por
  exemplo ("stringList", ["Servico"]) ou ("boolean", ["true"]); ou None;
- boundary (opcional): (caminho do modelo, variáveis) do permissions boundary avaliado junto.
"""
import argparse
import json
import os
import re
import subprocess
import sys

from _repositorios import RAIZ, reportar
import _iam_boundary
import _iam_deploy
import _iam_irsa
import _iam_state

CONTA = "198903849859"
REGIAO = "us-east-1"
PERFIL = os.environ.get("AWS_PROFILE", "autoreparos")
CASOS = {"irsa": _iam_irsa.casos, "deploy": _iam_deploy.casos, "boundary": _iam_boundary.casos,
         "state": _iam_state.casos}
INTERPOLACAO = re.compile(r"\$\$\{|\$\{([^}]*)\}")


def renderizar(caminho, variaveis):
    """Lê o modelo (relativo à raiz do pai) como o templatefile(): `${nome}` vira o valor e `$${` vira o texto
    `${`; recusa variáveis sem valor. Um dict é uma política literal e só é serializado."""
    if isinstance(caminho, dict):
        return json.dumps(caminho)
    with open(os.path.join(RAIZ, caminho), encoding="utf-8") as arquivo:
        texto = arquivo.read()
    sobras = set()

    def trocar(achado):
        if achado.group(0) == "$${":
            return "${"
        nome = achado.group(1)
        if nome not in variaveis:
            sobras.add(nome)
            return achado.group(0)
        return variaveis[nome]

    texto = INTERPOLACAO.sub(trocar, texto)
    if sobras:
        raise ValueError(f"{caminho}: variáveis sem valor: {sorted(sobras)}")
    json.loads(texto)
    return texto


def simular(politica, acao, recurso, contexto=None, boundary=None):
    """Devolve a EvalDecision do simulador (allowed, implicitDeny ou explicitDeny)."""
    comando = ["aws", "iam", "simulate-custom-policy", "--profile", PERFIL, "--output", "json",
               "--policy-input-list", politica, "--action-names", acao, "--resource-arns", recurso]
    if boundary is not None:
        comando += ["--permissions-boundary-policy-input-list", boundary]
    entradas = []
    for chave, valores in (contexto or {}).items():
        tipo, valores = valores if isinstance(valores, tuple) else ("string", valores)
        entradas.append({"ContextKeyName": chave, "ContextKeyValues": valores, "ContextKeyType": tipo})
    if entradas:
        comando += ["--context-entries", json.dumps(entradas)]
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
    for rotulo, caminho, variaveis, acao, recurso, permitido, contexto, *extra in casos:
        try:
            boundary = renderizar(*extra[0]) if extra else None
            decisao = simular(renderizar(caminho, variaveis), acao, recurso, contexto, boundary)
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
