#!/usr/bin/env python3
"""Provas estáticas de infra-ci-observabilidade (D-65 a D-67): C8, C14, C15, C19 e C21.

Uso: python3 scripts/specs/assert_observabilidade.py --check <fontes|meter-os|grafana|sns-helm|filtros-sns>
"""
import argparse
import glob
import json
import os
import re
import subprocess
import sys

from _repositorios import RAIZ, caminho_repositorio, reportar

SERVICOS = ["OS", "Cobranca", "Execucao", "Cadastros"]
CHART_OBSERVABILIDADE = os.path.join(RAIZ, "submodules", "AutoReparos.Infra.K8s", "k8s", "charts", "observability")
DATASOURCES = {
    "Prometheus": "http://autoreparos-prometheus:9090",
    "Jaeger": "http://autoreparos-jaeger:16686",
    "Loki": "http://autoreparos-loki:3100",
}
PAINEIS = ["Sagas iniciadas", "Sagas concluídas", "Compensações por passo", "Sagas paradas", "DLQ da Saga"]
TOPICO = "autoreparos-execucao-eventos"


def fontes_cs(servico, sufixo_projeto="Api"):
    """Texto dos .cs do projeto de entrada do serviço, sem bin/obj."""
    padrao = os.path.join(caminho_repositorio(servico), f"AutoReparos.{servico}.{sufixo_projeto}", "**", "*.cs")
    texto = []
    for caminho in glob.glob(padrao, recursive=True):
        if "/obj/" in caminho or "/bin/" in caminho:
            continue
        with open(caminho, encoding="utf-8-sig") as arquivo:
            texto.append(arquivo.read())
    return "\n".join(texto)


def registra(texto, metodo, nome):
    """True se `.AddSource`/`.AddMeter` recebe o nome literal ou uma constante cujo valor é o nome."""
    if re.search(rf'\.{metodo}\(\s*"{re.escape(nome)}"', texto):
        return True
    return bool(re.search(rf"\.{metodo}\(\s*[\w.]+\s*[,)]", texto)) and f'"{nome}"' in texto


def checar_fontes(erros):
    for servico in SERVICOS:
        nome = f"AutoReparos.{servico}.Mensageria"
        texto = fontes_cs(servico) + "\n" + todo_o_codigo(servico)
        if not registra(fontes_cs(servico), "AddSource", nome) and not (
                ".AddSource(" in fontes_cs(servico) and f'"{nome}"' in texto):
            erros.append(f"{servico}: o pipeline de traces não registra o ActivitySource `{nome}`")
        if f'"{nome}"' not in texto:
            erros.append(f"{servico}: nenhum ActivitySource chamado `{nome}`")


def todo_o_codigo(servico):
    texto = []
    for caminho in glob.glob(os.path.join(caminho_repositorio(servico), "**", "*.cs"), recursive=True):
        if "/obj/" in caminho or "/bin/" in caminho or "Tests" in caminho:
            continue
        with open(caminho, encoding="utf-8-sig") as arquivo:
            texto.append(arquivo.read())
    return "\n".join(texto)


def checar_meter_os(erros):
    nome = "AutoReparos.OS.Saga"
    api = fontes_cs("OS")
    codigo = todo_o_codigo("OS")
    # O Meter da Saga se chama exatamente `nome` e é ele (pela constante ou pelo literal) que o pipeline registra.
    if not re.search(rf'const\s+string\s+NomeMeter\s*=\s*"{re.escape(nome)}"', codigo) or "new(NomeMeter)" not in codigo:
        erros.append(f"OS: TelemetriaSaga não cria o Meter `{nome}` pela constante NomeMeter")
    if not re.search(rf'\.AddMeter\(\s*(TelemetriaSaga\.NomeMeter|"{re.escape(nome)}")\s*\)', api):
        erros.append(f"OS: o pipeline de métricas não registra o Meter `{nome}` (AddMeter)")


def renderizar(chart, *args):
    processo = subprocess.run(["helm", "template", "t", chart, *args], capture_output=True, text=True, encoding="utf-8")
    return processo.returncode, processo.stdout, processo.stderr


def checar_grafana(erros):
    status, saida, erro = renderizar(CHART_OBSERVABILIDADE)
    if status != 0:
        erros.append(f"helm template falhou: {erro.strip()[:200]}")
        return
    for nome, url in DATASOURCES.items():
        achado = re.search(rf"- name: {nome}\n(?:.*\n)*?\s+url: (\S+)", saida)
        if not achado or achado.group(1) != url:
            erros.append(f"datasource {nome}: url {achado.group(1) if achado else 'ausente'}, esperado {url}")
    bloco = re.search(r"^  saga\.json: \|\n((?:    .*\n|\n)+)", saida, re.MULTILINE)
    if not bloco:
        erros.append("dashboard saga.json ausente do configmap de dashboards")
        return
    try:
        dashboard = json.loads("\n".join(linha[4:] for linha in bloco.group(1).splitlines()))
    except json.JSONDecodeError as falha:
        erros.append(f"saga.json não é JSON válido: {falha}")
        return
    if dashboard.get("title") != "Saga":
        erros.append(f"dashboard com título {dashboard.get('title')!r}, esperado 'Saga'")
    titulos = [painel.get("title") for painel in dashboard.get("panels", [])]
    for painel in PAINEIS:
        if painel not in titulos:
            erros.append(f"dashboard Saga sem o painel `{painel}`")
    for painel in dashboard.get("panels", []):
        if not any(alvo.get("expr") for alvo in painel.get("targets", [])):
            erros.append(f"painel `{painel.get('title')}` sem consulta")


