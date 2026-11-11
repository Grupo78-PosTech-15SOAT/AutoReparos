#!/usr/bin/env python3
"""Provas C1-C4, C8 e C10 de infra-ci-observabilidade: lê o ci-cd.yml de cada serviço.

Uso: python scripts/specs/assert_pipelines.py --check <jobs|cobertura|qualidade|execucao-pr|deploy-staging|deploy-producao> [--minimo 80]
"""
import argparse
import re
import sys

from _repositorios import (JOBS_OBRIGATORIOS, SERVICOS, bloco, gh_api, jobs, ler_workflow,
                           nome_completo, reportar)


def branches_do_gatilho(texto, evento):
    on = bloco(texto, "on", 0)
    if on is None:
        return None
    gatilho = bloco(on, evento, 2)
    if gatilho is None:
        return None
    achado = re.search(r"branches:\s*\[([^\]]*)\]", gatilho)
    if not achado:
        return []
    return [b.strip().strip("'\"") for b in achado.group(1).split(",")]


def checar_jobs(servico, texto, erros):
    pr = branches_do_gatilho(texto, "pull_request")
    if pr is None or "main" not in pr:
        erros.append(f"{servico}: pull_request não dispara para main ({pr})")
    push = branches_do_gatilho(texto, "push")
    if push is None or not {"develop", "main"} <= set(push):
        erros.append(f"{servico}: push não dispara em develop e main ({push})")
    existentes = jobs(texto)
    for job in JOBS_OBRIGATORIOS:
        if job not in existentes:
            erros.append(f"{servico}: job `{job}` ausente")


def checar_cobertura(servico, texto, erros, minimo):
    job = jobs(texto).get("test")
    if job is None:
        erros.append(f"{servico}: job `test` ausente")
        return
    if "continue-on-error" in job:
        erros.append(f"{servico}: job `test` usa continue-on-error")
    if "--collect" not in job:
        erros.append(f"{servico}: job `test` não coleta cobertura")
    if "verificar_cobertura.py" not in job or "--minimo" not in job:
        erros.append(f"{servico}: job `test` não verifica a cobertura mínima")
    valor = re.search(r"^\s*MINIMO_COBERTURA:\s*(\d+)", texto, re.MULTILINE)
    if not valor:
        erros.append(f"{servico}: MINIMO_COBERTURA não definido")
    elif int(valor.group(1)) < minimo:
        erros.append(f"{servico}: MINIMO_COBERTURA {valor.group(1)} abaixo de {minimo}")


def checar_qualidade(servico, texto, erros):
    job = jobs(texto).get("quality")
    if job is None:
        erros.append(f"{servico}: job `quality` ausente")
        return
    if "continue-on-error" in job:
        erros.append(f"{servico}: job `quality` usa continue-on-error")
    for trecho in ("sonar.qualitygate.wait=true", "sonarcloud.io", "SONAR_TOKEN"):
        if trecho not in job:
            erros.append(f"{servico}: job `quality` sem `{trecho}`")


def checar_execucao_pr(servico, erros):
    repo = nome_completo(servico)
    status, corpo = gh_api(f"repos/{repo}/actions/workflows/ci-cd.yml/runs?event=pull_request&per_page=1")
    if status != 0 or not corpo or not corpo.get("workflow_runs"):
        erros.append(f"{servico}: nenhuma execução de ci-cd.yml disparada por PR")
        return
    execucao = corpo["workflow_runs"][0]
    status, corpo = gh_api(f"repos/{repo}/actions/runs/{execucao['id']}/jobs")
    conclusoes = {j["name"]: j["conclusion"] for j in (corpo or {}).get("jobs", [])}
    for job in JOBS_OBRIGATORIOS:
        if conclusoes.get(job) != "success":
            erros.append(f"{servico}: job `{job}` da execução {execucao['id']} terminou em {conclusoes.get(job)}")


def checar_deploy(servico, texto, erros, ambiente):
    candidatos = {nome: corpo for nome, corpo in jobs(texto).items() if nome.startswith("deploy")}
    if not candidatos:
        erros.append(f"{servico}: nenhum job de deploy")
        return
    if "id-token: write" not in texto:
        erros.append(f"{servico}: sem permissão id-token: write para OIDC")
    if ambiente == "staging":
        job = next((c for c in candidatos.values() if "staging" in c), None)
        if job is None:
            erros.append(f"{servico}: nenhum job de deploy para staging")
            return
        exigidos = ["refs/heads/develop", "configure-aws-credentials", "helm upgrade --install",
                    "--atomic", "deploy/helm/", "kubectl rollout status"]
    else:
        job = next((c for c in candidatos.values() if "environment: production" in c), None)
        if job is None:
            erros.append(f"{servico}: nenhum job de deploy com environment: production")
            return
        exigidos = ["refs/heads/main", "us-east-1", "helm upgrade --install", "--atomic"]
    for trecho in exigidos:
        if trecho not in job:
            erros.append(f"{servico}: deploy {ambiente} sem `{trecho}`")
    if ambiente == "staging" and not re.search(r"(--namespace|-n)\s+staging", job):
        erros.append(f"{servico}: deploy staging fora do namespace staging")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", required=True,
                        choices=["jobs", "cobertura", "qualidade", "execucao-pr", "deploy-staging", "deploy-producao"])
    parser.add_argument("--minimo", type=int, default=80)
    args = parser.parse_args()

    erros = []
    for servico in SERVICOS:
        if args.check == "execucao-pr":
            checar_execucao_pr(servico, erros)
            continue
        texto = ler_workflow(servico)
        if texto is None:
            erros.append(f"{servico}: .github/workflows/ci-cd.yml ausente")
            continue
        if args.check == "jobs":
            checar_jobs(servico, texto, erros)
        elif args.check == "cobertura":
            checar_cobertura(servico, texto, erros, args.minimo)
        elif args.check == "qualidade":
            checar_qualidade(servico, texto, erros)
        elif args.check == "deploy-staging":
            checar_deploy(servico, texto, erros, "staging")
        else:
            checar_deploy(servico, texto, erros, "producao")
    return reportar(erros, f"OK {args.check}: {len(SERVICOS)} repositórios")


if __name__ == "__main__":
    sys.exit(main())
