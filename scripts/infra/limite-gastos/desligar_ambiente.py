"""Desliga o ambiente AWS quando o orçamento total da conta passa do limite (D-52).

Disparado pelo SNS `autoreparos-limite-gastos`, que recebe o aviso do AWS Budgets.
Apaga tudo que cobra por hora e congela as roles do projeto para nenhum pipeline recriar:

1. anexa a política `autoreparos-congelado` (nega criar recursos caros) a toda role `autoreparos*`;
2. apaga node groups e Fargate profiles do EKS e, quando não sobra nenhum, o cluster;
3. apaga load balancers (ALB/NLB e clássicos);
4. apaga NAT Gateways e libera Elastic IPs soltos;
5. termina instâncias EC2 que sobraram;
6. apaga instâncias RDS (com snapshot final quando RDS_SNAPSHOT_FINAL=true).

O que não cobra por hora (segredos, ECR, chave KMS, snapshots, tabelas DynamoDB) fica para o
`destroy-aws.yml`. Evento com {"dryRun": true} (direto ou como mensagem SNS) só lista o que faria.
"""
import json
import os
import time
from datetime import datetime, timezone

import boto3

REGIOES = [r.strip() for r in os.environ.get("REGIOES", "us-east-1").split(",") if r.strip()]
POLITICA_CONGELADO = os.environ["POLITICA_CONGELADO_ARN"]
ROLE_PROPRIA = os.environ.get("ROLE_PROPRIA", "autoreparos-limite-gastos-lambda")
RDS_SNAPSHOT_FINAL = os.environ.get("RDS_SNAPSHOT_FINAL", "true").lower() == "true"
MAX_RODADAS = int(os.environ.get("MAX_RODADAS", "8"))
ESPERA_CLUSTER_S = int(os.environ.get("ESPERA_CLUSTER_S", "600"))


def _evento(event):
    """Junta o evento direto e o corpo da mensagem SNS; o texto do Budgets não é JSON."""
    dados = dict(event or {})
    for registro in (event or {}).get("Records", []):
        mensagem = registro.get("Sns", {}).get("Message", "")
        try:
            corpo = json.loads(mensagem)
            if isinstance(corpo, dict):
                dados.update(corpo)
        except ValueError:
            pass
    return dados


def congelar_roles(iam, simular, log):
    paginador = iam.get_paginator("list_roles")
    for pagina in paginador.paginate():
        for role in pagina["Roles"]:
            nome = role["RoleName"]
            if not nome.lower().startswith("autoreparos") or nome == ROLE_PROPRIA:
                continue
            log.append(f"congelar role {nome}")
            if not simular:
                iam.attach_role_policy(RoleName=nome, PolicyArn=POLITICA_CONGELADO)


def desligar_eks(eks, simular, log):
    """Apaga node groups e Fargate; devolve os clusters que ainda esperam node groups sumirem."""
    pendentes = []
    for cluster in eks.list_clusters()["clusters"]:
        grupos = eks.list_nodegroups(clusterName=cluster)["nodegroups"]
        fargate = eks.list_fargate_profiles(clusterName=cluster)["fargateProfileNames"]
        for grupo in grupos:
            estado = eks.describe_nodegroup(clusterName=cluster, nodegroupName=grupo)["nodegroup"]["status"]
            log.append(f"apagar node group {cluster}/{grupo} ({estado})")
            if not simular and estado != "DELETING":
                eks.delete_nodegroup(clusterName=cluster, nodegroupName=grupo)
        for perfil in fargate:
            log.append(f"apagar fargate {cluster}/{perfil}")
            if not simular:
                try:
                    eks.delete_fargate_profile(clusterName=cluster, fargateProfileName=perfil)
                except eks.exceptions.ResourceInUseException:
                    pass
        if grupos or fargate:
            pendentes.append(cluster)
            continue
        estado = eks.describe_cluster(name=cluster)["cluster"]["status"]
        log.append(f"apagar cluster {cluster} ({estado})")
        if not simular and estado != "DELETING":
            eks.delete_cluster(name=cluster)
    return pendentes


def desligar_load_balancers(elbv2, elb, simular, log):
    for pagina in elbv2.get_paginator("describe_load_balancers").paginate():
        for lb in pagina["LoadBalancers"]:
            log.append(f"apagar load balancer {lb['LoadBalancerName']}")
            if not simular:
                elbv2.delete_load_balancer(LoadBalancerArn=lb["LoadBalancerArn"])
    for lb in elb.describe_load_balancers()["LoadBalancerDescriptions"]:
        log.append(f"apagar load balancer clássico {lb['LoadBalancerName']}")
        if not simular:
            elb.delete_load_balancer(LoadBalancerName=lb["LoadBalancerName"])


