#!/usr/bin/env python3
"""Provas de web-deploy (fase 4): chart, pipeline e plataforma do deploy próprio do AutoReparos.Web.

Uso: python3 scripts/specs/assert_web_deploy.py --check <nome>

Checks estáticos (sem AWS): chart, chart-config, deploy-job, deploy-get, deploy-condicao, deploy-concorrencia,
guarda-chuva, pai-sem-web, destroy-web. Checks "no ar" (conta 198903849859 pelo perfil `autoreparos`, cluster
AutoReparosEKS e GitHub): plataforma-no-ar, environment, deploy-no-ar, quality-verde, rota-migrada, raiz-publica.
Cada check devolve a lista de erros; código de saída 0 só sem erros.
"""
import argparse
import json
import os
import re
import shutil
import stat
import subprocess
import sys
import tempfile
import urllib.request

import yaml

from _repositorios import ORG, RAIZ, reportar

WEB = os.path.join(RAIZ, "submodules", "AutoReparos.Web")
K8S = os.path.join(RAIZ, "submodules", "AutoReparos.Infra.K8s")
CHART_WEB = os.path.join(WEB, "deploy", "helm", "web")
GUARDA_CHUVA = os.path.join(K8S, "k8s")
WORKFLOW_WEB = os.path.join(WEB, ".github", "workflows", "ci-cd.yml")
DEPLOY_APP = os.path.join(RAIZ, ".github", "workflows", "deploy-app.yml")
DESTROY_AWS = os.path.join(RAIZ, ".github", "workflows", "destroy-aws.yml")
MODELO_POLITICA = "submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-web.json.tftpl"

CONTA = "198903849859"
REGIAO = "us-east-1"
CLUSTER = "AutoReparosEKS"
PERFIL = os.environ.get("AWS_PROFILE", "autoreparos")
REPO_WEB = f"{ORG}/AutoReparos.Web"
ROLE = "autoreparos-deploy-web"
ROLE_ARN = f"arn:aws:iam::{CONTA}:role/{ROLE}"
SUBS = {
    f"repo:{ORG}/AutoReparos.Web:environment:production",
    f"repo:{ORG}@272379485/AutoReparos.Web@1454153026:environment:production",
}
COLLECTOR = "autoreparos-otel-collector.autoreparos.svc.cluster.local"
INGRESS_ANOTACOES = {
    "alb.ingress.kubernetes.io/group.name": "autoreparos-internal",
    "alb.ingress.kubernetes.io/group.order": "999",
    "alb.ingress.kubernetes.io/scheme": "internal",
    "alb.ingress.kubernetes.io/target-type": "ip",
    "alb.ingress.kubernetes.io/healthcheck-path": "/",
}


# ---------------------------------------------------------------- utilitários

def executar(comando, entrada=None, ambiente=None, diretorio=None):
    processo = subprocess.run(comando, capture_output=True, text=True, encoding="utf-8", input=entrada,
                              env=ambiente, cwd=diretorio)
    return processo.returncode, processo.stdout, processo.stderr


def ler(caminho):
    with open(caminho, encoding="utf-8") as arquivo:
        return arquivo.read()


def helm_template(chart, *argumentos, release="web", namespace="web"):
    codigo, saida, erro = executar(["helm", "template", release, chart, "--namespace", namespace, *argumentos])
    if codigo != 0:
        raise RuntimeError(f"helm template {os.path.relpath(chart, RAIZ)} falhou: {erro.strip()}")
    return [doc for doc in yaml.safe_load_all(saida) if doc]


def objetos(docs, tipo, nome=None):
    return [d for d in docs if d.get("kind") == tipo and (nome is None or d["metadata"]["name"] == nome)]


def carregar_workflow(caminho):
    dados = yaml.safe_load(ler(caminho))
    # O PyYAML lê a chave `on` como o booleano True.
    if True in dados and "on" not in dados:
        dados["on"] = dados.pop(True)
    return dados


def juntar_linhas(texto):
    """Junta as continuações de linha do shell (`\\` no fim) para casar um comando inteiro."""
    return re.sub(r"\\\n\s*", " ", texto)


def expandir(texto, variaveis):
    """Troca `$VAR` e `${VAR}` pelos valores do `env` do job e do passo (o que o runner faria)."""
    def trocar(achado):
        nome = achado.group(1) or achado.group(2)
        return str(variaveis.get(nome, achado.group(0)))
    return re.sub(r"\$\{([A-Za-z_][A-Za-z0-9_]*)\}|\$([A-Za-z_][A-Za-z0-9_]*)", trocar, texto)


# ---------------------------------------------- avaliador de expressões do GitHub

TOKENS = re.compile(r"\s*(\|\||&&|==|!=|!|\(|\)|,|'(?:[^']|'')*'|[A-Za-z_][A-Za-z0-9_.\-]*)")


