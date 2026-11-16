"""Checks de deploy-servicos da plataforma (S5): nós, SSM, workflows do pai e Ingress do Web.

Cada check é uma função sem argumentos que devolve (erros, mensagem de sucesso) e entra em CHECKS.
"""
import glob
import os
import re
import shutil

import yaml

from _deploy_comum import SERVICOS_IMPLANTADOS, executar, ultimas_linhas
from _repositorios import RAIZ, caminho_repositorio

INFRA_K8S = os.path.join(RAIZ, "submodules", "AutoReparos.Infra.K8s")
STACK_BASE = os.path.join(INFRA_K8S, "terraform", "stacks", "base")
CHART_GUARDA_CHUVA = os.path.join(INFRA_K8S, "k8s")
WORKFLOWS = os.path.join(RAIZ, ".github", "workflows")
WORKFLOWS_DO_PAI = ["provision-aws.yml", "deploy-app.yml"]

# Seção "Parâmetros SSM" de contratos.md: o que os stacks aplicados pelo provision-aws.yml publicam.
PARAMETROS_SSM = [
    "vpc_id", "subnets/private", "security-groups/eks-nodes", "apigateway/endpoint",
    "database/endpoint", "database/address", "database/name", "database/secret_arn",
    "eks/cluster_name", "eks/oidc_provider_arn", "eks/oidc_provider_url", "provisionador-db/url",
]
PREFIXO_SSM = "/autoreparos/production/"

GRUPO_ALB = "autoreparos-internal"
ANOTACAO_GRUPO = "alb.ingress.kubernetes.io/group.name"
ANOTACAO_ORDEM = "alb.ingress.kubernetes.io/group.order"


def ler(caminho):
    with open(caminho, encoding="utf-8") as arquivo:
        return arquivo.read()


def relativo(caminho):
    return os.path.relpath(caminho, RAIZ)


# ---------------------------------------------------------------- Terraform (leitura textual simples)

def blocos(texto, cabecalho):
    """Devolve o corpo de cada bloco HCL cujo cabeçalho casa com a regex `cabecalho` (até a chave que fecha)."""
    corpos = []
    for achado in re.finditer(cabecalho + r"\s*\{", texto):
        nivel, inicio = 1, achado.end()
        for indice in range(inicio, len(texto)):
            if texto[indice] == "{":
                nivel += 1
            elif texto[indice] == "}":
                nivel -= 1
                if nivel == 0:
                    corpos.append(texto[inicio:indice])
                    break
    return corpos


def padrao_variavel(texto, nome):
    corpos = blocos(texto, rf'variable\s+"{re.escape(nome)}"')
    if not corpos:
        return None
    achado = re.search(r"^\s*default\s*=\s*(.+?)\s*$", corpos[0], re.MULTILINE)
    return achado.group(1) if achado else None


def checar_plataforma_nos():
    erros = []
    variaveis = ler(os.path.join(STACK_BASE, "variables.tf"))
    esperados = {"instance_types": '["t3.medium"]', "desired_size": "2", "min_size": "2", "max_size": "3"}
    for nome, valor in esperados.items():
        atual = padrao_variavel(variaveis, nome)
        if atual is None or re.sub(r"\s", "", atual) != valor:
            erros.append(f"stacks/base: variável {nome} com padrão {atual!r}, esperado {valor}")
        principal = ler(os.path.join(STACK_BASE, "main.tf"))
        if not re.search(rf"^\s*{nome}\s*=\s*var\.{nome}\s*$", principal, re.MULTILINE):
            erros.append(f"stacks/base/main.tf: o módulo eks não recebe {nome} = var.{nome}")
    workflow = ler(os.path.join(WORKFLOWS, "provision-aws.yml"))
    for nome in esperados:
        if re.search(rf"(-var[= ]['\"]?{nome}=|TF_VAR_{nome}\b)", workflow):
            erros.append(f"provision-aws.yml sobrescreve {nome} do stacks/base")
    return erros, "OK plataforma-nos: stacks/base com t3.medium, desired/min 2 e max 3, sem sobrescrita no provision-aws.yml"


def passos(texto):
    """Divide um workflow em passos (texto de cada `- ` dentro de `steps:`)."""
    return re.split(r"^\s{6}- ", texto, flags=re.MULTILINE)[1:]


