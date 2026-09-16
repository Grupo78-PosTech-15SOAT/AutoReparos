#!/usr/bin/env python3
"""
TC-09: Testes de Fumaça (Smoke Tests) na Nuvem AWS
Valida:
1. Endpoint público do AWS API Gateway v2 (/health);
2. Rota serverless da AWS Lambda (/auth/cliente com emissão de JWT);
3. Roteamento via VPC Link para o EKS (/api/ordem-servico/consulta);
4. Validação de cabeçalhos CORS (Access-Control-Allow-Origin).
Salva resultados na seção 'aws_smoke' de test_state.json.
"""

import sys
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def run_tc09() -> bool:
    sm = StateManager()
    aws_infra = sm.get_section("aws_infra")
    api_gw_endpoint = aws_infra.get("api_gateway_endpoint")
    test_email = sm.get_config("test_cliente_email", "josehenriquedotta61@gmail.com")

    cadastros = sm.get_section("tc02_cadastros")
    cliente_cpf = cadastros.get("cliente_documento", "52998224725")

    if not api_gw_endpoint:
        print_error("Endpoint do API Gateway não encontrado! Execute tc07_aws_infra_provision.py primeiro.")
        return False

    print_header("TC-09: Smoke Tests na Nuvem AWS")
    results = {}

    # 1. Healthcheck do API Gateway
    print_step(f"TC-9.1: Testando healthcheck do API Gateway ({api_gw_endpoint}/health)...")
    try:
        resp_health = requests.get(f"{api_gw_endpoint}/health", timeout=10)
        if resp_health.status_code == 200:
            print_success("API Gateway Healthcheck respondeu HTTP 200 OK!")
            results["api_gateway_health"] = "OK"
        else:
            print_warning(f"API Gateway Healthcheck respondeu HTTP {resp_health.status_code}")
            results["api_gateway_health"] = f"HTTP_{resp_health.status_code}"
    except Exception as e:
        print_error(f"Falha de conexão com API Gateway: {e}")
        results["api_gateway_health"] = "UNREACHABLE"

    # 2. AuthLambda via API Gateway
    print_step("TC-9.2: Testando autenticação de cliente via AWS Lambda Serverless (/auth/cliente)...")
    try:
        lambda_payload = {
            "cpf": cliente_cpf,
            "email": test_email
        }
        resp_auth = requests.post(f"{api_gw_endpoint}/auth/cliente", json=lambda_payload, timeout=15)
        if resp_auth.status_code == 200:
            token_cliente = resp_auth.json().get("token")
            print_success("AuthLambda executada com sucesso! Token JWT de cliente emitido pela AWS.")
            results["auth_lambda_status"] = "OK"
            results["cliente_jwt_emitted"] = bool(token_cliente)
        else:
            print_warning(f"AuthLambda respondeu HTTP {resp_auth.status_code}: {resp_auth.text}")
            results["auth_lambda_status"] = f"HTTP_{resp_auth.status_code}"
    except Exception as e:
        print_error(f"Erro ao invocar AuthLambda: {e}")
        results["auth_lambda_status"] = "ERROR"

    # 3. Proxy Reverso VPC Link para o EKS
    print_step("TC-9.3: Testando proxy reverso para os Pods do EKS (/api/ordem-servico/consulta)...")
    try:
        resp_proxy = requests.get(f"{api_gw_endpoint}/api/ordem-servico/consulta", timeout=15)
        if resp_proxy.status_code in (200, 400):
            print_success("Roteamento VPC Link -> EKS Ingress operando com sucesso!")
            results["vpc_link_proxy"] = "OK"
        else:
            print_warning(f"Proxy EKS respondeu HTTP {resp_proxy.status_code}")
            results["vpc_link_proxy"] = f"HTTP_{resp_proxy.status_code}"
    except Exception as e:
        print_error(f"Erro ao testar proxy VPC Link: {e}")
        results["vpc_link_proxy"] = "ERROR"

    # 4. Verificação de CORS
    print_step("TC-9.4: Testando cabeçalhos de CORS preflight (OPTIONS)...")
    try:
        cors_headers = {
            "Origin": "http://localhost:4200",
            "Access-Control-Request-Method": "POST",
            "Access-Control-Request-Headers": "Authorization,Content-Type"
        }
        resp_cors = requests.options(f"{api_gw_endpoint}/api/ordem-servico/consulta", headers=cors_headers, timeout=10)
        allow_origin = resp_cors.headers.get("Access-Control-Allow-Origin")
        if allow_origin:
            print_success(f"Headers CORS configurados corretamente: Access-Control-Allow-Origin = {allow_origin}")
            results["cors_status"] = "OK"
        else:
            print_info("Sem cabeçalho CORS explícito no preflight (verificar Ingress/Gateway).")
            results["cors_status"] = "MISSING_HEADER"
    except Exception as e:
        print_warning(f"Aviso no teste de CORS: {e}")
        results["cors_status"] = "ERROR"

    sm.update_section("aws_smoke", results)
    print_success("Subfase TC-09 concluída!")
    return True


if __name__ == "__main__":
    success = run_tc09()
    sys.exit(0 if success else 1)