class Expressao:
    """Avalia o subconjunto das expressões do GitHub Actions que os workflows usam: ||, &&, !, ==, !=,
    parênteses, literais, propriedades (github.ref, needs.x.result, inputs.y) e always()/success()/failure()."""

    def __init__(self, texto, contexto, needs):
        texto = texto.strip()
        if texto.startswith("${{") and texto.endswith("}}"):
            texto = texto[3:-2]
        self.tokens = []
        posicao = 0
        while posicao < len(texto):
            achado = TOKENS.match(texto, posicao)
            if not achado or achado.end() == posicao:
                if texto[posicao:].strip() == "":
                    break
                raise ValueError(f"expressão não suportada perto de {texto[posicao:]!r}")
            self.tokens.append(achado.group(1))
            posicao = achado.end()
        self.posicao = 0
        self.contexto = contexto
        self.needs = needs
        self.funcao_de_status = False

    def avaliar(self):
        valor = self.ou()
        if self.posicao != len(self.tokens):
            raise ValueError(f"sobra na expressão: {self.tokens[self.posicao:]}")
        return valor

    def olhar(self):
        return self.tokens[self.posicao] if self.posicao < len(self.tokens) else None

    def comer(self, esperado=None):
        token = self.olhar()
        if esperado is not None and token != esperado:
            raise ValueError(f"esperado {esperado!r}, veio {token!r}")
        self.posicao += 1
        return token

    def ou(self):
        valor = self.e()
        while self.olhar() == "||":
            self.comer()
            direita = self.e()
            valor = valor if verdadeiro(valor) else direita
        return valor

    def e(self):
        valor = self.comparacao()
        while self.olhar() == "&&":
            self.comer()
            direita = self.comparacao()
            valor = direita if verdadeiro(valor) else valor
        return valor

    def comparacao(self):
        valor = self.unario()
        while self.olhar() in ("==", "!="):
            operador = self.comer()
            direita = self.unario()
            igual = normalizar(valor) == normalizar(direita)
            valor = igual if operador == "==" else not igual
        return valor

    def unario(self):
        if self.olhar() == "!":
            self.comer()
            return not verdadeiro(self.unario())
        return self.primario()

    def primario(self):
        token = self.comer()
        if token == "(":
            valor = self.ou()
            self.comer(")")
            return valor
        if token.startswith("'"):
            return token[1:-1].replace("''", "'")
        if token in ("true", "false"):
            return token == "true"
        if token == "null":
            return None
        if self.olhar() == "(":
            self.comer("(")
            self.comer(")")
            return self.funcao(token)
        return self.propriedade(token)

    def funcao(self, nome):
        resultados = [self.needs.get(job) for job in self.needs]
        self.funcao_de_status = True
        if nome == "always":
            return True
        if nome == "success":
            return all(r == "success" for r in resultados)
        if nome == "failure":
            return any(r == "failure" for r in resultados)
        if nome == "cancelled":
            return False
        raise ValueError(f"função não suportada: {nome}()")

    def propriedade(self, caminho):
        partes = caminho.split(".")
        if partes[0] == "needs" and len(partes) == 3 and partes[2] == "result":
            return self.needs.get(partes[1])
        valor = self.contexto
        for parte in partes:
            valor = valor.get(parte) if isinstance(valor, dict) else None
        return valor


def verdadeiro(valor):
    return valor not in (None, False, "", 0)


def normalizar(valor):
    return valor.lower() if isinstance(valor, str) else valor


def condicao_do_job(job, contexto, needs):
    """Diz se o job roda: sem função de status na condição, o GitHub acrescenta `success() &&`."""
    texto = job.get("if")
    if texto is None:
        return all(needs.get(j) == "success" for j in needs)
    expressao = Expressao(str(texto), contexto, needs)
    valor = verdadeiro(expressao.avaliar())
    if not expressao.funcao_de_status:
        valor = valor and all(needs.get(j) == "success" for j in needs)
    return valor


def avaliar(texto, contexto, needs=None):
    return Expressao(str(texto), contexto, needs or {}).avaliar()


# -------------------------------------------------------------- checks do chart

def checar_chart():
    erros = []
    if not os.path.isfile(os.path.join(CHART_WEB, "Chart.yaml")):
        return ["deploy/helm/web/Chart.yaml ausente no AutoReparos.Web"]
    codigo, saida, erro = executar(["helm", "lint", CHART_WEB])
    if codigo != 0:
        erros.append(f"helm lint falhou: {saida.strip()} {erro.strip()}")
    docs = helm_template(CHART_WEB, "--set-string", "image.repository=r", "--set-string", "image.tag=t")
    tipos = sorted(f"{d['kind']}/{d['metadata']['name']}" for d in docs)
    esperados = ["Deployment/autoreparos-web", "Ingress/autoreparos-web-ingress", "Service/autoreparos-web"]
    if tipos != esperados:
        erros.append(f"objetos renderizados {tipos}, esperado {esperados}")
    for servico in objetos(docs, "Service", "autoreparos-web"):
        spec = servico["spec"]
        portas = spec.get("ports") or []
        if spec.get("type", "ClusterIP") != "ClusterIP":
            erros.append(f"Service do tipo {spec.get('type')}, esperado ClusterIP")
        if len(portas) != 1 or portas[0].get("port") != 80 or str(portas[0].get("targetPort")) != "8080":
            erros.append(f"Service com portas {portas}, esperado 80 -> 8080")
    for ingress in objetos(docs, "Ingress", "autoreparos-web-ingress"):
        anotacoes = ingress["metadata"].get("annotations") or {}
        for chave, valor in INGRESS_ANOTACOES.items():
            if str(anotacoes.get(chave)) != valor:
                erros.append(f"Ingress: {chave}={anotacoes.get(chave)!r}, esperado {valor!r}")
        spec = ingress["spec"]
        if spec.get("ingressClassName") != "alb":
            erros.append(f"Ingress: ingressClassName {spec.get('ingressClassName')!r}, esperado 'alb'")
        caminhos = [c for regra in spec.get("rules") or [] for c in (regra.get("http") or {}).get("paths") or []]
        esperado = [{"path": "/", "pathType": "Prefix",
                     "backend": {"service": {"name": "autoreparos-web", "port": {"number": 80}}}}]
        if caminhos != esperado or len(spec.get("rules") or []) != 1:
            erros.append(f"Ingress: regras {caminhos}, esperado só / Prefix -> autoreparos-web:80")
    return erros