def diretorios_aplicados():
    """Diretórios em que o provision-aws.yml roda `terraform apply`."""
    diretorios = []
    for passo in passos(ler(os.path.join(WORKFLOWS, "provision-aws.yml"))):
        achado = re.search(r"^\s*working-directory:\s*(\S+)\s*$", passo, re.MULTILINE)
        if achado and "terraform apply" in passo:
            diretorios.append(os.path.join(RAIZ, achado.group(1)))
    return diretorios


def arquivos_terraform(diretorio, vistos=None):
    """Arquivos .tf do diretório e dos módulos locais que ele usa, recursivamente."""
    vistos = set() if vistos is None else vistos
    diretorio = os.path.normpath(diretorio)
    if diretorio in vistos:
        return []
    vistos.add(diretorio)
    arquivos = sorted(glob.glob(os.path.join(diretorio, "*.tf")))
    for arquivo in list(arquivos):
        for origem in re.findall(r'^\s*source\s*=\s*"(\.\.?/[^"]+)"', ler(arquivo), re.MULTILINE):
            arquivos += arquivos_terraform(os.path.join(diretorio, origem), vistos)
    return arquivos


def parametros_ssm(arquivos):
    """Nomes dos aws_ssm_parameter declarados, com var.environment = production e each.key resolvido."""
    texto = "\n".join(ler(arquivo) for arquivo in arquivos)
    locais = {}
    for corpo in blocos(texto, r"\blocals"):
        for mapa in re.finditer(r"^\s*(\w+)\s*=\s*\{", corpo, re.MULTILINE):
            interno = blocos(corpo[mapa.start():], rf"{mapa.group(1)}\s*=")[0]
            locais[mapa.group(1)] = re.findall(r'^\s*"([^"]+)"\s*=', interno, re.MULTILINE)
    nomes = []
    for corpo in blocos(texto, r'resource\s+"aws_ssm_parameter"\s+"\w+"'):
        achado = re.search(r'^\s*name\s*=\s*"([^"]+)"', corpo, re.MULTILINE)
        if not achado:
            continue
        nome = achado.group(1).replace("${var.environment}", "production")
        if "${each.key}" in nome:
            origem = re.search(r"^\s*for_each\s*=\s*(.+?)\s*$", corpo, re.MULTILINE)
            chaves = []
            if origem and origem.group(1).startswith("local."):
                chaves = locais.get(origem.group(1)[len("local."):], [])
            elif origem:
                chaves = re.findall(r'"([^"]+)"', origem.group(1))
            nomes += [nome.replace("${each.key}", chave) for chave in chaves]
        else:
            nomes.append(nome)
    return nomes


def checar_ssm_plataforma():
    erros = []
    diretorios = diretorios_aplicados()
    if not diretorios:
        return ["provision-aws.yml não aplica nenhum diretório Terraform"], ""
    publicados = set()
    for diretorio in diretorios:
        publicados.update(parametros_ssm(arquivos_terraform(diretorio)))
    for nome in PARAMETROS_SSM:
        if PREFIXO_SSM + nome not in publicados:
            erros.append(f"nenhum stack aplicado pelo provision-aws.yml publica {PREFIXO_SSM}{nome}")
    base = parametros_ssm(arquivos_terraform(STACK_BASE))
    texto_base = "\n".join(ler(arquivo) for arquivo in arquivos_terraform(STACK_BASE))
    if PREFIXO_SSM + "eks/oidc_provider_url" in base and 'trimprefix(module.eks.oidc_provider_url, "https://")' not in texto_base:
        erros.append("stacks/base: eks/oidc_provider_url precisa ser publicado sem https://")
    for servico in SERVICOS_IMPLANTADOS:
        terraform = os.path.join(caminho_repositorio(servico), "infra", "terraform")
        for arquivo in glob.glob(os.path.join(terraform, "**", "*.tf"), recursive=True):
            if "/.terraform/" in arquivo:
                continue
            if re.search(r'data\s+"terraform_remote_state"', ler(arquivo)):
                erros.append(f"{relativo(arquivo)} usa terraform_remote_state (D-53: só aws_ssm_parameter)")
    aplicados = ", ".join(relativo(d) for d in diretorios)
    return erros, f"OK ssm-plataforma: {len(PARAMETROS_SSM)} parâmetros publicados por {aplicados}; nenhum serviço usa terraform_remote_state"


# ---------------------------------------------------------------- Helm

