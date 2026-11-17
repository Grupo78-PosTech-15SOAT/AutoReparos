"""Casos de simular_iam.py --caso sns (ver o docstring de simular_iam.py para o formato).

C22 de infra-ci-observabilidade (D-67, porta 3): SNS só no prefixo do serviço. A IRSA de um serviço, mesmo com uma
política inline `Allow *`, só publica nos tópicos `autoreparos-<servico>-*` (boundary pela tag `Servico`); a role de
deploy cria e altera só os próprios tópicos e assina tópicos de outros serviços (a assinatura é do consumidor).
"""
import _iam_deploy
from _iam_boundary import SOB_BOUNDARY, TUDO
from _iam_deploy import CONTA, REGIAO

TOPICO_EXECUCAO = "autoreparos-execucao-eventos"


def topico(nome):
    return f"arn:aws:sns:{REGIAO}:{CONTA}:{nome}"


def _irsa(servico, rotulo, acao, recurso, permitido):
    return (rotulo, TUDO, {}, acao, recurso, permitido, {"aws:PrincipalTag/Servico": [servico]}, SOB_BOUNDARY)


def _deploy(servico, rotulo, acao, recurso, permitido):
    return (rotulo, _iam_deploy.MODELO, _iam_deploy.variaveis(servico), acao, recurso, permitido, None)


def casos():
    return [
        _irsa("cobranca", "irsa da cobranca não publica no tópico da execucao", "sns:Publish",
              topico(TOPICO_EXECUCAO), False),
        _irsa("os", "irsa do os não publica no tópico da execucao", "sns:Publish", topico(TOPICO_EXECUCAO), False),
        _irsa("execucao", "irsa da execucao publica no próprio tópico", "sns:Publish", topico(TOPICO_EXECUCAO), True),
        _irsa("execucao", "irsa da execucao não apaga o próprio tópico", "sns:DeleteTopic", topico(TOPICO_EXECUCAO),
              False),
        _deploy("os", "deploy do os não cria autoreparos-execucao-x", "sns:CreateTopic",
                topico("autoreparos-execucao-x"), False),
        _deploy("os", "deploy do os não apaga o tópico da execucao", "sns:DeleteTopic", topico(TOPICO_EXECUCAO),
                False),
        _deploy("execucao", "deploy da execucao cria o próprio tópico", "sns:CreateTopic", topico(TOPICO_EXECUCAO),
                True),
        _deploy("os", "deploy do os assina o tópico da execucao", "sns:Subscribe", topico(TOPICO_EXECUCAO), True),
        _deploy("cadastros", "deploy do cadastros assina o tópico da execucao", "sns:Subscribe",
                topico(TOPICO_EXECUCAO), True),
        _deploy("cadastros", "deploy do cadastros não publica no tópico da execucao", "sns:Publish",
                topico(TOPICO_EXECUCAO), False),
    ]