def container_web(docs):
    deploys = objetos(docs, "Deployment", "autoreparos-web")
    if len(deploys) != 1:
        raise RuntimeError("Deployment autoreparos-web não renderizado")
    containers = deploys[0]["spec"]["template"]["spec"]["containers"]
    return next((c for c in containers if c.get("name") == "web"), None), deploys[0]


def checar_chart_config():
    erros = []
    docs = helm_template(CHART_WEB, "--set-string", "image.repository=R", "--set-string", "image.tag=T")
    container, _ = container_web(docs)
    if container is None:
        return ["Deployment sem container 'web'"]
    variaveis = {e["name"]: e.get("value") for e in container.get("env") or []}
    if variaveis.get("OTEL_COLLECTOR_HOST") != COLLECTOR:
        erros.append(f"OTEL_COLLECTOR_HOST={variaveis.get('OTEL_COLLECTOR_HOST')!r}, esperado {COLLECTOR!r}")
    if "API_URL" not in variaveis or variaveis["API_URL"] not in ("", None):
        erros.append(f"API_URL={variaveis.get('API_URL', '<ausente>')!r}, esperado vazio")
    if container.get("imagePullPolicy") != "Always":
        erros.append(f"imagePullPolicy={container.get('imagePullPolicy')!r}, esperado 'Always'")
    if container.get("image") != "R:T":
        erros.append(f"imagem {container.get('image')!r}, esperado 'R:T' a partir de image.repository/tag")
    portas = [p.get("containerPort") for p in container.get("ports") or []]
    if portas != [8080]:
        erros.append(f"portas do container {portas}, esperado [8080]")
    for sonda in ("readinessProbe", "livenessProbe"):
        caminho = ((container.get(sonda) or {}).get("httpGet") or {}).get("path")
        if caminho != "/health":
            erros.append(f"{sonda} em {caminho!r}, esperado '/health'")
    recursos = container.get("resources") or {}
    esperado = {"requests": {"cpu": "50m", "memory": "64Mi"}, "limits": {"cpu": "250m", "memory": "128Mi"}}
    if recursos != esperado:
        erros.append(f"resources {recursos}, esperado {esperado}")
    return erros


# ----------------------------------------------------------- checks do pipeline

def job_deploy():
    workflow = carregar_workflow(WORKFLOW_WEB)
    job = (workflow.get("jobs") or {}).get("deploy")
    if job is None:
        raise RuntimeError("ci-cd.yml do Web sem job deploy")
    return workflow, job


def passos_com_env(workflow, job):
    """Lista (texto do run já expandido e sem continuações, passo) de cada passo."""
    base = {**(workflow.get("env") or {}), **(job.get("env") or {})}
    resultado = []
    for passo in job.get("steps") or []:
        variaveis = {**base, **(passo.get("env") or {})}
        texto = juntar_linhas(str(passo.get("run") or ""))
        resultado.append((expandir(texto, variaveis), passo))
    return resultado


def checar_deploy_job():
    erros = []
    workflow, job = job_deploy()
    needs = job.get("needs")
    needs = [needs] if isinstance(needs, str) else list(needs or [])
    if sorted(needs) != ["build", "quality", "test"]:
        erros.append(f"needs {needs}, esperado [build, test, quality]")
    ambiente = job.get("environment")
    nome_ambiente = ambiente.get("name") if isinstance(ambiente, dict) else ambiente
    if nome_ambiente != "production":
        erros.append(f"environment {ambiente!r}, esperado 'production'")
    if (job.get("permissions") or {}).get("id-token") != "write":
        erros.append(f"permissions {job.get('permissions')}, esperado id-token: write")
    passos = passos_com_env(workflow, job)
    oidc = [i for i, (_, p) in enumerate(passos) if str(p.get("uses", "")).startswith("aws-actions/configure-aws-credentials@")]
    if len(oidc) != 1:
        erros.append("esperado exatamente um passo aws-actions/configure-aws-credentials")
    else:
        com = passos[oidc[0]][1].get("with") or {}
        if com.get("role-to-assume") != "${{ vars.AWS_ROLE_ARN }}":
            erros.append(f"role-to-assume {com.get('role-to-assume')!r}, esperado '${{{{ vars.AWS_ROLE_ARN }}}}'")
        if com.get("audience") != "sts.amazonaws.com":
            erros.append(f"audience {com.get('audience')!r}, esperado 'sts.amazonaws.com'")

    def achar(padrao):
        for indice, (texto, _) in enumerate(passos):
            for linha in texto.splitlines():
                if re.search(padrao, linha):
                    return indice, linha
        return None, None

    sha = re.escape("${{ github.sha }}")
    indice_build, build = achar(rf"docker build\b.*autoreparos-web[^\s]*:{sha}")
    if build is None:
        erros.append("sem `docker build` com a tag `${{ github.sha }}` no repositório autoreparos-web")
    elif re.search(r"\s-f\s", build) and not re.search(r"\s-f\s+(\./)?Dockerfile(\s|$)", build):
        erros.append(f"docker build não usa o Dockerfile da raiz: {build.strip()}")
    elif not re.search(r"\s\.\s*$", build):
        erros.append(f"docker build sem o contexto da raiz do repositório: {build.strip()}")
    indice_push, push = achar(rf"docker push\b.*autoreparos-web[^\s]*:{sha}")
    if push is None:
        erros.append("sem `docker push` da tag `${{ github.sha }}` no repositório autoreparos-web")
    indice_eks, eks = achar(r"aws eks update-kubeconfig\b.*--name\s+['\"]?\$\{\{ vars\.EKS_CLUSTER_NAME \}\}")
    if eks is None:
        erros.append("sem `aws eks update-kubeconfig --name` com `vars.EKS_CLUSTER_NAME`")
    indice_helm, helm = achar(r"helm upgrade --install web deploy/helm/web\b")
    if helm is None:
        erros.append("sem `helm upgrade --install web deploy/helm/web`")
    else:
        for opcao in (r"--namespace web\b", r"--atomic\b", r"--wait\b", r"--timeout 10m\b",
                      r"image\.repository=\S*autoreparos-web", rf"image\.tag={sha}"):
            if not re.search(opcao, helm):
                erros.append(f"helm upgrade sem {opcao}: {helm.strip()}")
    indice_rollout, rollout = achar(r"kubectl rollout status deployment/autoreparos-web -n web\b")
    if rollout is None:
        erros.append("sem `kubectl rollout status deployment/autoreparos-web -n web`")
    ordem = [("OIDC", oidc[0] if oidc else None), ("docker build", indice_build), ("docker push", indice_push),
             ("update-kubeconfig", indice_eks), ("helm upgrade", indice_helm), ("rollout", indice_rollout)]
    presentes = [(nome, i) for nome, i in ordem if i is not None]
    if [i for _, i in presentes] != sorted(i for _, i in presentes):
        erros.append(f"ordem dos passos errada: {presentes}")
    return erros


