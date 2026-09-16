#!/usr/bin/env python3
"""
TC-07: Provisionamento de Infraestrutura AWS via Terraform (IaC)
Aplica os planos Terraform de:
1. submodules/AutoReparos.Infra.K8s/terraform (VPC, EKS, ECR, API Gateway v2)
2. submodules/AutoReparos.Infra.Database/terraform (AWS RDS PostgreSQL 16 privado)
Salva os outputs gerados na seção 'aws_infra' de test_state.json.
"""

import json
import os
import subprocess
import sys
from pathlib import Path

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info, REPO_ROOT

K8S_TF_DIR = REPO_ROOT / "submodules" / "AutoReparos.Infra.K8s" / "terraform"
DB_TF_DIR = REPO_ROOT / "submodules" / "AutoReparos.Infra.Database" / "terraform"


def run_command(cmd: list, cwd: Path, desc: str, env_vars: dict = None) -> tuple[bool, str]:
    print_step(f"{desc} (Diretório: {cwd.relative_to(REPO_ROOT)})...")
    try:
        proc_env = os.environ.copy()
        if env_vars:
            proc_env.update(env_vars)
        proc = subprocess.run(cmd, cwd=cwd, env=proc_env, capture_output=True, text=True, check=False)
        output = proc.stdout + proc.stderr
        if proc.returncode != 0:
            print_error(f"Comando falhou (código {proc.returncode}): {' '.join(cmd)}\n{output}")
            return False, output
        print_success(f"{desc} executado com sucesso.")
        return True, output
    except Exception as e:
        print_error(f"Erro ao executar {desc}: {e}")
        return False, str(e)


def run_tc07() -> bool:
    sm = StateManager()
    print_header("TC-07: Provisionamento Terraform na Nuvem AWS")

    aws_env = {
        "AWS_ACCESS_KEY_ID": sm.env.get("AWS_ACCESS_KEY_ID", ""),
        "AWS_SECRET_ACCESS_KEY": sm.env.get("AWS_SECRET_ACCESS_KEY", ""),
        "AWS_DEFAULT_REGION": sm.env.get("AWS_REGION", "us-east-1"),
    }
    if sm.env.get("AWS_SESSION_TOKEN"):
        aws_env["AWS_SESSION_TOKEN"] = sm.env["AWS_SESSION_TOKEN"]

    # 1. Terraform K8s (VPC, EKS, API Gateway v2)
    state = sm.load_full_state()
    aws_infra = state.get("aws_infra", {})
    if aws_infra.get("status") == "K8S_PROVISIONED" and aws_infra.get("vpc_id"):
        print_info("K8s e VPC já provisionados no estado local. Pulando reaplicação do K8s...")
        vpc_id = aws_infra.get("vpc_id")
        private_subnet_ids = aws_infra.get("private_subnet_ids", [])
        cluster_name = aws_infra.get("cluster_name")
        api_gateway_endpoint = aws_infra.get("api_gateway_endpoint")
        ecr_repo_url = aws_infra.get("ecr_repository_url")
        cluster_sg_id = aws_infra.get("cluster_security_group_id")
    else:
        ok, _ = run_command(["terraform", "init"], K8S_TF_DIR, "Terraform Init (K8s & VPC)", aws_env)
        if not ok:
            return False

        ok, _ = run_command(["terraform", "apply", "-auto-approve"], K8S_TF_DIR, "Terraform Apply (K8s & VPC)", aws_env)
        if not ok:
            return False

        # Captura outputs do K8s
        ok, k8s_out_json = run_command(["terraform", "output", "-json"], K8S_TF_DIR, "Capturando Outputs do K8s", aws_env)
        if not ok:
            return False

        k8s_outputs = json.loads(k8s_out_json)
        vpc_id = k8s_outputs.get("vpc_id", {}).get("value")
        private_subnet_ids = k8s_outputs.get("private_subnet_ids", {}).get("value", [])
        cluster_name = k8s_outputs.get("cluster_name", {}).get("value")
        api_gateway_endpoint = k8s_outputs.get("api_gateway_endpoint", {}).get("value")
        ecr_repo_url = k8s_outputs.get("ecr_repository_url", {}).get("value")
        cluster_sg_id = k8s_outputs.get("cluster_security_group_id", {}).get("value")

    print_info(f"VPC provisionada: {vpc_id}")
    print_info(f"Cluster EKS: {cluster_name}")
    print_info(f"API Gateway Endpoint: {api_gateway_endpoint}")

    # 2. Terraform Database (RDS PostgreSQL 16)
    # Configura terraform.tfvars dinamicamente no módulo de database
    tfvars_path = DB_TF_DIR / "terraform.tfvars"
    subnets_str = json.dumps(private_subnet_ids)
    sg_str = json.dumps([cluster_sg_id] if cluster_sg_id else [])
    tfvars_content = f"""vpc_id = "{vpc_id}"
private_subnet_ids = {subnets_str}
allowed_security_group_ids = {sg_str}
environment = "production"
aws_region = "us-east-1"
"""
    with open(tfvars_path, "w", encoding="utf-8") as f:
        f.write(tfvars_content)
    print_info(f"Arquivo terraform.tfvars configurado para RDS em {tfvars_path.name}")

    ok, _ = run_command(["terraform", "init"], DB_TF_DIR, "Terraform Init (RDS Database)", aws_env)
    if not ok:
        return False

    ok, _ = run_command(["terraform", "apply", "-auto-approve"], DB_TF_DIR, "Terraform Apply (RDS Database)", aws_env)
    if not ok:
        return False

    ok, db_out_json = run_command(["terraform", "output", "-json"], DB_TF_DIR, "Capturando Outputs do Database", aws_env)
    if not ok:
        return False

    db_outputs = json.loads(db_out_json)
    db_endpoint = db_outputs.get("db_endpoint", {}).get("value")
    print_info(f"RDS PostgreSQL Endpoint: {db_endpoint}")

    # Salva estado consolidado
    sm.update_section("aws_infra", {
        "vpc_id": vpc_id,
        "private_subnet_ids": private_subnet_ids,
        "cluster_name": cluster_name,
        "api_gateway_endpoint": api_gateway_endpoint,
        "ecr_repository_url": ecr_repo_url,
        "db_endpoint": db_endpoint,
        "status": "PROVISIONED"
    })

    print_success("Subfase TC-07 (Provisionamento AWS IaC) concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc07()
    sys.exit(0 if success else 1)
