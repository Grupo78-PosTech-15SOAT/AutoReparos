#!/usr/bin/env python3
"""Aplica na conta, por CLI, as políticas IAM do `stacks/base` que valem para os 4 serviços, iguais ao Terraform.

Usado quando rodar o `provision-aws.yml` (plataforma inteira, ~35 min) não cabe: boundary v2 e roles de deploy em 16/11
(deploy-servicos) e SNS por prefixo do serviço (D-67, AD-072). As políticas saem dos mesmos modelos `.json.tftpl` que o
Terraform usa, com as variáveis reais da conta; o Terraform continua sendo a fonte e o próximo `apply` não muda nada.

    AWS_PROFILE=autoreparos python3 scripts/infra/aplicar_iam_base.py                # só mostra o que mudaria
    AWS_PROFILE=autoreparos python3 scripts/infra/aplicar_iam_base.py --aplicar      # aplica, com backup antes
    AWS_PROFILE=autoreparos python3 scripts/infra/aplicar_iam_base.py --reverter <pasta do backup>

- Roles de deploy `autoreparos-deploy-<servico>`: política inline de mesmo nome (`put-role-policy`).
- Boundary `autoreparos-servico-boundary`: nova versão padrão (`create-policy-version --set-as-default`); com 5 versões,
  apaga antes a mais antiga que não é a padrão. Reverter volta a versão padrão anterior e reaplica as inline do backup.
- Idempotente: política igual à da conta não é reaplicada. Sai com erro se o perfil não apontar para a conta da Fase 4.
"""
import argparse
import datetime
import json
import os
import pathlib
import subprocess
import sys

RAIZ = pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(RAIZ / "scripts" / "specs"))
from simular_iam import renderizar  # noqa: E402  (mesma renderização do templatefile() usada nas provas)

CONTA = "198903849859"
REGIAO = "us-east-1"
CLUSTER = "AutoReparosEKS"
BUCKET = f"autoreparos-tfstate-{CONTA}"
TABELA_LOCK = "autoreparos-terraform-locks"
SERVICOS = ["os", "cobranca", "execucao", "cadastros"]
MODELOS = "submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/"
BOUNDARY_ARN = f"arn:aws:iam::{CONTA}:policy/autoreparos-servico-boundary"
BACKUPS = RAIZ / "scripts" / "infra" / ".backups-iam"


def aws(*args, saida_json=True):
    comando = ["aws", *args] + (["--output", "json"] if saida_json else [])
    processo = subprocess.run(comando, capture_output=True, text=True, encoding="utf-8")
    if processo.returncode != 0:
        raise SystemExit(f"falhou: aws {' '.join(args[:3])}...\n{processo.stderr.strip()}")
    return json.loads(processo.stdout) if saida_json and processo.stdout.strip() else None


def canonico(documento):
    return json.dumps(documento, sort_keys=True, separators=(",", ":"))


def politica_deploy(servico):
    variaveis = {"servico": servico, "conta": CONTA, "regiao": REGIAO, "bucket": BUCKET,
                 "tabela_lock": TABELA_LOCK, "cluster": CLUSTER}
    return json.loads(renderizar(MODELOS + "deploy-servico.json.tftpl", variaveis))


def politica_boundary():
    return json.loads(renderizar(MODELOS + "servico-boundary.json.tftpl", {"conta": CONTA, "regiao": REGIAO}))


def deploy_atual(servico):
    nome = f"autoreparos-deploy-{servico}"
    return aws("iam", "get-role-policy", "--role-name", nome, "--policy-name", nome)["PolicyDocument"]


def boundary_atual():
    padrao = aws("iam", "get-policy", "--policy-arn", BOUNDARY_ARN)["Policy"]["DefaultVersionId"]
    versao = aws("iam", "get-policy-version", "--policy-arn", BOUNDARY_ARN, "--version-id", padrao)
    return padrao, versao["PolicyVersion"]["Document"]


def sids(documento):
    return [s.get("Sid", "?") for s in documento["Statement"]]


def descrever(rotulo, atual, nova):
    if canonico(atual) == canonico(nova):
        print(f"= {rotulo}: igual à conta")
        return False
    novos = sorted(set(sids(nova)) - set(sids(atual)))
    removidos = sorted(set(sids(atual)) - set(sids(nova)))
    alterados = sorted(s["Sid"] for s in nova["Statement"]
                       if s.get("Sid") in set(sids(atual)) and canonico(s) != canonico(
                           next(a for a in atual["Statement"] if a.get("Sid") == s.get("Sid"))))
    print(f"~ {rotulo}: acrescenta {novos or '-'}; remove {removidos or '-'}; altera {alterados or '-'}")
    return True


