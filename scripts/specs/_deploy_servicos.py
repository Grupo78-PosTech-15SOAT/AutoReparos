"""Checks de deploy-servicos dos repositórios de serviço: Terraform (S2), charts (S3), filas (C35) e staging (C21).

Cada check é uma função sem argumentos que devolve (erros, mensagem de sucesso) e entra em CHECKS.
Os checks de chart rodam `helm template` com valores de exemplo, sem cluster.
"""
import fnmatch
import json
import os
import re
import shutil
import subprocess
import tempfile

from _deploy_comum import SERVICOS_IMPLANTADOS, executar, ultimas_linhas
from _repositorios import RAIZ, caminho_repositorio

CONTA = "198903849859"
REGIAO = "us-east-1"
GRUPO_ALB = "autoreparos-internal"
CREDENCIAL_ADMIN = "autoreparos/production/database/credentials"
MODELO_DEPLOY = os.path.join("submodules", "AutoReparos.Infra.K8s", "terraform", "stacks", "base", "politicas",
                             "deploy-servico.json.tftpl")

# Tabela de roteamento do Surface do plano e group.order do contrato.
PREFIXOS = {
    "OS": {"/api/v1/ordens-servico", "/api/v1/admin/sagas"},
    "Cobranca": {"/api/v1/orcamentos", "/api/v1/pagamentos", "/api/v1/webhooks/mercado-pago"},
    "Execucao": {"/api/v1/execucoes", "/api/v1/fila-execucao", "/api/v1/servicos", "/api/v1/insumos"},
    "Cadastros": {"/api", "/.well-known", "/health"},
}
ORDEM = {"OS": 10, "Cobranca": 20, "Execucao": 30, "Cadastros": 100}
CONEXAO = {"OS": "ConnectionStrings__OrdemServico", "Cobranca": "ConnectionStrings__Billing",
           "Execucao": "ConnectionStrings__Execucao", "Cadastros": "ConnectionStrings__DbConnection"}
# Objetos que a AmazonEKSEditPolicy com escopo de namespace permite criar.
TIPOS_PERMITIDOS = {"ConfigMap", "Deployment", "HorizontalPodAutoscaler", "Ingress", "Job", "NetworkPolicy",
                    "Secret", "Service", "ServiceAccount"}
URL_FILA = re.compile(r"https://sqs\.([a-z0-9-]+)\.amazonaws\.com/(\d+)/([A-Za-z0-9_-]+)")


def servico_de(repo):
    return repo.lower()


def diretorio_terraform(repo):
    return os.path.join(caminho_repositorio(repo), "infra", "terraform")


def diretorio_chart(repo):
    return os.path.join(caminho_repositorio(repo), "deploy", "helm", servico_de(repo))


def role_irsa_exemplo(repo):
    return f"arn:aws:iam::{CONTA}:role/autoreparos-{servico_de(repo)}-irsa"


def valores_exemplo(repo):
    """Valores que o job deploy informa ao chart, com conteúdo de exemplo."""
    servico = servico_de(repo)
    valores = {
        "image.repository": f"{CONTA}.dkr.ecr.{REGIAO}.amazonaws.com/autoreparos-{servico}",
        "image.tag": "0123456789abcdef0123456789abcdef01234567",
        "aws.conta": CONTA,
        "serviceAccount.roleArn": role_irsa_exemplo(repo),
    }
    if repo == "Cadastros":
        valores["config.jwtIssuer"] = "https://exemplo.execute-api.us-east-1.amazonaws.com"
    else:
        valores["autenticacao.emissor"] = "https://exemplo.execute-api.us-east-1.amazonaws.com"
    return valores


def carregar_yaml():
    try:
        import yaml  # noqa: PLC0415
    except ImportError:
        return None
    return yaml


def renderizar_chart(repo):
    """Roda helm template e devolve (erros, objetos, texto)."""
    chart = diretorio_chart(repo)
    if not os.path.isdir(chart):
        return [f"{repo}: chart ausente em {os.path.relpath(chart, RAIZ)}"], [], ""
    if shutil.which("helm") is None:
        return [f"{repo}: helm não encontrado no PATH"], [], ""
    yaml = carregar_yaml()
    if yaml is None:
        return ["PyYAML não instalado (pip install pyyaml)"], [], ""
    servico = servico_de(repo)
    comando = ["helm", "template", servico, chart, "--namespace", servico]
    for chave, valor in valores_exemplo(repo).items():
        comando += ["--set-string", f"{chave}={valor}"]
    if repo == "Cadastros":
        comando += ["--set", "networkPolicy.allowedCidrs={10.0.1.0/24,10.0.2.0/24}"]
    codigo, saida = executar(comando)
    if codigo != 0:
        return [f"{repo}: helm template falhou\n{ultimas_linhas(saida)}"], [], ""
    objetos = [objeto for objeto in yaml.safe_load_all(saida) if isinstance(objeto, dict)]
    return [], objetos, saida