def falso_executavel(pasta, nome, corpo):
    caminho = os.path.join(pasta, nome)
    with open(caminho, "w", encoding="utf-8") as arquivo:
        arquivo.write("#!/usr/bin/env bash\n" + corpo + "\n")
    os.chmod(caminho, os.stat(caminho).st_mode | stat.S_IEXEC)


def rodar_passo_get(script, status_http):
    """Roda o passo do GET com kubectl, curl e sleep falsos; devolve (código, chamadas do curl, urls)."""
    with tempfile.TemporaryDirectory() as pasta:
        registro = os.path.join(pasta, "curl.log")
        falso_executavel(pasta, "kubectl", 'case "$1" in port-forward) echo "$@" >> "$KUBECTL_LOG"; exec /bin/sleep 30;; '
                                          '*) echo "$@" >> "$KUBECTL_LOG"; exit 0;; esac')
        falso_executavel(pasta, "curl", f'echo "$@" >> "{registro}"; printf "%s" "{status_http}"')
        falso_executavel(pasta, "sleep", 'exit 0')
        ambiente = {**os.environ, "PATH": f"{pasta}:{os.environ['PATH']}", "RUNNER_TEMP": pasta,
                    "KUBECTL_LOG": os.path.join(pasta, "kubectl.log")}
        codigo, _, _ = executar(["bash", "-e", "-c", script], ambiente=ambiente)
        chamadas = ler(registro).splitlines() if os.path.exists(registro) else []
        kubectl = ler(ambiente["KUBECTL_LOG"]).splitlines() if os.path.exists(ambiente["KUBECTL_LOG"]) else []
        return codigo, chamadas, kubectl


def checar_deploy_get():
    erros = []
    workflow, job = job_deploy()
    passos = [(texto, passo) for texto, passo in passos_com_env(workflow, job) if "port-forward" in texto]
    if len(passos) != 1:
        return [f"esperado um passo com kubectl port-forward, achados {len(passos)}"]
    script = passos[0][0]
    script = re.sub(r"\$\{\{[^}]*\}\}", "x", script)
    codigo, chamadas, kubectl = rodar_passo_get(script, "200")
    if codigo != 0:
        erros.append(f"com GET / = 200 o passo saiu com {codigo}")
    if not any(re.search(r"\bport-forward\b", k) and re.search(r"(^|\s)service/autoreparos-web(\s|$)", k)
               and re.search(r"(^|\s)(-n|--namespace)[ =]web(\s|$)", k) for k in kubectl):
        erros.append(f"port-forward não aponta para service/autoreparos-web no namespace web: {kubectl}")
    urls = [re.findall(r"https?://\S+", c) for c in chamadas]
    if not chamadas or not all(u and re.fullmatch(r"https?://(127\.0\.0\.1|localhost):\d+/", u[-1]) for u in urls):
        erros.append(f"o curl não faz GET / no port-forward local: {chamadas}")
    codigo, chamadas, _ = rodar_passo_get(script, "503")
    if codigo == 0:
        erros.append("com GET / = 503 o passo saiu com 0")
    if len(chamadas) != 15:
        erros.append(f"com GET / = 503 o curl foi chamado {len(chamadas)} vezes, esperado 15")
    codigo, chamadas, _ = rodar_passo_get(script, "000")
    if codigo == 0:
        erros.append("sem resposta (000) o passo saiu com 0")
    return erros