def conferir_conta():
    conta = aws("sts", "get-caller-identity")["Account"]
    if conta != CONTA:
        raise SystemExit(f"o perfil aponta para a conta {conta}; esperado {CONTA}")


def aplicar():
    pasta = BACKUPS / datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    pasta.mkdir(parents=True)
    mudancas = []
    for servico in SERVICOS:
        atual, nova = deploy_atual(servico), politica_deploy(servico)
        (pasta / f"deploy-{servico}.json").write_text(json.dumps(atual, indent=2))
        if descrever(f"autoreparos-deploy-{servico}", atual, nova):
            mudancas.append(("deploy", servico, nova))
    padrao, atual = boundary_atual()
    (pasta / "boundary.json").write_text(json.dumps({"versao": padrao, "documento": atual}, indent=2))
    nova = politica_boundary()
    if descrever(f"autoreparos-servico-boundary ({padrao})", atual, nova):
        mudancas.append(("boundary", None, nova))
    print(f"backup em {pasta}")

    for tipo, servico, documento in mudancas:
        if tipo == "deploy":
            nome = f"autoreparos-deploy-{servico}"
            aws("iam", "put-role-policy", "--role-name", nome, "--policy-name", nome,
                "--policy-document", json.dumps(documento), saida_json=False)
            confere = canonico(deploy_atual(servico)) == canonico(documento)
            print(f"{'ok' if confere else 'DIVERGE'} put-role-policy {nome}")
        else:
            versoes = aws("iam", "list-policy-versions", "--policy-arn", BOUNDARY_ARN)["Versions"]
            if len(versoes) >= 5:
                antiga = min((v for v in versoes if not v["IsDefaultVersion"]), key=lambda v: v["CreateDate"])
                aws("iam", "delete-policy-version", "--policy-arn", BOUNDARY_ARN, "--version-id", antiga["VersionId"],
                    saida_json=False)
                print(f"apagada a versão antiga {antiga['VersionId']} do boundary (limite de 5)")
            criada = aws("iam", "create-policy-version", "--policy-arn", BOUNDARY_ARN,
                         "--policy-document", json.dumps(documento), "--set-as-default")["PolicyVersion"]["VersionId"]
            nova_padrao, aplicado = boundary_atual()
            confere = nova_padrao == criada and canonico(aplicado) == canonico(documento)
            print(f"{'ok' if confere else 'DIVERGE'} boundary agora em {criada}")
    if not mudancas:
        print("nada a aplicar")


def reverter(pasta):
    pasta = pathlib.Path(pasta)
    for servico in SERVICOS:
        nome = f"autoreparos-deploy-{servico}"
        documento = json.loads((pasta / f"deploy-{servico}.json").read_text())
        aws("iam", "put-role-policy", "--role-name", nome, "--policy-name", nome,
            "--policy-document", json.dumps(documento), saida_json=False)
        print(f"revertida {nome}")
    versao = json.loads((pasta / "boundary.json").read_text())["versao"]
    aws("iam", "set-default-policy-version", "--policy-arn", BOUNDARY_ARN, "--version-id", versao, saida_json=False)
    print(f"boundary de volta à {versao}")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    grupo = parser.add_mutually_exclusive_group()
    grupo.add_argument("--aplicar", action="store_true", help="aplica as diferenças (com backup antes)")
    grupo.add_argument("--reverter", metavar="PASTA", help="reaplica o backup de uma aplicação anterior")
    args = parser.parse_args()
    if "AWS_PROFILE" not in os.environ:
        raise SystemExit("defina AWS_PROFILE (perfil da conta da Fase 4)")
    conferir_conta()
    if args.reverter:
        reverter(args.reverter)
    elif args.aplicar:
        aplicar()
    else:
        for servico in SERVICOS:
            descrever(f"autoreparos-deploy-{servico}", deploy_atual(servico), politica_deploy(servico))
        padrao, atual = boundary_atual()
        descrever(f"autoreparos-servico-boundary ({padrao})", atual, politica_boundary())
        print("dry-run: nada foi aplicado; use --aplicar")


if __name__ == "__main__":
    main()
