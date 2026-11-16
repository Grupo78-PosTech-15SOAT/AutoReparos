#!/usr/bin/env python3
"""Provas dos pipelines: lê o ci-cd.yml de cada serviço.

infra-ci-observabilidade C1-C4 (jobs, cobertura, qualidade, execucao-pr) nos 5 repositórios e deploy-servicos
C18-C20 (deploy-producao, deploy-verifica-saude, sem-deploy-fora-da-main) nos 4 serviços implantados pelo próprio
repositório (o Web continua implantado pelo pai).

Uso: python scripts/specs/assert_pipelines.py --check <jobs|cobertura|qualidade|execucao-pr|deploy-producao|deploy-verifica-saude|sem-deploy-fora-da-main> [--minimo 80]
"""
import argparse
import re
import sys

from _deploy_comum import SERVICOS_IMPLANTADOS
from _repositorios import (JOBS_OBRIGATORIOS, SERVICOS, bloco, gh_api, jobs, ler_workflow,
                           nome_completo, reportar)

CHECKS_DE_DEPLOY = {
    "deploy-producao": lambda servico, texto, erros: checar_deploy_producao(servico, texto, erros),
    "deploy-verifica-saude": lambda servico, texto, erros: checar_deploy_verifica_saude(servico, texto, erros),
    "sem-deploy-fora-da-main": lambda servico, texto, erros: checar_sem_deploy_fora_da_main(servico, texto, erros),
}


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


SHA_FIXO = re.compile(r"^[0-9a-f]{40}$")
MARCAS_DE_PUBLICACAO = ("docker push", "docker login", "aws ecr", "helm upgrade", "kubectl apply", "terraform apply",
                        "configure-aws-credentials", "provisionador-db")


def condicao(job):
    achado = re.search(r"^    if:\s*(.+)$", job, re.MULTILINE)
    return achado.group(1).strip() if achado else ""


def nivel_superior(expressao):
    """Tira ${{ }} e o conteúdo entre parênteses, deixando só os termos de nível superior da expressão."""
    texto = expressao.strip()
    if texto.startswith("${{") and texto.endswith("}}"):
        texto = texto[3:-2]
    anterior = None
    while anterior != texto:
        anterior, texto = texto, re.sub(r"\([^()]*\)", "(...)", texto)
    return texto


def so_na_main(job):
    """A condição exige github.ref == 'refs/heads/main' como termo de um E de nível superior."""
    topo = nivel_superior(condicao(job))
    return "||" not in topo and re.search(r"github\.ref\s*==\s*'refs/heads/main'", topo) is not None


def posicao(texto, trecho):
    indice = texto.find(trecho)
    return indice if indice >= 0 else None


def job_deploy(servico, texto, erros):
    job = jobs(texto).get("deploy")
    if job is None:
        erros.append(f"{servico}: job `deploy` ausente")
    return job


def checar_deploy_producao(servico, texto, erros):
    job = job_deploy(servico, texto, erros)
    if job is None:
        return
    needs = re.search(r"^    needs:\s*\[([^\]]*)\]", job, re.MULTILINE)
    if not needs or {n.strip() for n in needs.group(1).split(",")} != set(JOBS_OBRIGATORIOS):
        erros.append(f"{servico}: deploy precisa de needs: [build, test, quality] ({needs.group(0).strip() if needs else None})")
    if not so_na_main(job) or "develop" in condicao(job):
        erros.append(f"{servico}: deploy não fica restrito a refs/heads/main (if: {condicao(job)!r})")
    if not re.search(r"^    environment:\s*production\s*$", job, re.MULTILINE):
        erros.append(f"{servico}: deploy sem environment: production")
    permissoes = bloco(job, "permissions", 4) or ""
    for trecho in ("id-token: write", "contents: read"):
        if trecho not in permissoes:
            erros.append(f"{servico}: deploy sem a permissão `{trecho}`")
    usos = re.findall(r"uses:\s*([^\s@#]+)@([^\s#]+)", job)
    for acao, versao in usos:
        if not acao.startswith("./") and not SHA_FIXO.match(versao):
            erros.append(f"{servico}: `{acao}@{versao}` não está fixada em SHA de 40 caracteres")
    exigidos = ["us-east-1", "aws-actions/configure-aws-credentials", "role-to-assume: ${{ vars.AWS_ROLE_ARN }}",
                "terraform init", "-backend-config", "vars.TF_STATE_BUCKET", "vars.TF_LOCK_TABLE", "terraform apply",
                "docker build", "docker push", "${{ github.sha }}", "/autoreparos/production/provisionador-db/url",
                "--aws-sigv4", "aws eks update-kubeconfig", f"{servico.lower()}-database", "kubectl apply",
                "helm upgrade --install", "--atomic"]
    for trecho in exigidos:
        if trecho not in job:
            erros.append(f"{servico}: deploy sem `{trecho}`")
    helm = posicao(job, "helm upgrade --install")
    for trecho, descricao in (("/autoreparos/production/provisionador-db/url", "chamada ao provisionador"),
                              ("--aws-sigv4", "chamada assinada ao provisionador"),
                              ("terraform apply", "terraform apply"),
                              ("docker push", "publicação da imagem"),
                              (f"{servico.lower()}-database", f"Secret {servico.lower()}-database")):
        indice = posicao(job, trecho)
        if helm is not None and indice is not None and indice > helm:
            erros.append(f"{servico}: {descricao} acontece depois do helm upgrade")


def checar_deploy_verifica_saude(servico, texto, erros):
    job = job_deploy(servico, texto, erros)
    if job is None:
        return
    helm = posicao(job, "helm upgrade --install")
    rollout = posicao(job, "kubectl rollout status")
    if helm is None:
        erros.append(f"{servico}: deploy sem helm upgrade --install")
        return
    if rollout is None or rollout < helm:
        erros.append(f"{servico}: deploy sem kubectl rollout status depois do helm upgrade")
        return
    saude = job.find("/health", rollout)
    if saude < 0:
        erros.append(f"{servico}: deploy sem GET /health depois do rollout")
        return
    resto = job[saude:]
    # A falha precisa ficar no ramo em que o status é diferente de 200: `if [ "$status" != "200" ]; then ... exit 1`.
    falha = re.search(r'\$status"?\s*(!=|-ne)\s*"?200"?\s*\]?;?\s*then(.*?)\bfi\b', resto, re.DOTALL)
    if not falha:
        erros.append(f"{servico}: o GET /health não compara o status com 200")
    elif "exit 1" not in falha.group(2):
        erros.append(f"{servico}: o GET /health não falha o job quando o status não é 200")


def checar_sem_deploy_fora_da_main(servico, texto, erros):
    for nome, job in jobs(texto).items():
        marcas = [marca for marca in MARCAS_DE_PUBLICACAO if marca in (job or "")]
        if marcas and not so_na_main(job):
            erros.append(f"{servico}: job `{nome}` publica ou implanta ({', '.join(marcas)}) sem ficar restrito "
                         f"a refs/heads/main (if: {condicao(job)!r})")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", required=True,
                        choices=["jobs", "cobertura", "qualidade", "execucao-pr", *CHECKS_DE_DEPLOY])
    parser.add_argument("--minimo", type=int, default=80)
    args = parser.parse_args()

    erros = []
    repositorios = SERVICOS_IMPLANTADOS if args.check in CHECKS_DE_DEPLOY else SERVICOS
    for servico in repositorios:
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
        else:
            CHECKS_DE_DEPLOY[args.check](servico, texto, erros)
    return reportar(erros, f"OK {args.check}: {len(repositorios)} repositórios")


if __name__ == "__main__":
    sys.exit(main())
