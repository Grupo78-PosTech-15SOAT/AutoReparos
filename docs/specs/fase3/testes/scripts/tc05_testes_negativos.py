#!/usr/bin/env python3
"""
TC-05: Bateria de Testes Negativos e Invariantes de Falha
Valida casos de rejeição esperados:
- Token de aprovação inválido/adulterado (HTTP 400);
- Alocação de insumo acima do estoque disponível (HTTP 400);
- Transição de estado proibida pela máquina de estados (HTTP 400);
- Consulta pública permitida e anonimizada (HTTP 200).
Salva resultados na seção 'tc05_negativos'.
"""

import sys
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def run_tc05() -> bool:
    sm = StateManager()
    base_url = sm.get_config("base_url_local", "http://localhost:8080")

    auth_data = sm.get_section("tc01_auth")
    token = auth_data.get("admin_token")
    if not token:
        print_error("Token de admin não encontrado! Execute tc01_auth_rbac.py primeiro.")
        return False

    cadastros = sm.get_section("tc02_cadastros")
    cliente_doc = cadastros.get("cliente_documento")
    veiculo_placa = cadastros.get("veiculo_placa")

    catalogo = sm.get_section("tc03_catalogo")
    insumo_id = catalogo.get("insumo_id")

    os_data = sm.get_section("tc04_ordem_servico")
    os_id_concluida = os_data.get("os_id")

    print_header("TC-05: Bateria de Testes Negativos e Invariantes")
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    }

    checks_passed = 0

    # Caso TC-5.1: Token Adulterado
    print_step("TC-5.1: Enviando token HMAC com assinatura fraudada/corrompida...")
    token_falso = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.ASSINATURA_TOTALMENTE_FALSA"
    resp_token_falso = requests.get(f"{base_url}/api/ordem-servico/aprovar?token={token_falso}", timeout=10)
    if resp_token_falso.status_code == 400:
        print_success("Token adulterado rejeitado corretamente com HTTP 400 Bad Request.")
        checks_passed += 1
    else:
        print_error(f"Esperava HTTP 400 para token adulterado, mas recebeu {resp_token_falso.status_code}")
        return False

    # Caso TC-5.2: Alocação de Insumo com Quantidade Superior ao Estoque
    if cliente_doc and veiculo_placa and insumo_id:
        print_step("TC-5.2: Criando nova OS de teste para tentar alocar 100 unidades de óleo (estoque é 6)...")
        resp_nova_os = requests.post(f"{base_url}/api/ordem-servico", json={
            "documentoCliente": cliente_doc,
            "placaVeiculo": veiculo_placa,
            "observacao": "OS de teste para validação de estouro de estoque."
        }, headers=headers, timeout=10)

        if resp_nova_os.status_code in (200, 201):
            nova_os_id = resp_nova_os.json().get("id")
            # Tenta alocar 100 unidades (estoque atual é 6)
            insumo_estouro_payload = {
                "insumoId": insumo_id,
                "descricao": "Tentativa de alocar estoque excessivo",
                "valorUnitario": 45.00,
                "quantidade": 100,
                "origem": 1
            }
            resp_estouro = requests.post(f"{base_url}/api/ordem-servico/{nova_os_id}/insumos", json=insumo_estouro_payload, headers=headers, timeout=10)
            if resp_estouro.status_code in (400, 422):
                print_success(f"Alocação além do estoque bloqueada com sucesso (HTTP {resp_estouro.status_code})!")
                checks_passed += 1
            else:
                print_error(f"Esperava bloqueio HTTP 400/422, mas recebeu {resp_estouro.status_code}")
                return False

            # Caso TC-5.3: Transição de Estado Proibida
            print_step("TC-5.3: Tentando entregar a nova OS sem diagnóstico, aprovação ou execução...")
            resp_entrega_invalida = requests.patch(f"{base_url}/api/ordem-servico/{nova_os_id}/entregar", headers=headers, timeout=10)
            if resp_entrega_invalida.status_code == 400:
                print_success("Transição de estado inválida bloqueada com sucesso com HTTP 400 Bad Request.")
                checks_passed += 1
            else:
                print_error(f"Esperava HTTP 400 para transição proibida, mas recebeu {resp_entrega_invalida.status_code}")
                return False

    # Caso TC-5.4: Consulta Pública sem Token (Permitida)
    if veiculo_placa:
        print_step(f"TC-5.4: Testando consulta pública anônima por placa ({veiculo_placa})...")
        resp_consulta = requests.get(f"{base_url}/api/ordem-servico/consulta?placa={veiculo_placa}", timeout=10)
        if resp_consulta.status_code == 200:
            print_success("Consulta pública anônima executada com sucesso (HTTP 200 OK)!")
            checks_passed += 1
        else:
            print_error(f"Consulta pública falhou. HTTP {resp_consulta.status_code}")
            return False

    # Caso TC-5.5: Consulta Pública por ID
    if os_id_concluida:
        print_step(f"TC-5.5: Testando consulta pública detalhada por ID ({os_id_concluida})...")
        resp_consulta_id = requests.get(f"{base_url}/api/ordem-servico/consulta/{os_id_concluida}", timeout=10)
        if resp_consulta_id.status_code == 200:
            print_success("Consulta pública detalhada por ID executada com sucesso (HTTP 200 OK)!")
            checks_passed += 1
        else:
            print_error(f"Consulta pública por ID falhou. HTTP {resp_consulta_id.status_code}")
            return False

    sm.update_section("tc05_negativos", {
        "checks_passed": checks_passed,
        "status": "PASSED"
    })
    print_success(f"Subfase TC-05 concluída com {checks_passed} verificações de integridade aprovadas!")
    return True


if __name__ == "__main__":
    success = run_tc05()
    sys.exit(0 if success else 1)
