#!/usr/bin/env python3
"""Provas de infraestrutura da feature `cadastros` (portáveis: cmd.exe e bash).

Uso:
    python3 scripts/specs/verificar_infra_cadastros.py rds-sem-lambda
    python3 scripts/specs/verificar_infra_cadastros.py networkpolicy

rds-sem-lambda: nenhum workflow passa o security group da Lambda ao RDS (somente o do EKS) e a camada de banco
    exige apenas o SG do EKS.
networkpolicy: o chart do Cadastros renderiza uma NetworkPolicy que nega por padrão e libera só as origens previstas.
"""
import pathlib
import re
import subprocess
import sys

RAIZ = pathlib.Path(__file__).resolve().parents[2]


def falhar(mensagem):
    print(f"FALHA: {mensagem}")
    sys.exit(1)


def rds_sem_lambda():
    for workflow in sorted((RAIZ / ".github" / "workflows").glob("*.yml")):
        for numero, linha in enumerate(workflow.read_text(encoding="utf-8").splitlines(), 1):
            if "TF_VAR_allowed_security_group_ids" in linha and re.search(r"\$lambda|lambda_sg|lambda_security_group", linha):
                falhar(f"{workflow.name}:{numero} ainda libera a Lambda no SG do RDS")

    variaveis = (RAIZ / "submodules" / "AutoReparos.Infra.Database" / "terraform" / "variables.tf").read_text(encoding="utf-8")
    if ">= 2" in variaveis.split('variable "allowed_security_group_ids"')[1].split("variable ")[0]:
        falhar("variables.tf ainda exige dois security groups (EKS e Lambda)")
    print("ok: o RDS só libera o security group do EKS")


def networkpolicy():
    saida = subprocess.run(
        ["helm", "template", "autoreparos", str(RAIZ / "submodules" / "AutoReparos.Infra.K8s" / "k8s"),
         "--set", "api.networkPolicy.allowedCidrs[0]=10.0.0.0/16"],
        capture_output=True, text=True, encoding="utf-8", errors="replace", check=False)
    if saida.returncode != 0:
        falhar(f"helm template falhou: {saida.stderr.strip()[:300]}")
    politica = saida.stdout.split("kind: NetworkPolicy", 1)
    if len(politica) < 2:
        falhar("o chart não renderiza NetworkPolicy")
    trecho = politica[1].split("\n---", 1)[0]
    for esperado in ("policyTypes:", "- Ingress", "ingress-nginx", "10.0.0.0/16", "podSelector: {}"):
        if esperado not in trecho:
            falhar(f"a NetworkPolicy não contém '{esperado}'")
    print("ok: NetworkPolicy renderizada com origens restritas")


if __name__ == "__main__":
    comandos = {"rds-sem-lambda": rds_sem_lambda, "networkpolicy": networkpolicy}
    if len(sys.argv) != 2 or sys.argv[1] not in comandos:
        falhar(f"uso: {sys.argv[0]} {{{'|'.join(comandos)}}}")
    comandos[sys.argv[1]]()
