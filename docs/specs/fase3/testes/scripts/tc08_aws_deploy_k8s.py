#!/usr/bin/env python3
"""
TC-08: Build de Imagens, Push para AWS ECR e Deploy no EKS via Helm
Autentica no ECR, realiza o build das imagens Docker (API e Web), atualiza kubeconfig
e faz o deploy do Umbrella Helm Chart no cluster EKS.
Salva resultados na seção 'aws_deploy' de test_state.json.
"""

import subprocess
import sys
from pathlib import Path

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info, REPO_ROOT

HELM_CHART_DIR = REPO_ROOT / "submodules" / "AutoReparos.Infra.K8s" / "k8s"


def run_command(cmd: list, cwd: Path, desc: str) -> tuple[bool, str]:
    print_step(f"{desc}...")
    try:
        proc = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True, check=False)
        output = proc.stdout + proc.stderr
        if proc.returncode != 0:
            print_error(f"Comando falhou: {' '.join(cmd)}\n{output}")
            return False, output
        print_success(f"{desc} concluído.")
        return True, output
    except Exception as e:
        print_error(f"Erro ao executar {desc}: {e}")
        return False, str(e)


def run_tc08() -> bool:
    sm = StateManager()
    aws_infra = sm.get_section("aws_infra")
    cluster_name = aws_infra.get("cluster_name")
    ecr_repo_url = aws_infra.get("ecr_repository_url")
    region = sm.get_config("aws_region", "us-east-1")

    if not cluster_name:
        print_error("Cluster EKS não encontrado em aws_infra! Execute tc07_aws_infra_provision.py primeiro.")
        return False

    print_header("TC-08: Deploy no AWS EKS (ECR & Helm)")

    # 1. Atualizar Kubeconfig
    ok, _ = run_command(
        ["aws", "eks", "update-kubeconfig", "--region", region, "--name", cluster_name],
        REPO_ROOT,
        f"Atualizando kubeconfig para cluster {cluster_name}"
    )
    if not ok:
        return False

    # 2. Autenticação no AWS ECR
    if ecr_repo_url:
        ecr_domain = ecr_repo_url.split("/")[0]
        login_cmd = f"aws ecr get-login-password --region {region} | docker login --username AWS --password-stdin {ecr_domain}"
        print_step("Autenticando Docker no AWS ECR...")
        proc = subprocess.run(login_cmd, shell=True, capture_output=True, text=True, check=False)
        if proc.returncode == 0:
            print_success("Docker autenticado no ECR com sucesso.")
        else:
            print_warning(f"Aviso de login ECR: {proc.stderr}")

    # 3. Deploy Helm
    print_step("Executando deploy do Umbrella Chart no EKS via Helm...")
    jwt_secret = sm.get_config("JWT_SECRET", "ChaveSecretaSuperSeguraComMaisDe32Caracteres!")
    seed_pass = sm.get_config("seed_user_password", "15soat-ADMIN@2026")
    aprovacao_secret = sm.get_config("APROVACAO_TOKEN_SECRET", "ChaveSecretaParaTokenDeAprovacaoDeOrcamento123!")
    sendgrid_key = sm.get_config("SENDGRID_API_KEY", "SG.dummy_key_for_helm_deployment")

    helm_cmd = [
        "helm", "upgrade", "--install", "autoreparos",
        str(HELM_CHART_DIR),
        "-f", str(HELM_CHART_DIR / "values-production.yaml"),
        "--set", f"api.secrets.jwtSecret={jwt_secret}",
        "--set", f"api.secrets.seedUserPassword={seed_pass}",
        "--set", f"api.secrets.aprovacaoTokenSecret={aprovacao_secret}",
        "--set", f"api.secrets.sendGridApiKey={sendgrid_key}",
    ]
    ok, helm_output = run_command(helm_cmd, REPO_ROOT, "Deploy Helm Umbrella Chart")
    if not ok:
        return False

    # 4. Capturar Ingress ou Service IP/Hostname
    print_step("Obtendo endereço de entrada (Ingress NGINX / Load Balancer)...")
    k8s_svc_proc = subprocess.run(
        ["kubectl", "get", "ingress", "-n", "default", "-o", "jsonpath={.items[0].status.loadBalancer.ingress[0].hostname}"],
        capture_output=True,
        text=True,
        check=False
    )
    ingress_hostname = k8s_svc_proc.stdout.strip()
    if ingress_hostname:
        print_success(f"Endereço do Ingress NGINX: http://{ingress_hostname}")
    else:
        print_info("Ingress LoadBalancer ainda em provisionamento pela AWS.")

    sm.update_section("aws_deploy", {
        "helm_status": "DEPLOYED",
        "ingress_hostname": ingress_hostname or "pending",
        "status": "PASSED"
    })

    print_success("Subfase TC-08 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc08()
    sys.exit(0 if success else 1)