CASOS_DEPLOY = [
    # (rótulo, evento, ref, build, test, quality, ignorar_quality, roda?)
    ("push main verde", "push", "refs/heads/main", "success", "success", "success", None, True),
    ("push main quality vermelho", "push", "refs/heads/main", "success", "success", "failure", None, False),
    ("dispatch main com input e quality vermelho", "workflow_dispatch", "refs/heads/main", "success", "success", "failure", True, True),
    ("dispatch main sem input e quality vermelho", "workflow_dispatch", "refs/heads/main", "success", "success", "failure", False, False),
    ("dispatch main com input e test vermelho", "workflow_dispatch", "refs/heads/main", "success", "failure", "skipped", True, False),
    ("dispatch main com input e build vermelho", "workflow_dispatch", "refs/heads/main", "failure", "skipped", "skipped", True, False),
    ("dispatch main verde sem input", "workflow_dispatch", "refs/heads/main", "success", "success", "success", False, True),
    ("dispatch develop com input", "workflow_dispatch", "refs/heads/develop", "success", "success", "skipped", True, False),
    ("push develop", "push", "refs/heads/develop", "success", "success", "skipped", None, False),
    ("pull_request para main", "pull_request", "refs/pull/3/merge", "success", "success", "success", None, False),
]


def contexto(evento, ref, entrada=None, base_ref=""):
    return {"github": {"event_name": evento, "ref": ref, "base_ref": base_ref},
            "inputs": {} if entrada is None else {"ignorar_quality": entrada}}


def checar_deploy_condicao():
    erros = []
    workflow, job = job_deploy()
    gatilho = (workflow.get("on") or {}).get("workflow_dispatch") or {}
    entrada = ((gatilho.get("inputs") or {}).get("ignorar_quality")) or {}
    if entrada.get("type") != "boolean" or entrada.get("default") is not False:
        erros.append(f"input ignorar_quality {entrada}, esperado type: boolean e default: false")
    for rotulo, evento, ref, build, test, quality, ignorar, esperado in CASOS_DEPLOY:
        base = "main" if evento == "pull_request" else ""
        needs = {"build": build, "test": test, "quality": quality}
        try:
            roda = condicao_do_job(job, contexto(evento, ref, ignorar, base), needs)
        except ValueError as erro:
            return erros + [f"condição do deploy: {erro}"]
        if roda != esperado:
            erros.append(f"{rotulo}: deploy {'roda' if roda else 'pulado'}, esperado {'roda' if esperado else 'pulado'}")
        else:
            print(f"ok {rotulo}: {'roda' if roda else 'pulado'}")
    return erros


def checar_deploy_concorrencia():
    erros = []
    workflow, job = job_deploy()
    concorrencia = job.get("concurrency") or {}
    if concorrencia.get("group") != "deploy-web-production" or concorrencia.get("cancel-in-progress") is not False:
        erros.append(f"concurrency do deploy {concorrencia}, esperado group deploy-web-production e cancel-in-progress false")
    geral = workflow.get("concurrency") or {}
    for ref, esperado in (("refs/heads/main", False), ("refs/heads/develop", True)):
        valor = geral.get("cancel-in-progress")
        cancela = verdadeiro(avaliar(valor, contexto("push", ref))) if isinstance(valor, str) else bool(valor)
        if cancela != esperado:
            erros.append(f"concorrência do workflow em {ref}: cancel-in-progress {cancela}, esperado {esperado}")
    gatilhos = workflow.get("on") or {}
    if "develop" not in ((gatilhos.get("push") or {}).get("branches") or []):
        erros.append("push na develop não dispara o workflow")
    if "main" not in ((gatilhos.get("pull_request") or {}).get("branches") or []):
        erros.append("PR para a main não dispara o workflow")
    jobs = workflow.get("jobs") or {}
    cenarios = [
        ("push main", contexto("push", "refs/heads/main"), {"build", "test", "quality"}),
        ("dispatch main", contexto("workflow_dispatch", "refs/heads/main", False), {"build", "test", "quality"}),
        ("PR para main", contexto("pull_request", "refs/pull/3/merge", base_ref="main"), {"build", "test", "quality"}),
        ("push develop", contexto("push", "refs/heads/develop"), {"build", "test"}),
    ]
    for rotulo, ctx, esperados in cenarios:
        resultados = {}
        for nome in ("build", "test", "quality", "deploy"):
            dados = jobs.get(nome) or {}
            needs = dados.get("needs") or []
            needs = [needs] if isinstance(needs, str) else needs
            estado = {n: resultados.get(n, "skipped") for n in needs}
            resultados[nome] = "success" if condicao_do_job(dados, ctx, estado) else "skipped"
        rodam = {n for n, r in resultados.items() if r == "success" and n != "deploy"}
        if rodam != esperados:
            erros.append(f"{rotulo}: rodam {sorted(rodam)}, esperado {sorted(esperados)}")
        if rotulo in ("push develop", "PR para main") and resultados["deploy"] == "success":
            erros.append(f"{rotulo}: o deploy roda")
    return erros


# ---------------------------------------------------------------- checks do pai