def desligar_rede_e_instancias(ec2, simular, log):
    nats = ec2.describe_nat_gateways(Filter=[{"Name": "state", "Values": ["pending", "available"]}])["NatGateways"]
    for nat in nats:
        log.append(f"apagar NAT {nat['NatGatewayId']}")
        if not simular:
            ec2.delete_nat_gateway(NatGatewayId=nat["NatGatewayId"])
    for endereco in ec2.describe_addresses()["Addresses"]:
        if "AssociationId" in endereco:
            continue
        log.append(f"liberar Elastic IP {endereco.get('PublicIp')}")
        if not simular:
            ec2.release_address(AllocationId=endereco["AllocationId"])
    reservas = ec2.describe_instances(
        Filters=[{"Name": "instance-state-name", "Values": ["pending", "running", "stopping", "stopped"]}]
    )["Reservations"]
    ids = [i["InstanceId"] for r in reservas for i in r["Instances"]]
    if ids:
        log.append(f"terminar instâncias {', '.join(ids)}")
        if not simular:
            ec2.terminate_instances(InstanceIds=ids)
    return bool(nats)


def desligar_rds(rds, simular, log):
    carimbo = datetime.now(timezone.utc).strftime("%Y%m%d%H%M")
    for banco in rds.describe_db_instances()["DBInstances"]:
        nome, estado = banco["DBInstanceIdentifier"], banco["DBInstanceStatus"]
        if estado == "deleting":
            continue
        log.append(f"apagar RDS {nome} ({estado}, snapshot final={RDS_SNAPSHOT_FINAL})")
        if simular:
            continue
        if banco.get("DeletionProtection"):
            rds.modify_db_instance(DBInstanceIdentifier=nome, DeletionProtection=False, ApplyImmediately=True)
        argumentos = {"DBInstanceIdentifier": nome, "DeleteAutomatedBackups": True}
        if RDS_SNAPSHOT_FINAL:
            argumentos["FinalDBSnapshotIdentifier"] = f"{nome}-limite-{carimbo}"
        else:
            argumentos["SkipFinalSnapshot"] = True
        rds.delete_db_instance(**argumentos)


def handler(event, context):
    dados = _evento(event)
    simular = bool(dados.get("dryRun"))
    rodada = int(dados.get("rodada", 1))
    log = [f"rodada {rodada}, simulação={simular}"]

    congelar_roles(boto3.client("iam"), simular, log)
    pendentes = {}
    for regiao in REGIOES:
        eks = boto3.client("eks", region_name=regiao)
        desligar_load_balancers(boto3.client("elbv2", region_name=regiao), boto3.client("elb", region_name=regiao), simular, log)
        nat_pendente = desligar_rede_e_instancias(boto3.client("ec2", region_name=regiao), simular, log)
        desligar_rds(boto3.client("rds", region_name=regiao), simular, log)
        clusters = desligar_eks(eks, simular, log)
        if clusters or nat_pendente:
            pendentes[regiao] = clusters

    if pendentes and not simular:
        # Node groups levam minutos para sumir: espera aqui e, se não der tempo, chama a si mesma de novo.
        limite = time.time() + min(ESPERA_CLUSTER_S, max(0, context.get_remaining_time_in_millis() / 1000 - 90))
        while time.time() < limite and pendentes:
            time.sleep(30)
            for regiao in list(pendentes):
                restantes = desligar_eks(boto3.client("eks", region_name=regiao), False, log)
                desligar_rede_e_instancias(boto3.client("ec2", region_name=regiao), False, log)
                if restantes:
                    pendentes[regiao] = restantes
                else:
                    pendentes.pop(regiao)
        if pendentes and rodada < MAX_RODADAS:
            log.append(f"ainda pendente {pendentes}; nova rodada {rodada + 1}")
            boto3.client("lambda").invoke(
                FunctionName=context.function_name,
                InvocationType="Event",
                Payload=json.dumps({"rodada": rodada + 1}).encode(),
            )

    for linha in log:
        print(linha)
    return {"simulacao": simular, "rodada": rodada, "acoes": log, "pendentes": pendentes}