def renderizar(nome, chart, *arquivos_de_valores):
    if shutil.which("helm") is None:
        return None, "helm não encontrado no PATH"
    comando = ["helm", "template", nome, chart]
    for valores in arquivos_de_valores:
        comando += ["-f", valores]
    codigo, saida = executar(comando)
    if codigo != 0:
        return None, f"helm template {relativo(chart)} falhou\n{ultimas_linhas(saida)}"
    return [doc for doc in yaml.safe_load_all(saida) if isinstance(doc, dict)], None


def renderizar_guarda_chuva():
    return renderizar("autoreparos", CHART_GUARDA_CHUVA,
                      os.path.join(CHART_GUARDA_CHUVA, "values.yaml"),
                      os.path.join(CHART_GUARDA_CHUVA, "values-production.yaml"))


def servicos_do_ingress(ingress):
    nomes = set()
    for regra in (ingress.get("spec") or {}).get("rules") or []:
        for caminho in ((regra.get("http") or {}).get("paths")) or []:
            nomes.add((((caminho.get("backend") or {}).get("service")) or {}).get("name"))
    return nomes


def caminhos_do_ingress(ingress):
    return [caminho.get("path") for regra in (ingress.get("spec") or {}).get("rules") or []
            for caminho in ((regra.get("http") or {}).get("paths")) or []]


def checar_pai_so_web():
    erros = []
    proibidos = ["autoreparos-api", "ECR_API", "AutoReparos.Cadastros/", "api.image", "api.config",
                 "api.networkPolicy", "Database__RunMigrations", "JWT_CHAVE_PRIVADA_PEM", "SEED_USER_PASSWORD"]
    for nome in WORKFLOWS_DO_PAI:
        texto = ler(os.path.join(WORKFLOWS, nome))
        builds = [linha.strip() for linha in texto.splitlines() if re.search(r"\bdocker build\b", linha)]
        pushes = [linha.strip() for linha in texto.splitlines() if re.search(r"\bdocker push\b", linha)]
        if not builds:
            erros.append(f"{nome}: nenhum docker build (o Web precisa continuar publicado)")
        for linha in builds:
            if "submodules/AutoReparos.Web" not in linha or re.search(r"AutoReparos\.(?!Web\b)\w+", linha):
                erros.append(f"{nome}: docker build de algo além do Web: {linha}")
        for linha in pushes:
            if "$ECR_WEB:" not in linha:
                erros.append(f"{nome}: docker push de algo além do Web: {linha}")
        for termo in proibidos:
            if termo in texto:
                erros.append(f"{nome}: ainda cita {termo!r} (API do Cadastros saiu do pai)")
        for helm in re.findall(r"helm upgrade --install autoreparos[^\n]*(?:\\\n[^\n]*)*", texto):
            if re.search(r"--set(-string)?\s+['\"]?api\.", helm):
                erros.append(f"{nome}: o helm upgrade do guarda-chuva ainda configura o subchart api")

    chart = yaml.safe_load(ler(os.path.join(CHART_GUARDA_CHUVA, "Chart.yaml")))
    if any(dep.get("name") == "api" for dep in chart.get("dependencies") or []):
        erros.append("k8s/Chart.yaml ainda depende do subchart api")
    sobras = [p for p in glob.glob(os.path.join(CHART_GUARDA_CHUVA, "charts", "api*")) if os.path.exists(p)]
    if sobras:
        erros.append(f"k8s/charts ainda tem o subchart api: {[relativo(p) for p in sobras]}")
    documentos, erro = renderizar_guarda_chuva()
    if erro:
        erros.append(erro)
    else:
        for doc in documentos:
            nome = (doc.get("metadata") or {}).get("name") or ""
            if "autoreparos-api" in nome or "cadastros" in nome:
                erros.append(f"o guarda-chuva ainda renderiza {doc.get('kind')} {nome}")
            if doc.get("kind") == "Ingress":
                if servicos_do_ingress(doc) - {"autoreparos-web"}:
                    erros.append(f"Ingress {nome} do guarda-chuva aponta para {sorted(servicos_do_ingress(doc))}")
    return erros, "OK pai-so-web: provision-aws.yml e deploy-app.yml publicam só o Web; o guarda-chuva não tem o subchart api"


def ordem(ingress):
    valor = ((ingress.get("metadata") or {}).get("annotations") or {}).get(ANOTACAO_ORDEM, "0")
    try:
        return int(str(valor))
    except ValueError:
        return None


def grupo(ingress):
    return ((ingress.get("metadata") or {}).get("annotations") or {}).get(ANOTACAO_GRUPO)