def do_tipo(objetos, tipo):
    return [objeto for objeto in objetos if objeto.get("kind") == tipo]


def nome(objeto):
    return (objeto.get("metadata") or {}).get("name")


def anotacoes(objeto):
    return (objeto.get("metadata") or {}).get("annotations") or {}


def containers(objeto):
    return ((objeto.get("spec") or {}).get("template") or {}).get("spec", {}).get("containers") or []


def lista(valor):
    return {item.strip() for item in str(valor or "").split(",") if item.strip()}


def variaveis(container, objetos):
    """Mapa nome -> valor do container, resolvendo configMapKeyRef; secretKeyRef vira ('secret', nome, chave)."""
    mapas = {nome(c): (c.get("data") or {}) for c in do_tipo(objetos, "ConfigMap")}
    resultado = {}
    for item in container.get("env") or []:
        origem = item.get("valueFrom") or {}
        if "value" in item:
            resultado[item["name"]] = str(item["value"])
        elif "configMapKeyRef" in origem:
            ref = origem["configMapKeyRef"]
            resultado[item["name"]] = str(mapas.get(ref.get("name"), {}).get(ref.get("key"), ""))
        elif "secretKeyRef" in origem:
            ref = origem["secretKeyRef"]
            resultado[item["name"]] = ("secret", ref.get("name"), ref.get("key"))
    return resultado


def ler(caminho):
    with open(caminho, encoding="utf-8") as arquivo:
        return arquivo.read()


def textos_terraform(repo):
    diretorio = diretorio_terraform(repo)
    if not os.path.isdir(diretorio):
        return None
    return {arquivo: ler(os.path.join(diretorio, arquivo))
            for arquivo in sorted(os.listdir(diretorio)) if arquivo.endswith(".tf")}


def blocos(texto, cabecalho):
    """Corpos dos blocos HCL que começam com o cabeçalho dado (regex), contando chaves."""
    corpos = []
    for achado in re.finditer(cabecalho, texto):
        inicio = texto.find("{", achado.end() - 1)
        if inicio < 0:
            continue
        profundidade = 0
        for indice in range(inicio, len(texto)):
            if texto[indice] == "{":
                profundidade += 1
            elif texto[indice] == "}":
                profundidade -= 1
                if profundidade == 0:
                    corpos.append(texto[inicio + 1:indice])
                    break
    return corpos


# C10 -----------------------------------------------------------------------------------------------------------

def checar_terraform_servicos():
    erros = []
    if shutil.which("terraform") is None:
        return ["terraform não encontrado no PATH"], ""
    for repo in SERVICOS_IMPLANTADOS:
        textos = textos_terraform(repo)
        if textos is None:
            erros.append(f"{repo}: infra/terraform ausente")
            continue
        backends = [corpo for texto in textos.values() for corpo in blocos(texto, r'backend\s+"s3"\s*\{')]
        esperado = f"servicos/{servico_de(repo)}/production.tfstate"
        if len(backends) != 1:
            erros.append(f"{repo}: esperado um backend \"s3\", encontrados {len(backends)}")
        else:
            chave = re.search(r'^\s*key\s*=\s*"([^"]*)"', backends[0], re.MULTILINE)
            if not chave or chave.group(1) != esperado:
                erros.append(f"{repo}: chave do backend {chave.group(1) if chave else None!r}, esperado {esperado!r}")
        # init sem backend num diretório temporário de plugins, para não depender do state remoto.
        with tempfile.TemporaryDirectory() as dados:
            ambiente = dict(os.environ, TF_DATA_DIR=dados)
            diretorio = diretorio_terraform(repo)
            for comando in (["terraform", "init", "-backend=false", "-input=false", "-no-color"],
                            ["terraform", "validate", "-no-color"]):
                processo = subprocess.run(comando, cwd=diretorio, env=ambiente, capture_output=True, text=True,
                                          encoding="utf-8")
                if processo.returncode != 0:
                    erros.append(f"{repo}: {' '.join(comando[:2])} falhou\n"
                                 f"{ultimas_linhas(processo.stdout + processo.stderr)}")
                    break
    return erros, f"OK terraform-servicos: validate e chave do backend em {', '.join(SERVICOS_IMPLANTADOS)}"