def checar_guarda_chuva():
    erros = []
    chart = yaml.safe_load(ler(os.path.join(GUARDA_CHUVA, "Chart.yaml")))
    web = next((d for d in chart.get("dependencies") or [] if d.get("name") == "web"), None)
    if web is None or web.get("condition") != "web.enabled":
        erros.append(f"dependência web do k8s/Chart.yaml {web}, esperado condition: web.enabled")
    producao = yaml.safe_load(ler(os.path.join(GUARDA_CHUVA, "values-production.yaml"))) or {}
    if ((producao.get("web") or {}).get("enabled")) is not False:
        erros.append("values-production.yaml sem web.enabled: false")
    valores = ["-f", os.path.join(GUARDA_CHUVA, "values.yaml"), "-f", os.path.join(GUARDA_CHUVA, "values-production.yaml")]
    desligado = helm_template(GUARDA_CHUVA, *valores, release="autoreparos", namespace="autoreparos")
    nomes = [f"{d['kind']}/{d['metadata']['name']}" for d in desligado if "autoreparos-web" in d["metadata"]["name"]]
    if nomes:
        erros.append(f"com web.enabled false o guarda-chuva ainda renderiza {nomes}")
    ligado = helm_template(GUARDA_CHUVA, *valores, "--set", "web.enabled=true",
                           release="autoreparos", namespace="autoreparos")
    for tipo, nome in (("Deployment", "autoreparos-web"), ("Service", "autoreparos-web"),
                       ("Ingress", "autoreparos-web-ingress")):
        if not objetos(ligado, tipo, nome):
            erros.append(f"com web.enabled true o guarda-chuva não renderiza {tipo}/{nome}")
    for ingress in objetos(ligado, "Ingress", "autoreparos-web-ingress"):
        ordem = (ingress["metadata"].get("annotations") or {}).get("alb.ingress.kubernetes.io/group.order")
        if str(ordem) != "1000":
            erros.append(f"Ingress do guarda-chuva com group.order {ordem!r}, esperado '1000'")
    return erros


def checar_pai_sem_web():
    erros = []
    texto = ler(DEPLOY_APP)
    for proibido in (r"submodules/AutoReparos\.Web", r"ecr_web_repository_url", r"web\.image",
                     r"deployment/autoreparos-web", r"ECR_WEB"):
        if re.search(proibido, texto):
            erros.append(f"deploy-app.yml ainda tem {proibido}")
    for comando in re.findall(r"docker (?:build|push)[^\n]*", texto):
        erros.append(f"deploy-app.yml ainda monta ou publica imagem: {comando.strip()}")
    helm = re.search(r"helm upgrade --install autoreparos submodules/AutoReparos\.Infra\.K8s/k8s\b", juntar_linhas(texto))
    if helm is None:
        erros.append("deploy-app.yml sem `helm upgrade --install autoreparos submodules/AutoReparos.Infra.K8s/k8s`")
    junto = juntar_linhas(texto)
    for arquivo in ("k8s/values.yaml", "k8s/values-production.yaml"):
        if not re.search(rf"-f submodules/AutoReparos\.Infra\.K8s/{re.escape(arquivo)}\b", junto):
            erros.append(f"helm upgrade do pai sem -f .../{arquivo}")
    return erros


def checar_destroy_web():
    texto = ler(DESTROY_AWS)
    espera = texto.find("describe-load-balancers")
    if espera < 0:
        return ["destroy-aws.yml sem a espera pelos load balancers (describe-load-balancers)"]
    antes = texto[:espera]
    erros = []
    explicito_helm = re.search(r"helm uninstall web --namespace web\b", antes)
    explicito_ingress = re.search(r"kubectl delete ingress --all --namespace web\b", antes)
    laco = None
    for achado in re.finditer(r"for (\w+) in ([^;\n]+);\s*do(.*?)done", antes, re.S):
        variavel, lista, corpo = achado.groups()
        if "web" in lista.split():
            laco = (variavel, corpo)
    if laco and not (explicito_helm and explicito_ingress):
        variavel, corpo = laco
        ref = rf'"?\$\{{?{variavel}\}}?"?'
        explicito_helm = explicito_helm or re.search(rf"helm uninstall {ref} --namespace {ref}", corpo)
        explicito_ingress = explicito_ingress or re.search(rf"kubectl delete ingress --all --namespace {ref}", corpo)
    if not explicito_helm:
        erros.append("destroy-aws.yml não roda `helm uninstall web --namespace web` antes da espera dos load balancers")
    if not explicito_ingress:
        erros.append("destroy-aws.yml não apaga os Ingress do namespace web antes da espera dos load balancers")
    return erros


# --------------------------------------------------------------- checks no ar

def aws(*argumentos):
    codigo, saida, erro = executar(["aws", *argumentos, "--profile", PERFIL, "--region", REGIAO, "--output", "json"])
    if codigo != 0:
        raise RuntimeError(f"aws {' '.join(argumentos[:2])}: {erro.strip()}")
    return json.loads(saida) if saida.strip() else None


class Cluster:
    """kubeconfig temporário do AutoReparosEKS, sem mexer no ~/.kube/config."""

    def __enter__(self):
        self.pasta = tempfile.mkdtemp()
        self.config = os.path.join(self.pasta, "kubeconfig")
        codigo, _, erro = executar(["aws", "eks", "update-kubeconfig", "--name", CLUSTER, "--region", REGIAO,
                                    "--profile", PERFIL, "--kubeconfig", self.config])
        if codigo != 0:
            raise RuntimeError(f"update-kubeconfig: {erro.strip()}")
        self.ambiente = {**os.environ, "KUBECONFIG": self.config, "AWS_PROFILE": PERFIL}
        return self

    def __exit__(self, *_):
        shutil.rmtree(self.pasta, ignore_errors=True)

    def json(self, *comando):
        codigo, saida, erro = executar(list(comando), ambiente=self.ambiente)
        if codigo != 0:
            raise RuntimeError(f"{' '.join(comando[:3])}: {erro.strip()}")
        return json.loads(saida)

    def codigo(self, *comando):
        return executar(list(comando), ambiente=self.ambiente)


