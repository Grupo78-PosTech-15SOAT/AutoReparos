"""Checks de deploy-servicos dos repositórios de serviço: Terraform (S2), charts (S3), filas (C35) e staging (C21).

Cada check é uma função sem argumentos que devolve (erros, mensagem de sucesso) e entra em CHECKS.
"""
from _deploy_comum import SERVICOS_IMPLANTADOS, executar, ultimas_linhas  # noqa: F401
from _repositorios import caminho_repositorio  # noqa: F401

CHECKS = {}
