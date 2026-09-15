#!/usr/bin/env python3
"""
TC-01: Autenticação, RBAC e Gestão de Tokens JWT
Testa login com usuário seed admin, verifica emissão do token e testes negativos (401 e credencial errada).
Salva 'admin_token' na seção 'tc01_auth' de test_state.json.
"""

import sys
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def run_tc01() -> bool:
    sm = StateManager()
    base_url = sm.get_config("base_url_local", "http://localhost:8080")
    email = sm.get_config("seed_user_email", "admin@autoreparos.com")
    password = sm.get_config("seed_user_password")

    print_header("TC-01: Autenticação, RBAC e Gestão de Tokens")

    if not password:
        print_error("SEED_USER_PASSWORD não configurada no .env!")
        return False

    session = requests.Session()

    # Caso TC-1.1: Login com Seed Admin (Sucesso)
    print_step(f"TC-1.1: Realizando login com seed admin ({email})...")
    login_url = f"{base_url}/api/auth/login"
    login_payload = {
        "email": email,
        "password": password
    }

    try:
        resp = session.post(login_url, json=login_payload, timeout=10)
    except requests.exceptions.RequestException as e:
        print_error(f"Falha de conexão com a API em {login_url}: {e}")
        return False

    if resp.status_code != 200:
        print_error(f"Login falhou. HTTP Status: {resp.status_code}. Resposta: {resp.text}")
        return False

    data = resp.json()
    token = data.get("token") or data.get("accessToken")
    expires_in = data.get("expiresIn", 3600)

    if not token:
        print_error("Token não retornado no payload de login!")
        return False

    print_success(f"Login efetuado com sucesso! Token JWT emitido (expira em {expires_in}s).")

    # Caso TC-1.2: Acesso à rota protegida sem token (Deve retornar 401)
    print_step("TC-1.2: Testando acesso a endpoint protegido sem token Authorization...")
    protected_url = f"{base_url}/api/ordem-servico"
    resp_unauth = requests.get(protected_url, timeout=10)
    if resp_unauth.status_code == 401:
        print_success("Acesso não autenticado bloqueado corretamente com HTTP 401 Unauthorized.")
    else:
        print_error(f"Esperava HTTP 401, mas recebeu {resp_unauth.status_code}")
        return False

    # Caso TC-1.3: Login com senha inválida (Deve falhar)
    print_step("TC-1.3: Testando login com senha incorreta...")
    bad_login_payload = {
        "email": email,
        "password": "SenhaCompletamenteIncorreta123!"
    }
    resp_bad = requests.post(login_url, json=bad_login_payload, timeout=10)
    if resp_bad.status_code in (400, 401):
        print_success(f"Tentativa de login com senha inválida rejeitada corretamente (HTTP {resp_bad.status_code}).")
    else:
        print_error(f"Esperava HTTP 400 ou 401, mas recebeu {resp_bad.status_code}")
        return False

    # Salva no estado
    sm.update_section("tc01_auth", {
        "admin_token": token,
        "expires_in": expires_in,
        "status": "PASSED"
    })
    print_success("Subfase TC-01 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc01()
    sys.exit(0 if success else 1)