def checar_web_ingress():
    erros = []
    documentos, erro = renderizar_guarda_chuva()
    if erro:
        return [erro], ""
    ingresses = [("guarda-chuva", d) for d in documentos if d.get("kind") == "Ingress"]
    for servico in SERVICOS_IMPLANTADOS:
        for chart in sorted(glob.glob(os.path.join(caminho_repositorio(servico), "deploy", "helm", "*", "Chart.yaml"))):
            diretorio = os.path.dirname(chart)
            docs, erro = renderizar(os.path.basename(diretorio), diretorio)
            if erro:
                erros.append(erro)
                continue
            ingresses += [(servico, d) for d in docs if d.get("kind") == "Ingress"]

    web = [d for origem, d in ingresses if origem == "guarda-chuva" and "autoreparos-web" in servicos_do_ingress(d)]
    if len(web) != 1:
        return erros + [f"esperado 1 Ingress do Web no guarda-chuva, encontrados {len(web)}"], ""
    web = web[0]
    if grupo(web) != GRUPO_ALB:
        erros.append(f"Ingress do Web no grupo {grupo(web)!r}, esperado {GRUPO_ALB}")
    if "/" not in caminhos_do_ingress(web):
        erros.append("Ingress do Web sem o catch-all /")
    ordem_web = ordem(web)
    if ordem_web is None or ANOTACAO_ORDEM not in (web["metadata"].get("annotations") or {}):
        erros.append(f"Ingress do Web sem {ANOTACAO_ORDEM} numérico")
        ordem_web = None
    outros = [(origem, d) for origem, d in ingresses if d is not web and grupo(d) == GRUPO_ALB]
    for origem, doc in outros:
        if ordem_web is not None and (ordem(doc) is None or ordem(doc) >= ordem_web):
            erros.append(f"Ingress {doc['metadata'].get('name')} ({origem}) tem group.order {ordem(doc)}, "
                         f"que não fica abaixo do Web ({ordem_web})")
    comparados = ", ".join(f"{origem}:{d['metadata'].get('name')}={ordem(d)}" for origem, d in outros) or "nenhum outro"
    return erros, f"OK web-ingress: Web em {GRUPO_ALB} com group.order {ordem_web}, acima de {comparados}"


def checar_destroy_servicos():
    """O destroy-aws.yml destrói o state de cada serviço (servicos/*) com o Terraform dos serviços, antes da Lambda e da plataforma."""
    caminho = os.path.join(WORKFLOWS, "destroy-aws.yml")
    with open(caminho, encoding="utf-8") as arquivo:
        passos = yaml.safe_load(arquivo)["jobs"]["teardown"]["steps"]
    nomes = [passo.get("name", "") for passo in passos]
    erros = []
    indices = [i for i, nome in enumerate(nomes) if "service stacks" in nome]
    if not indices:
        return ["destroy-aws.yml sem o passo que remove os service stacks (servicos/*)"], ""
    posicao = indices[0]
    corpo = passos[posicao].get("run", "")
    for servico in SERVICOS_IMPLANTADOS:
        if not re.search(rf"{servico.lower()}:{servico}", corpo):
            erros.append(f"o passo dos service stacks não cobre {servico}")
    if "infra/terraform" not in corpo or "terraform -chdir" not in corpo or " destroy " not in corpo:
        erros.append("o passo dos service stacks não executa terraform destroy")
    for alvo in ("Remove Lambda", "Remove platform add-ons"):
        outros = [i for i, nome in enumerate(nomes) if nome == alvo]
        if outros and outros[0] < posicao:
            erros.append(f"os service stacks saem depois do passo {alvo!r}; a IRSA depende da plataforma")
    versoes = [str(passo.get("with", {}).get("terraform_version")) for passo in passos[:posicao]
               if "setup-terraform" in passo.get("uses", "")]
    if not versoes or versoes[-1] != "1.15.8":
        erros.append(f"Terraform antes dos service stacks é {versoes[-1] if versoes else 'ausente'}, esperado 1.15.8 (o dos serviços)")
    return erros, f"OK destroy-servicos: {len(SERVICOS_IMPLANTADOS)} serviços destruídos antes da Lambda e da plataforma, com o Terraform 1.15.8"


CHECKS = {
    "plataforma-nos": checar_plataforma_nos,
    "ssm-plataforma": checar_ssm_plataforma,
    "pai-so-web": checar_pai_so_web,
    "web-ingress": checar_web_ingress,
    "destroy-servicos": checar_destroy_servicos,
}