# C14 -----------------------------------------------------------------------------------------------------------

def checar_chart_hooks():
    erros = []
    for repo in SERVICOS_IMPLANTADOS:
        falhas, objetos, texto = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        jobs = do_tipo(objetos, "Job")
        migracoes = []
        for job in jobs:
            notas = anotacoes(job)
            hooks = lista(notas.get("helm.sh/hook"))
            politica = lista(notas.get("helm.sh/hook-delete-policy"))
            env = {chave: valor for c in containers(job) for chave, valor in variaveis(c, objetos).items()}
            if env.get("Database__RunMigrations") == "true":
                migracoes.append(job)
                if hooks != {"pre-install", "pre-upgrade"}:
                    erros.append(f"{repo}: Job {nome(job)} com hook {sorted(hooks)}, esperado pre-install,pre-upgrade")
                if politica != {"before-hook-creation", "hook-succeeded"}:
                    erros.append(f"{repo}: Job {nome(job)} com hook-delete-policy {sorted(politica)}, "
                                 "esperado before-hook-creation,hook-succeeded")
                if (job.get("spec", {}).get("template", {}).get("spec", {}).get("serviceAccountName")
                        == servico_de(repo)):
                    erros.append(f"{repo}: Job {nome(job)} usa o ServiceAccount do chart, que não existe no pre-install")
            else:
                erros.append(f"{repo}: Job {nome(job)} não é a migration (Database__RunMigrations=true)")
        if len(migracoes) != 1:
            erros.append(f"{repo}: esperado 1 Job de migration, encontrados {len(migracoes)}")
        if re.search(r"create\s+database|createdb|createuser|create\s+user", texto, re.IGNORECASE):
            erros.append(f"{repo}: o chart renderiza criação de database ou usuário")
    return erros, "OK chart-hooks: um Job de migration pre-install,pre-upgrade por chart, sem criação de database"


# C15 -----------------------------------------------------------------------------------------------------------

def porta_da_probe(probe, container):
    porta = ((probe or {}).get("httpGet") or {}).get("port")
    if isinstance(porta, str) and not porta.isdigit():
        for item in container.get("ports") or []:
            if item.get("name") == porta:
                return item.get("containerPort")
        return None
    return int(porta) if porta is not None else None


def checar_chart_deployment():
    erros = []
    for repo in SERVICOS_IMPLANTADOS:
        falhas, objetos, _ = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        servico = servico_de(repo)
        estranhos = sorted({o.get("kind") for o in objetos} - TIPOS_PERMITIDOS)
        if estranhos:
            erros.append(f"{repo}: objetos fora da AmazonEKSEditPolicy de namespace: {estranhos}")
        deployments = [d for d in do_tipo(objetos, "Deployment") if nome(d) == servico]
        if len(deployments) != 1:
            erros.append(f"{repo}: Deployment {servico} ausente")
            continue
        deployment = deployments[0]
        spec = deployment.get("spec") or {}
        if spec.get("replicas") != 1:
            erros.append(f"{repo}: Deployment com {spec.get('replicas')} réplicas, esperado 1")
        hpas = [h for h in do_tipo(objetos, "HorizontalPodAutoscaler")
                if (h.get("spec") or {}).get("scaleTargetRef", {}).get("name") == servico]
        if len(hpas) != 1:
            erros.append(f"{repo}: HPA do Deployment {servico} ausente")
        else:
            hpa = hpas[0]["spec"]
            if (hpa.get("minReplicas"), hpa.get("maxReplicas")) != (1, 3):
                erros.append(f"{repo}: HPA {hpa.get('minReplicas')}-{hpa.get('maxReplicas')}, esperado 1-3")
        pod = spec.get("template", {}).get("spec", {})
        if pod.get("serviceAccountName") != servico:
            erros.append(f"{repo}: Deployment usa o ServiceAccount {pod.get('serviceAccountName')!r}, esperado {servico!r}")
        contas = [s for s in do_tipo(objetos, "ServiceAccount") if nome(s) == servico]
        if not contas or anotacoes(contas[0]).get("eks.amazonaws.com/role-arn") != role_irsa_exemplo(repo):
            erros.append(f"{repo}: ServiceAccount {servico} sem a anotação eks.amazonaws.com/role-arn da role IRSA")
        principais = containers(deployment)
        if not principais:
            erros.append(f"{repo}: Deployment sem container")
            continue
        container = principais[0]
        for tipo in ("readinessProbe", "livenessProbe"):
            probe = container.get(tipo)
            caminho = ((probe or {}).get("httpGet") or {}).get("path")
            if caminho != "/health" or porta_da_probe(probe, container) != 8080:
                erros.append(f"{repo}: {tipo} em {caminho!r} porta {porta_da_probe(probe, container)!r}, "
                             "esperado /health na 8080")
        env = variaveis(container, objetos)
        esperados = {"AWS_REGION": "us-east-1", "Database__SkipStartupMigration": "true"}
        if repo == "OS":
            esperados["DynamoDb__CriarTabelas"] = "false"
        for chave, valor in esperados.items():
            if env.get(chave) != valor:
                erros.append(f"{repo}: {chave}={env.get(chave)!r}, esperado {valor!r}")
    return erros, "OK chart-deployment: 1 réplica, HPA 1-3, probes em /health:8080, IRSA e variáveis nos 4 charts"


