"""Casos de simular_iam.py --caso boundary (ver o docstring de simular_iam.py para o formato).

C39: o boundary `autoreparos-servico-boundary` (modelo `servico-boundary.json.tftpl` da plataforma) é um só para os
4 serviços e se limita ao serviço pela tag `Servico` da role IRSA. Mesmo com uma política inline `Allow *`, que a role
de deploy consegue pôr na própria IRSA, a IRSA da Cobrança não lê o segredo do OS nem escreve na tabela da Saga, e
ainda publica na fila da Saga (porta 4). A role de deploy da Cobrança só cria role com `Servico = cobranca`, não troca
nem tira essa tag, e só invoca o provisionador pela Function URL (porta 10, AC 12).
"""
import _iam_deploy
from _iam_deploy import CONTA, PROVISIONADOR, REGIAO, fila, role

BOUNDARY = "submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/servico-boundary.json.tftpl"
SOB_BOUNDARY = (BOUNDARY, {"conta": CONTA, "regiao": REGIAO})
TUDO = {"Version": "2012-10-17", "Statement": [{"Effect": "Allow", "Action": "*", "Resource": "*"}]}
COBRANCA = {"aws:PrincipalTag/Servico": ["cobranca"]}
CRIAR_COM_BOUNDARY = {"iam:PermissionsBoundary": [_iam_deploy.BOUNDARY]}
TAG_ROLE = "iam:TagRole"
UNTAG_ROLE = "iam:UntagRole"


def segredo(caminho):
    return f"arn:aws:secretsmanager:{REGIAO}:{CONTA}:secret:autoreparos/production/{caminho}"


def tabela(nome):
    return f"arn:aws:dynamodb:{REGIAO}:{CONTA}:table/{nome}"


def _irsa(rotulo, acao, recurso, permitido, contexto=COBRANCA):
    return (rotulo, TUDO, {}, acao, recurso, permitido, contexto, SOB_BOUNDARY)


def _deploy(rotulo, acao, recurso, permitido, contexto=None):
    return (rotulo, _iam_deploy.MODELO, _iam_deploy.variaveis("cobranca"), acao, recurso, permitido, contexto)


def casos():
    irsa = role("autoreparos-cobranca-irsa")
    return [
        # IRSA da Cobrança com Allow * sob o boundary.
        _irsa("irsa cobranca lê o segredo da database do OS", "secretsmanager:GetSecretValue",
              segredo("os/database-AbCdEf"), False),
        _irsa("irsa cobranca grava na tabela da Saga do OS", "dynamodb:PutItem", tabela("autoreparos-os-saga"), False),
        _irsa("irsa cobranca consome a fila da Saga do OS", "sqs:ReceiveMessage", fila("autoreparos-os-saga"), False),
        _irsa("irsa cobranca cria role", "iam:CreateRole", role("autoreparos-cobranca-outra"), False),
        _irsa("irsa cobranca lê o próprio segredo", "secretsmanager:GetSecretValue",
              segredo("cobranca/mercadopago-AbCdEf"), True),
        _irsa("irsa cobranca publica na fila da Saga do OS", "sqs:SendMessage", fila("autoreparos-os-saga"), True),
        _irsa("irsa cobranca consome a própria fila", "sqs:ReceiveMessage", fila("autoreparos-cobranca-comandos"), True),
        _irsa("irsa sem a tag Servico lê o segredo da Cobrança", "secretsmanager:GetSecretValue",
              segredo("cobranca/mercadopago-AbCdEf"), False, None),
        # Role de deploy da Cobrança: a tag Servico só com o próprio serviço.
        _deploy("deploy cobranca cria role com o boundary e sem a tag Servico", "iam:CreateRole", irsa, False,
                CRIAR_COM_BOUNDARY),
        _deploy("deploy cobranca cria role com o boundary e Servico = os", "iam:CreateRole", irsa, False,
                {**CRIAR_COM_BOUNDARY, "aws:RequestTag/Servico": ["os"], "aws:TagKeys": ("stringList", ["Servico"])}),
        _deploy("deploy cobranca cria role com o boundary e Servico = cobranca", "iam:CreateRole", irsa, True,
                {**CRIAR_COM_BOUNDARY, "aws:RequestTag/Servico": ["cobranca"],
                 "aws:TagKeys": ("stringList", ["Servico"])}),
        _deploy("deploy cobranca troca a tag Servico da própria IRSA para os", TAG_ROLE, irsa, False,
                {"aws:RequestTag/Servico": ["os"], "aws:TagKeys": ("stringList", ["Servico"])}),
        _deploy("deploy cobranca tira a tag Servico da própria IRSA", UNTAG_ROLE, irsa, False,
                {"aws:TagKeys": ("stringList", ["Servico"])}),
        # A chave da tag de role não distingue caixa: `servico=os` substitui `Servico=cobranca` (achado R2 ALTA).
        _deploy("deploy cobranca grava a tag servico = os na própria IRSA", TAG_ROLE, irsa, False,
                {"aws:RequestTag/servico": ["os"], "aws:TagKeys": ("stringList", ["servico"])}),
        _deploy("deploy cobranca grava a tag SERVICO = os na própria IRSA", TAG_ROLE, irsa, False,
                {"aws:RequestTag/SERVICO": ["os"], "aws:TagKeys": ("stringList", ["SERVICO"])}),
        _deploy("deploy cobranca tira a tag servico da própria IRSA", UNTAG_ROLE, irsa, False,
                {"aws:TagKeys": ("stringList", ["servico"])}),
        _deploy("deploy cobranca cria role com o boundary e servico = os", "iam:CreateRole", irsa, False,
                {**CRIAR_COM_BOUNDARY, "aws:RequestTag/servico": ["os"], "aws:TagKeys": ("stringList", ["servico"])}),
        # Várias chaves na mesma requisição (achado R2: ForAllValues no lugar de ForAnyValue).
        _deploy("deploy cobranca troca Ambiente e Servico = os da própria IRSA", TAG_ROLE, irsa, False,
                {"aws:RequestTag/Ambiente": ["production"], "aws:RequestTag/Servico": ["os"],
                 "aws:TagKeys": ("stringList", ["Ambiente", "Servico"])}),
        _deploy("deploy cobranca tira Ambiente e Servico da própria IRSA", UNTAG_ROLE, irsa, False,
                {"aws:TagKeys": ("stringList", ["Ambiente", "Servico"])}),
        # Tag de sessão Servico sobrepõe a da role: a role de deploy não passa tags de sessão.
        _deploy("deploy cobranca assume a própria IRSA com a tag de sessão Servico = os", "sts:TagSession", irsa,
                False, {"aws:RequestTag/Servico": ["os"], "aws:TagKeys": ("stringList", ["Servico"])}),
        _deploy("deploy cobranca troca outra tag da própria IRSA", TAG_ROLE, irsa, True,
                {"aws:RequestTag/Ambiente": ["production"], "aws:TagKeys": ("stringList", ["Ambiente"])}),
        # Provisionador só pela Function URL (porta 10).
        _deploy("deploy cobranca invoca o provisionador direto", "lambda:InvokeFunction", PROVISIONADOR, False),
        _deploy("deploy cobranca invoca o provisionador pela Function URL", "lambda:InvokeFunction", PROVISIONADOR,
                True, {"lambda:InvokedViaFunctionUrl": ("boolean", ["true"])}),
        _deploy("deploy cobranca chama a Function URL do provisionador", "lambda:InvokeFunctionUrl", PROVISIONADOR,
                True),
    ]
