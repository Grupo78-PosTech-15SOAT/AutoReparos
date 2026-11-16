"""Casos de simular_iam.py --caso state (ver o docstring de simular_iam.py para o formato).

C40: a role `autoreparos-deploy-cobranca` só lê e grava o próprio state (`servicos/cobranca/production.tfstate`,
portas 2 e 3) e só pega o lock dele na tabela do Terraform; o state do OS, que guarda a senha da database em texto,
fica fora de alcance.
"""
from _iam_deploy import CONTA, MODELO, REGIAO, variaveis

COBRANCA = variaveis("cobranca")
BUCKET = COBRANCA["bucket"]
TABELA_LOCK = f"arn:aws:dynamodb:{REGIAO}:{CONTA}:table/{COBRANCA['tabela_lock']}"


def chave(servico):
    return f"servicos/{servico}/production.tfstate"


def objeto(servico):
    return f"arn:aws:s3:::{BUCKET}/{chave(servico)}"


def lock(servico):
    """Contexto do PutItem do lock: o Terraform usa `<bucket>/<chave>` como chave de partição."""
    return {"dynamodb:LeadingKeys": ("stringList", [f"{BUCKET}/{chave(servico)}"])}


def _caso(rotulo, acao, recurso, permitido, contexto=None):
    return (rotulo, MODELO, COBRANCA, acao, recurso, permitido, contexto)


def casos():
    return [
        _caso("cobranca lê o state do OS", "s3:GetObject", objeto("os"), False),
        _caso("cobranca grava o state do OS", "s3:PutObject", objeto("os"), False),
        _caso("cobranca pega o lock do state do OS", "dynamodb:PutItem", TABELA_LOCK, False, lock("os")),
        _caso("cobranca lê o próprio state", "s3:GetObject", objeto("cobranca"), True),
        _caso("cobranca grava o próprio state", "s3:PutObject", objeto("cobranca"), True),
        _caso("cobranca pega o lock do próprio state", "dynamodb:PutItem", TABELA_LOCK, True, lock("cobranca")),
    ]
