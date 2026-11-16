"""Checks de deploy-servicos da plataforma (S5): nós, SSM, workflows do pai, Ingress do Web e sem-admin da role de deploy.

Cada check é uma função sem argumentos que devolve (erros, mensagem de sucesso) e entra em CHECKS.
"""
from _deploy_comum import SERVICOS_IMPLANTADOS, executar, ultimas_linhas  # noqa: F401
from _repositorios import caminho_repositorio  # noqa: F401

CHECKS = {}