def checar_plataforma_no_ar():
    erros = []
    role = aws("iam", "get-role", "--role-name", ROLE)["Role"]
    documento = role["AssumeRolePolicyDocument"]
    declaracoes = documento.get("Statement") or []
    if len(declaracoes) != 1:
        erros.append(f"trust com {len(declaracoes)} declarações, esperado 1")
    else:
        condicao = declaracoes[0].get("Condition", {}).get("StringEquals", {})
        subs = condicao.get("token.actions.githubusercontent.com:sub")
        subs = {subs} if isinstance(subs, str) else set(subs or [])
        if subs != SUBS:
            erros.append(f"sub do trust {sorted(subs)}, esperado {sorted(SUBS)}")
        if condicao.get("token.actions.githubusercontent.com:aud") != "sts.amazonaws.com":
            erros.append("trust sem aud sts.amazonaws.com")
    politicas = aws("iam", "list-role-policies", "--role-name", ROLE)["PolicyNames"]
    anexadas = aws("iam", "list-attached-role-policies", "--role-name", ROLE)["AttachedPolicies"]
    if politicas != [ROLE] or anexadas:
        erros.append(f"políticas da role: inline {politicas}, anexadas {anexadas}; esperado só a inline {ROLE}")
    else:
        sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
        from simular_iam import renderizar
        esperada = json.loads(renderizar(MODELO_POLITICA, {"conta": CONTA, "regiao": REGIAO, "cluster": CLUSTER}))
        atual = aws("iam", "get-role-policy", "--role-name", ROLE, "--policy-name", ROLE)["PolicyDocument"]
        if atual != esperada:
            erros.append("a política inline da role difere da renderizada de deploy-web.json.tftpl")
    associacoes = aws("eks", "list-associated-access-policies", "--cluster-name", CLUSTER,
                      "--principal-arn", ROLE_ARN)["associatedAccessPolicies"]
    resumo = [(a["policyArn"].rsplit("/", 1)[-1], a["accessScope"]["type"], sorted(a["accessScope"].get("namespaces") or []))
              for a in associacoes]
    if resumo != [("AmazonEKSEditPolicy", "namespace", ["web"])]:
        erros.append(f"access policies da role no cluster {resumo}, esperado só AmazonEKSEditPolicy no namespace web")
    with Cluster() as cluster:
        namespace = cluster.json("kubectl", "get", "namespace", "web", "-o", "json")
        rotulo = (namespace["metadata"].get("labels") or {}).get("app.kubernetes.io/part-of")
        if rotulo != "autoreparos":
            erros.append(f"namespace web com part-of={rotulo!r}, esperado 'autoreparos'")
    return erros


def gh_json(*argumentos):
    codigo, saida, erro = executar(["gh", *argumentos])
    if codigo != 0:
        raise RuntimeError(f"gh {' '.join(argumentos[:3])}: {erro.strip()}")
    return json.loads(saida) if saida.strip() else None


def checar_environment():
    erros = []
    ambiente = gh_json("api", f"repos/{REPO_WEB}/environments/production")
    if (ambiente or {}).get("name") != "production":
        return ["Environment production ausente no AutoReparos.Web"]
    variaveis = gh_json("api", f"repos/{REPO_WEB}/environments/production/variables")
    valores = {v["name"]: v["value"] for v in (variaveis or {}).get("variables", [])}
    esperado = {"AWS_ROLE_ARN": ROLE_ARN, "AWS_REGION": REGIAO, "EKS_CLUSTER_NAME": CLUSTER}
    for nome, valor in esperado.items():
        if valores.get(nome) != valor:
            erros.append(f"variável {nome}={valores.get(nome)!r}, esperado {valor!r}")
    return erros


def ultimo_run_main(eventos):
    runs = gh_json("run", "list", "--repo", REPO_WEB, "--workflow", "ci-cd.yml", "--branch", "main",
                   "--status", "completed", "--limit", "20", "--json", "databaseId,headSha,event,conclusion")
    runs = [r for r in runs or [] if r["event"] in eventos]
    if not runs:
        raise RuntimeError(f"nenhum run concluído do ci-cd.yml na main com evento {eventos}")
    run = runs[0]
    jobs = gh_json("run", "view", str(run["databaseId"]), "--repo", REPO_WEB, "--json", "jobs")["jobs"]
    return run, {j["name"]: j for j in jobs}


def checar_deploy_no_ar():
    erros = []
    run, jobs = ultimo_run_main({"push", "workflow_dispatch"})
    deploy = jobs.get("deploy")
    if deploy is None or deploy.get("conclusion") != "success":
        return [f"run {run['databaseId']}: job deploy {deploy and deploy.get('conclusion')}, esperado success"]
    codigo, log, erro = executar(["gh", "run", "view", "--repo", REPO_WEB, "--job", str(deploy["databaseId"]), "--log"])
    if codigo != 0 or "GET / respondeu 200" not in log:
        erros.append(f"log do deploy do run {run['databaseId']} sem 'GET / respondeu 200'")
    with Cluster() as cluster:
        deployment = cluster.json("kubectl", "get", "deployment", "autoreparos-web", "-n", "web", "-o", "json")
        imagens = [c["image"] for c in deployment["spec"]["template"]["spec"]["containers"]]
        if not any(re.search(rf"/autoreparos-web:{run['headSha']}$", i) for i in imagens):
            erros.append(f"Deployment web/autoreparos-web com imagens {imagens}, esperado a tag {run['headSha']}")
        codigo, _, erro = cluster.codigo("kubectl", "rollout", "status", "deployment/autoreparos-web", "-n", "web",
                                         "--timeout=60s")
        if codigo != 0:
            erros.append(f"rollout de web/autoreparos-web incompleto: {erro.strip()}")
    print(f"run {run['databaseId']} ({run['event']}, {run['headSha'][:7]})")
    return erros