# C16 -----------------------------------------------------------------------------------------------------------

def checar_chart_ingress():
    erros = []
    ordens = {}
    for repo in SERVICOS_IMPLANTADOS:
        falhas, objetos, _ = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        servico = servico_de(repo)
        publicos = [i for i in do_tipo(objetos, "Ingress") if anotacoes(i).get("alb.ingress.kubernetes.io/group.name") == GRUPO_ALB]
        if len(publicos) != 1:
            erros.append(f"{repo}: esperado 1 Ingress no grupo {GRUPO_ALB}, encontrados {len(publicos)}")
            continue
        ingress = publicos[0]
        notas = anotacoes(ingress)
        if notas.get("alb.ingress.kubernetes.io/scheme") != "internal":
            erros.append(f"{repo}: Ingress fora do esquema internal")
        caminhos = []
        for regra in (ingress.get("spec") or {}).get("rules") or []:
            for caminho in (regra.get("http") or {}).get("paths") or []:
                caminhos.append(caminho.get("path"))
                if caminho.get("pathType") != "Prefix":
                    erros.append(f"{repo}: {caminho.get('path')} com pathType {caminho.get('pathType')}, esperado Prefix")
                backend = (caminho.get("backend") or {}).get("service") or {}
                if backend.get("name") != servico or (backend.get("port") or {}).get("number") != 8080:
                    erros.append(f"{repo}: {caminho.get('path')} não aponta para o Service {servico}:8080")
        if set(caminhos) != PREFIXOS[repo] or len(caminhos) != len(set(caminhos)):
            erros.append(f"{repo}: prefixos {sorted(caminhos)}, esperado {sorted(PREFIXOS[repo])}")
        try:
            ordens[repo] = int(notas.get("alb.ingress.kubernetes.io/group.order"))
        except (TypeError, ValueError):
            erros.append(f"{repo}: group.order ausente ou inválido ({notas.get('alb.ingress.kubernetes.io/group.order')!r})")
            continue
        if ordens[repo] != ORDEM[repo]:
            erros.append(f"{repo}: group.order {ordens[repo]}, esperado {ORDEM[repo]} (contrato)")
        internos = [c.get("path") for i in publicos for r in (i.get("spec") or {}).get("rules") or []
                    for c in (r.get("http") or {}).get("paths") or [] if str(c.get("path")).startswith("/internal")]
        if internos:
            erros.append(f"{repo}: /internal exposto no grupo {GRUPO_ALB}")
    if "Cadastros" in ordens:
        outros = [ordens[r] for r in ("OS", "Cobranca", "Execucao") if r in ordens]
        if outros and ordens["Cadastros"] <= max(outros):
            erros.append(f"Cadastros: group.order {ordens['Cadastros']} não é maior que os demais ({outros})")
    return erros, "OK chart-ingress: prefixos do Surface no grupo autoreparos-internal e ordem do Cadastros por último"


# C17 -----------------------------------------------------------------------------------------------------------

