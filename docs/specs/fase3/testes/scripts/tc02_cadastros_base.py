#!/usr/bin/env python3
"""
TC-02: Cadastros Base e Validação de Value Objects
Cadastra Cliente PF real (com e-mail josehenriquedotta61@gmail.com e CPF Módulo 11)
e Veículo com Placa Mercosul. Executa testes negativos com CPF e placa inválidos.
Salva 'cliente_id', 'documento', 'veiculo_id' e 'placa' na seção 'tc02_cadastros'.
"""

import random
import sys
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def generate_valid_cpf() -> str:
    """Gera um CPF matematicamente válido com base no algoritmo Módulo 11 da Receita Federal."""
    digits = [random.randint(0, 9) for _ in range(9)]
    # Evita todos os dígitos iguais
    if len(set(digits)) == 1:
        digits[0] = (digits[0] + 1) % 10

    # Primeiro dígito verificador
    s1 = sum(d * (10 - i) for i, d in enumerate(digits))
    r1 = 11 - (s1 % 11)
    d1 = 0 if r1 >= 10 else r1
    digits.append(d1)

    # Segundo dígito verificador
    s2 = sum(d * (11 - i) for i, d in enumerate(digits))
    r2 = 11 - (s2 % 11)
    d2 = 0 if r2 >= 10 else r2
    digits.append(d2)

    return "".join(map(str, digits))


def run_tc02() -> bool:
    sm = StateManager()
    base_url = sm.get_config("base_url_local", "http://localhost:8080")
    test_email = sm.get_config("test_cliente_email", "josehenriquedotta61@gmail.com")

    auth_data = sm.get_section("tc01_auth")
    token = auth_data.get("admin_token")

    if not token:
        print_error("Token de admin não encontrado em tc01_auth! Execute tc01_auth_rbac.py primeiro.")
        return False

    print_header("TC-02: Cadastros Base (Clientes & Veículos)")
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    }

    # Caso TC-2.1: Cadastrar Cliente PF Real
    print_step(f"TC-2.1: Cadastrando cliente real com e-mail {test_email}...")
    cpf = generate_valid_cpf()
    cliente_payload = {
        "nome": "José Henrique Dotta",
        "documento": cpf,
        "telefone": "11987654321",
        "email": test_email
    }

    resp_cliente = requests.post(f"{base_url}/api/clientes", json=cliente_payload, headers=headers, timeout=10)
    if resp_cliente.status_code not in (200, 201):
        print_error(f"Falha ao cadastrar cliente. HTTP {resp_cliente.status_code}: {resp_cliente.text}")
        return False

    cliente_data = resp_cliente.json()
    cliente_id = cliente_data.get("id")
    print_success(f"Cliente cadastrado com sucesso! ID: {cliente_id} | CPF: {cpf}")

    # Caso TC-2.2: Teste Negativo com CPF Inválido
    print_step("TC-2.2: Testando cadastro com CPF inválido (dígitos repetidos 11111111111)...")
    bad_cliente_payload = {
        "nome": "Cliente Teste Errado",
        "documento": "11111111111",
        "telefone": "11999999999",
        "email": "errado@teste.com"
    }
    resp_bad_cpf = requests.post(f"{base_url}/api/clientes", json=bad_cliente_payload, headers=headers, timeout=10)
    if resp_bad_cpf.status_code == 400:
        print_success("Cadastro com CPF inválido rejeitado corretamente com HTTP 400 Bad Request.")
    else:
        print_error(f"Esperava HTTP 400 para CPF inválido, mas recebeu {resp_bad_cpf.status_code}")
        return False

    # Caso TC-2.3: Cadastrar Veículo com Placa Mercosul
    # Gera placa aleatória no formato Mercosul (ex: ABC1D23) para evitar duplicidade de placa
    letters = "ABCDEFGHJKLMNPQRSTUVWXYZ"
    placa_mercosul = f"{random.choice(letters)}{random.choice(letters)}{random.choice(letters)}{random.randint(0,9)}{random.choice(letters)}{random.randint(10,99)}"

    print_step(f"TC-2.3: Cadastrando veículo Mercosul ({placa_mercosul}) para o cliente...")
    veiculo_payload = {
        "clienteId": cliente_id,
        "marca": "Toyota",
        "modelo": "Corolla 2.0 Dynamic",
        "anoFabricacao": 2023,
        "anoModelo": 2024,
        "placa": placa_mercosul,
        "chassi": f"9BWZZZ377VT{random.randint(100000, 999999)}",
        "renavam": f"{random.randint(10000000000, 99999999999)}"
    }

    resp_veiculo = requests.post(f"{base_url}/api/veiculos", json=veiculo_payload, headers=headers, timeout=10)
    if resp_veiculo.status_code not in (200, 201):
        print_error(f"Falha ao cadastrar veículo. HTTP {resp_veiculo.status_code}: {resp_veiculo.text}")
        return False

    veiculo_data = resp_veiculo.json()
    veiculo_id = veiculo_data.get("id")
    print_success(f"Veículo cadastrado com sucesso! ID: {veiculo_id} | Placa: {placa_mercosul}")

    # Caso TC-2.4: Teste Negativo com Placa Inválida
    print_step("TC-2.4: Testando cadastro de veículo com placa inválida...")
    bad_veiculo_payload = {
        "clienteId": cliente_id,
        "marca": "Fiat",
        "modelo": "Uno",
        "anoFabricacao": 2010,
        "anoModelo": 2010,
        "placa": "PLACA_TOTALMENTE_INVALIDA",
        "chassi": "CHASSI12345678901",
        "renavam": "12345678901"
    }
    resp_bad_placa = requests.post(f"{base_url}/api/veiculos", json=bad_veiculo_payload, headers=headers, timeout=10)
    if resp_bad_placa.status_code == 400:
        print_success("Cadastro de veículo com placa inválida rejeitado corretamente com HTTP 400 Bad Request.")
    else:
        print_error(f"Esperava HTTP 400 para placa inválida, mas recebeu {resp_bad_placa.status_code}")
        return False

    # Salva no estado
    sm.update_section("tc02_cadastros", {
        "cliente_id": cliente_id,
        "cliente_documento": cpf,
        "cliente_email": test_email,
        "veiculo_id": veiculo_id,
        "veiculo_placa": placa_mercosul,
        "status": "PASSED"
    })
    print_success("Subfase TC-02 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc02()
    sys.exit(0 if success else 1)