def checar_quality_verde():
    erros = []
    run, jobs = ultimo_run_main({"push"})
    quality = jobs.get("quality")
    if quality is None or quality.get("conclusion") != "success":
        erros.append(f"run {run['databaseId']}: quality {quality and quality.get('conclusion')}, esperado success")
    url = ("https://sonarcloud.io/api/qualitygates/project_status"
           "?projectKey=Grupo78-PosTech-15SOAT_AutoReparos.Web&branch=main")
    with urllib.request.urlopen(url, timeout=30) as resposta:
        status = json.load(resposta)["projectStatus"]["status"]
    if status != "OK":
        erros.append(f"quality gate da main no SonarCloud: {status}, esperado OK")
    print(f"run {run['databaseId']} ({run['headSha'][:7]}), quality gate {status}")
    return erros


def checar_rota_migrada():
    erros = []
    with Cluster() as cluster:
        ingresses = cluster.json("kubectl", "get", "ingress", "-A", "-o", "json")["items"]
        do_web = [i for i in ingresses if i["metadata"]["name"] == "autoreparos-web-ingress"]
        namespaces = sorted(i["metadata"]["namespace"] for i in do_web)
        if namespaces != ["web"]:
            erros.append(f"Ingress autoreparos-web-ingress nos namespaces {namespaces}, esperado só ['web']")
        for ingress in do_web:
            anotacoes = ingress["metadata"].get("annotations") or {}
            endereco = ((ingress.get("status") or {}).get("loadBalancer") or {}).get("ingress") or []
            if anotacoes.get("alb.ingress.kubernetes.io/group.name") != "autoreparos-internal" or not endereco:
                erros.append(f"Ingress {ingress['metadata']['namespace']}/autoreparos-web-ingress sem ALB do grupo autoreparos-internal")
        codigo, _, _ = cluster.codigo("kubectl", "get", "deployment", "autoreparos-web", "-n", "autoreparos")
        if codigo == 0:
            erros.append("Deployment autoreparos-web ainda existe no namespace autoreparos")
        valores = cluster.json("helm", "get", "values", "autoreparos", "-n", "autoreparos", "--all", "-o", "json")
        if ((valores.get("web") or {}).get("enabled")) is not False:
            erros.append(f"release autoreparos com web.enabled={((valores.get('web') or {}).get('enabled'))!r}, esperado false")
        historico_web = cluster.json("helm", "history", "web", "-n", "web", "-o", "json")
        historico_pai = cluster.json("helm", "history", "autoreparos", "-n", "autoreparos", "-o", "json")
        primeiro_web = min((h["updated"] for h in historico_web if h["status"] in ("deployed", "superseded")), default=None)
        ultimo_pai = max(h["updated"] for h in historico_pai)
        if primeiro_web is None:
            erros.append("release web sem revisão implantada")
        elif ultimo_pai <= primeiro_web:
            erros.append(f"última revisão do autoreparos ({ultimo_pai}) não é posterior ao primeiro deploy do web ({primeiro_web})")
    return erros


def checar_raiz_publica():
    endpoint = aws("ssm", "get-parameter", "--name", "/autoreparos/production/apigateway/endpoint")["Parameter"]["Value"]
    with urllib.request.urlopen(endpoint.rstrip("/") + "/", timeout=30) as resposta:
        corpo = resposta.read().decode("utf-8", "replace")
        status = resposta.status
    erros = []
    if status != 200:
        erros.append(f"GET / público respondeu {status}")
    if "<app-root" not in corpo:
        erros.append("GET / público sem <app-root")
    return erros


CHECKS = {
    "chart": checar_chart,
    "chart-config": checar_chart_config,
    "deploy-job": checar_deploy_job,
    "deploy-get": checar_deploy_get,
    "deploy-condicao": checar_deploy_condicao,
    "deploy-concorrencia": checar_deploy_concorrencia,
    "guarda-chuva": checar_guarda_chuva,
    "pai-sem-web": checar_pai_sem_web,
    "destroy-web": checar_destroy_web,
    "plataforma-no-ar": checar_plataforma_no_ar,
    "environment": checar_environment,
    "deploy-no-ar": checar_deploy_no_ar,
    "quality-verde": checar_quality_verde,
    "rota-migrada": checar_rota_migrada,
    "raiz-publica": checar_raiz_publica,
}


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", required=True, choices=sorted(CHECKS))
    args = parser.parse_args()
    try:
        erros = CHECKS[args.check]()
    except (OSError, RuntimeError, ValueError, KeyError) as erro:
        erros = [f"{type(erro).__name__}: {erro}"]
    return reportar(erros, f"OK assert_web_deploy --check {args.check}")


if __name__ == "__main__":
    sys.exit(main())