def checar_sem_admin():
    erros = []
    for repo in SERVICOS_IMPLANTADOS:
        falhas, objetos, texto = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        servico = servico_de(repo)
        if CREDENCIAL_ADMIN in texto or "database/credentials" in texto:
            erros.append(f"{repo}: o chart renderizado referencia a credencial admin do RDS")
        cargas = do_tipo(objetos, "Deployment") + do_tipo(objetos, "Job")
        for carga in cargas:
            for container in containers(carga):
                env = variaveis(container, objetos)
                conexoes = {chave: valor for chave, valor in env.items() if chave.startswith("ConnectionStrings__")}
                esperado = ("secret", f"{servico}-database", CONEXAO[repo])
                if conexoes != {CONEXAO[repo]: esperado}:
                    erros.append(f"{repo}: {carga.get('kind')} {nome(carga)} lê a connection string de {conexoes}, "
                                 f"esperado o Secret {servico}-database chave {CONEXAO[repo]}")
        workflow = os.path.join(caminho_repositorio(repo), ".github", "workflows", "ci-cd.yml")
        if os.path.isfile(workflow) and CREDENCIAL_ADMIN in ler(workflow):
            erros.append(f"{repo}: o ci-cd.yml referencia a credencial admin do RDS")
    modelo = os.path.join(RAIZ, MODELO_DEPLOY)
    if not os.path.isfile(modelo):
        erros.append(f"{MODELO_DEPLOY} ausente: a política da role de deploy não pôde ser conferida")
    elif CREDENCIAL_ADMIN in ler(modelo) or "database/credentials" in ler(modelo):
        erros.append(f"{MODELO_DEPLOY} referencia a credencial admin do RDS")
    return erros, "OK sem-admin: charts e política de deploy sem a credencial admin; connection string do Secret do serviço"


# C21 -----------------------------------------------------------------------------------------------------------

def checar_sem_staging():
    caminho = os.path.join(RAIZ, "scripts", "specs", "assert_pipelines.py")
    texto = ler(caminho)
    erros = []
    if "deploy-staging" in texto:
        erros.append("assert_pipelines.py ainda tem a regra deploy-staging")
    if re.search(r"staging", texto, re.IGNORECASE):
        erros.append("assert_pipelines.py ainda cobra algo de staging")
    codigo, saida = executar(["python3", caminho, "--check", "deploy-staging"])
    if codigo != 2:
        erros.append(f"assert_pipelines.py --check deploy-staging não foi recusado pelo argparse (código {codigo})")
    return erros, "OK sem-staging: assert_pipelines.py sem a regra deploy-staging"


# C35 -----------------------------------------------------------------------------------------------------------

def filas_declaradas(repo):
    textos = textos_terraform(repo) or {}
    nomes = set()
    for texto in textos.values():
        for corpo in blocos(texto, r'resource\s+"aws_sqs_queue"\s+"[A-Za-z0-9_-]+"\s*\{'):
            achado = re.search(r'^\s*name\s*=\s*"([^"$]+)"\s*$', corpo, re.MULTILINE)
            if achado:
                nomes.add(achado.group(1))
    return nomes


# Tabela "Serviços" de contratos.md: cada chave de produtor -> (consumidor, chave que o consumidor lê no chart).
PRODUCAO = {
    ("Cobranca", "Sqs__QueueUrl"): ("OS", "Sqs__SagaQueueUrl"),
    ("Execucao", "Sqs__OsServiceQueueUrl"): ("OS", "Sqs__SagaQueueUrl"),
    ("OS", "Sqs__BillingQueueUrl"): ("Cobranca", "Sqs__ComandosQueueUrl"),
    ("OS", "Sqs__ExecucaoQueueUrl"): ("Execucao", "Sqs__LiberadasQueueUrl"),
    ("OS", "Sqs__CadastrosQueueUrl"): ("Cadastros", "Notificacoes__FilaUrl"),
}
# D-59: execucao.concluida.v1 vai para uma fila da Cobrança que ninguém consome ainda.
SEM_CONSUMIDOR = {("Execucao", "Sqs__BillingQueueUrl"): "autoreparos-cobranca-execucoes"}
# DLQs de saída (outbox esgotado): o produtor publica na própria fila.
SAIDA_PROPRIA = {("Cobranca", "Sqs__DeadLetterQueueUrl"), ("Execucao", "Sqs__DeadLetterQueueUrl")}
# Chaves que o próprio serviço lê (fila de entrada e DLQ dela).
CONSUMO = {
    "OS": {"Sqs__SagaQueueUrl", "Sqs__SagaDeadLetterQueueUrl"},
    "Cobranca": {"Sqs__ComandosQueueUrl", "Sqs__ComandosDeadLetterQueueUrl"},
    "Execucao": {"Sqs__LiberadasQueueUrl", "Sqs__LiberadasDeadLetterQueueUrl"},
    "Cadastros": {"Notificacoes__FilaUrl", "Notificacoes__DlqUrl"},
}


