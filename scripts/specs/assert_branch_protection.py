#!/usr/bin/env python3
"""Provas C5 e C7 de infra-ci-observabilidade: proteção da main pela API do GitHub e evidência no README.

Uso: python scripts/specs/assert_branch_protection.py --check <main|readme>
"""
import argparse
import os
import sys

from _repositorios import JOBS_OBRIGATORIOS, SERVICOS, caminho_repositorio, gh_api, nome_completo, reportar


def checar_main(servico, erros):
    status, protecao = gh_api(f"repos/{nome_completo(servico)}/branches/main/protection")
    if status != 0 or not protecao or "url" not in protecao:
        erros.append(f"{servico}: main sem proteção")
        return
    revisoes = protecao.get("required_pull_request_reviews") or {}
    if revisoes.get("required_approving_review_count", 0) < 1:
        erros.append(f"{servico}: PR não exige 1 aprovação")
    if not revisoes.get("dismiss_stale_reviews"):
        erros.append(f"{servico}: dismiss_stale_reviews desligado")
    checks = protecao.get("required_status_checks") or {}
    if not checks.get("strict"):
        erros.append(f"{servico}: status checks não são strict")
    contextos = set(checks.get("contexts") or []) | {c.get("context") for c in checks.get("checks") or []}
    for job in JOBS_OBRIGATORIOS:
        if job not in contextos:
            erros.append(f"{servico}: status check `{job}` não é obrigatório")
    if not (protecao.get("enforce_admins") or {}).get("enabled"):
        erros.append(f"{servico}: regra não vale para administradores")
    if (protecao.get("allow_force_pushes") or {}).get("enabled"):
        erros.append(f"{servico}: force push permitido")
    if (protecao.get("allow_deletions") or {}).get("enabled"):
        erros.append(f"{servico}: exclusão da main permitida")


def checar_readme(servico, erros):
    caminho = os.path.join(caminho_repositorio(servico), "README.md")
    if not os.path.isfile(caminho):
        erros.append(f"{servico}: README.md ausente")
        return
    with open(caminho, encoding="utf-8") as arquivo:
        texto = arquivo.read()
    marcador = "## Proteção de branch"
    if marcador not in texto:
        erros.append(f"{servico}: README sem a seção `{marcador}`")
        return
    secao = texto.split(marcador, 1)[1].split("\n## ", 1)[0]
    if "branches/main/protection" not in secao:
        erros.append(f"{servico}: seção não mostra a saída de branches/main/protection")
    for job in JOBS_OBRIGATORIOS:
        if f'"{job}"' not in secao:
            erros.append(f"{servico}: evidência não lista o check `{job}`")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", required=True, choices=["main", "readme"])
    args = parser.parse_args()
    erros = []
    for servico in SERVICOS:
        (checar_main if args.check == "main" else checar_readme)(servico, erros)
    return reportar(erros, f"OK {args.check}: {len(SERVICOS)} repositórios")


if __name__ == "__main__":
    sys.exit(main())