def checar_sns_helm(erros):
    chart = os.path.join(caminho_repositorio("Execucao"), "deploy", "helm", "execucao")
    status, saida, erro = renderizar(chart, "--set", "aws.conta=123456789012")
    if status != 0:
        erros.append(f"helm template falhou: {erro.strip()[:200]}")
        return
    achado = re.search(r"- name: Sns__EventosTopicArn\n\s+value: \"?([^\"\n]+)\"?", saida)
    esperado = f"arn:aws:sns:us-east-1:123456789012:{TOPICO}"
    if not achado or achado.group(1) != esperado:
        erros.append(f"Execução: Sns__EventosTopicArn = {achado.group(1) if achado else 'ausente'}, esperado {esperado}")


def tipos_no_terraform(servico):
    """Tipos do filter_policy da assinatura do tópico da Execução no Terraform do serviço."""
    pasta = os.path.join(caminho_repositorio(servico), "infra", "terraform")
    texto = ""
    for caminho in glob.glob(os.path.join(pasta, "*.tf")):
        with open(caminho, encoding="utf-8") as arquivo:
            texto += arquivo.read() + "\n"
    recurso = re.search(r'resource "aws_sns_topic_subscription"[^{]*\{(.*?)\n\}', texto, re.DOTALL)
    if not recurso:
        return None, texto
    corpo = recurso.group(1)
    nomes = set(re.findall(r'"([a-z-]+\.[a-z.-]+\.v\d+)"', corpo))
    for local in re.findall(r"local\.(\w+)", corpo):
        definicao = re.search(rf"{local}\s*=\s*\[(.*?)\]", texto, re.DOTALL)
        if definicao:
            nomes |= set(re.findall(r'"([a-z-]+\.[a-z.-]+\.v\d+)"', definicao.group(1)))
    return nomes, corpo


def literais_de_tipo(texto):
    return set(re.findall(r'"([a-z-]+\.[a-z.-]+\.v\d+)"', texto))


def checar_filtros_sns(erros):
    publicados = literais_de_tipo(todo_o_codigo("Execucao"))
    with open(os.path.join(caminho_repositorio("OS"), "AutoReparos.OS.Domain", "TiposMensagem.cs"),
              encoding="utf-8-sig") as arquivo:
        tipos_os = arquivo.read()
    entrada = re.search(r"Entrada\b.*?[\[{](.*?)[\]}]\s*;", tipos_os, re.DOTALL)
    consumidos_os = literais_de_tipo(entrada.group(1)) if entrada else set()
    if not consumidos_os:
        constantes = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]+)"', tipos_os))
        consumidos_os = {constantes[n] for n in re.findall(r"\b(\w+)\b", entrada.group(1) if entrada else "")
                         if n in constantes}
    esperado_os = {t for t in consumidos_os if t.startswith(("execucao.", "insumos."))} & publicados \
        if publicados else set()
    filtro_os, corpo_os = tipos_no_terraform("OS")
    if filtro_os is None:
        erros.append("OS: Terraform sem aws_sns_topic_subscription")
    else:
        if not esperado_os:
            erros.append("OS: não foi possível derivar de TiposMensagem os tipos de entrada produzidos pela Execução")
        if filtro_os != esperado_os:
            erros.append(f"OS: filtro {sorted(filtro_os)} difere do esperado {sorted(esperado_os)}")
        if not re.search(r"raw_message_delivery\s*=\s*true", corpo_os):
            erros.append("OS: assinatura sem raw_message_delivery = true")
    filtro_cad, corpo_cad = tipos_no_terraform("Cadastros")
    if filtro_cad is None:
        erros.append("Cadastros: Terraform sem aws_sns_topic_subscription")
    else:
        if filtro_cad != {"execucao.concluida.v1"}:
            erros.append(f"Cadastros: filtro {sorted(filtro_cad)}, esperado só execucao.concluida.v1")
        aceitos = literais_de_tipo(todo_o_codigo("Cadastros"))
        if "execucao.concluida.v1" not in aceitos:
            erros.append("Cadastros: EventoNotificacao não aceita execucao.concluida.v1")
        if not re.search(r"raw_message_delivery\s*=\s*true", corpo_cad):
            erros.append("Cadastros: assinatura sem raw_message_delivery = true")


CHECKS = {
    "fontes": checar_fontes,
    "meter-os": checar_meter_os,
    "grafana": checar_grafana,
    "sns-helm": checar_sns_helm,
    "filtros-sns": checar_filtros_sns,
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", required=True, choices=sorted(CHECKS))
    args = parser.parse_args()
    erros = []
    CHECKS[args.check](erros)
    return reportar(erros, f"OK {args.check}")


if __name__ == "__main__":
    sys.exit(main())