def politica_irsa(repo):
    """Política IRSA do serviço renderizada como o templatefile() do Terraform dele."""
    from simular_iam import renderizar  # noqa: PLC0415
    caminho = os.path.relpath(os.path.join(diretorio_terraform(repo), "politicas", "irsa.json.tftpl"), RAIZ)
    return json.loads(renderizar(caminho, {"conta": CONTA, "regiao": REGIAO}))


def permite(politica, acao, recurso):
    """Avaliação estática: algum Allow sem condição casa ação e recurso e nenhum Deny casa."""
    def casa(declaracao):
        acoes = declaracao.get("Action", [])
        recursos = declaracao.get("Resource", [])
        acoes = [acoes] if isinstance(acoes, str) else acoes
        recursos = [recursos] if isinstance(recursos, str) else recursos
        return (any(fnmatch.fnmatchcase(acao.lower(), a.lower()) for a in acoes)
                and any(fnmatch.fnmatchcase(recurso, r) for r in recursos))

    declaracoes = politica.get("Statement", [])
    declaracoes = [declaracoes] if isinstance(declaracoes, dict) else declaracoes
    if any(d.get("Effect") == "Deny" and casa(d) for d in declaracoes):
        return False
    return any(d.get("Effect") == "Allow" and "Condition" not in d and casa(d) for d in declaracoes)


def urls_de_fila(objetos):
    urls = {}
    for carga in do_tipo(objetos, "Deployment") + do_tipo(objetos, "Job"):
        for container in containers(carga):
            for chave, valor in variaveis(container, objetos).items():
                if isinstance(valor, str) and "sqs" in valor:
                    urls.setdefault(chave, set()).add(valor)
    return urls


def checar_ligacoes(urls, erros):
    """Cada chave de produtor aponta para a mesma fila que o consumidor lê, e a IRSA do produtor pode publicar nela."""
    politicas = {}
    for repo, chaves in urls.items():
        servico = servico_de(repo)
        for chave, valores in sorted(chaves.items()):
            if len(valores) != 1:
                erros.append(f"{repo}: {chave} tem valores diferentes entre as cargas: {sorted(valores)}")
                continue
            fila = next(iter(valores)).rsplit("/", 1)[-1]
            publica = True
            if (repo, chave) in PRODUCAO:
                consumidor, chave_consumidor = PRODUCAO[(repo, chave)]
                lida = urls.get(consumidor, {}).get(chave_consumidor)
                if lida != valores:
                    erros.append(f"{repo}: {chave} aponta para {fila}, mas {consumidor} lê "
                                 f"{chave_consumidor}={sorted(lida or [])}")
            elif (repo, chave) in SEM_CONSUMIDOR:
                if fila != SEM_CONSUMIDOR[(repo, chave)]:
                    erros.append(f"{repo}: {chave} aponta para {fila}, esperado {SEM_CONSUMIDOR[(repo, chave)]} (D-59)")
            elif (repo, chave) in SAIDA_PROPRIA or chave in CONSUMO.get(repo, set()):
                publica = (repo, chave) in SAIDA_PROPRIA
                if not fila.startswith(f"autoreparos-{servico}-"):
                    erros.append(f"{repo}: {chave} aponta para {fila}, que não é fila de {repo}")
            else:
                erros.append(f"{repo}: {chave} não está na tabela Serviços de contratos.md (produtor ou consumidor)")
                continue
            if publica:
                if repo not in politicas:
                    try:
                        politicas[repo] = politica_irsa(repo)
                    except (OSError, ValueError) as erro:
                        politicas[repo] = None
                        erros.append(f"{repo}: política IRSA ilegível: {erro}")
                arn = f"arn:aws:sqs:{REGIAO}:{CONTA}:{fila}"
                if politicas[repo] is not None and not permite(politicas[repo], "sqs:SendMessage", arn):
                    erros.append(f"{repo}: {chave} aponta para {fila}, mas a política IRSA de {repo} não permite "
                                 f"sqs:SendMessage nela")
    for (repo, chave), (consumidor, chave_consumidor) in sorted(PRODUCAO.items()):
        if repo in urls and chave not in urls[repo]:
            erros.append(f"{repo}: o chart não injeta {chave} (produtor para {consumidor})")
        if consumidor in urls and chave_consumidor not in urls[consumidor]:
            erros.append(f"{consumidor}: o chart não injeta {chave_consumidor}, que {repo} alimenta")


def checar_filas_produtor_consumidor():
    erros = []
    por_servico = {servico_de(repo): repo for repo in SERVICOS_IMPLANTADOS}
    declaradas = {repo: filas_declaradas(repo) for repo in SERVICOS_IMPLANTADOS}
    total = 0
    por_repo = {}
    for repo in SERVICOS_IMPLANTADOS:
        falhas, objetos, _ = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        por_repo[repo] = urls_de_fila(objetos)
        urls = {(chave, valor) for chave, valores in por_repo[repo].items() for valor in valores}
        if not urls:
            erros.append(f"{repo}: o chart não injeta nenhuma URL de fila")
        for chave, valor in sorted(urls):
            achado = URL_FILA.fullmatch(valor)
            if not achado:
                erros.append(f"{repo}: {chave}={valor!r} não é uma URL de fila SQS")
                continue
            regiao, conta, fila = achado.groups()
            total += 1
            if (regiao, conta) != (REGIAO, CONTA):
                erros.append(f"{repo}: {chave} aponta para {regiao}/{conta}, esperado {REGIAO}/{CONTA}")
            partes = fila.split("-")
            consumidor = por_servico.get(partes[1]) if len(partes) > 2 and partes[0] == "autoreparos" else None
            if consumidor is None:
                erros.append(f"{repo}: {chave} aponta para {fila}, fora do padrão autoreparos-<consumidor>-<finalidade>")
            elif fila not in declaradas[consumidor]:
                erros.append(f"{repo}: {chave} aponta para {fila}, que o Terraform de {consumidor} não declara "
                             f"(declaradas: {sorted(declaradas[consumidor])})")
    checar_ligacoes(por_repo, erros)
    return erros, (f"OK filas-produtor-consumidor: {total} URLs de fila apontam para filas do Terraform do consumidor, "
                   f"cada produtor para a fila que o consumidor lê, com sqs:SendMessage na política IRSA")


AUDIENCIA = "autoreparos"


def checar_jwt_audiencia():
    """C36 (D-57): o Cadastros emite aud = autoreparos e OS, Cobrança e Execução exigem o mesmo valor."""
    erros = []
    falhas, objetos, _ = renderizar_chart("Cadastros")
    if falhas:
        return falhas, ""
    emitidas = {str((c.get("data") or {}).get("Jwt__Audience")) for c in do_tipo(objetos, "ConfigMap")
                if "Jwt__Audience" in (c.get("data") or {})}
    if emitidas != {AUDIENCIA}:
        erros.append(f"Cadastros: Jwt__Audience renderizado {sorted(emitidas)}, esperado {AUDIENCIA!r}")
    for repo in ("OS", "Cobranca", "Execucao"):
        falhas, objetos, _ = renderizar_chart(repo)
        if falhas:
            erros += falhas
            continue
        servico = servico_de(repo)
        deployments = [d for d in do_tipo(objetos, "Deployment") if nome(d) == servico]
        valores = {variaveis(c, objetos).get("Authentication__Audience") for d in deployments for c in containers(d)}
        if valores != {AUDIENCIA}:
            erros.append(f"{repo}: Authentication__Audience renderizado {sorted(map(str, valores))}, esperado {AUDIENCIA!r}")
    return erros, f"OK jwt-audiencia: Cadastros emite aud={AUDIENCIA} e OS, Cobranca e Execucao exigem o mesmo valor"


CHECKS = {
    "jwt-audiencia": checar_jwt_audiencia,
    "terraform-servicos": checar_terraform_servicos,
    "chart-hooks": checar_chart_hooks,
    "chart-deployment": checar_chart_deployment,
    "chart-ingress": checar_chart_ingress,
    "sem-admin": checar_sem_admin,
    "sem-staging": checar_sem_staging,
    "filas-produtor-consumidor": checar_filas_produtor_consumidor,
}
