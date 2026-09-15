#!/usr/bin/env python3
"""
TC-03: Catálogo de Serviços e Controle de Insumos (Estoque)
Cadastra Insumo controlado com estoque inicial de 10 unidades e Serviço de Oficina.
Salva 'insumo_id', 'servico_id' e metadados na seção 'tc03_catalogo'.
"""

import random
import sys
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def run_tc03() -> bool:
    sm = StateManager()
    base_url = sm.get_config("base_url_local", "http://localhost:8080")

    auth_data = sm.get_section("tc01_auth")
    token = auth_data.get("admin_token")

    if not token:
        print_error("Token de admin não encontrado em tc01_auth! Execute tc01_auth_rbac.py primeiro.")
        return False

    print_header("TC-03: Catálogo de Serviços e Controle de Insumos")
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    }

    # Caso TC-3.1: Cadastrar Insumo com Estoque
    sufixo = random.randint(100, 999)
    insumo_nome = f"Óleo Sintético 5W30 #{sufixo}"
    print_step(f"TC-3.1: Cadastrando insumo com estoque inicial ({insumo_nome})...")
    insumo_payload = {
        "nome": insumo_nome,
        "descricao": "Óleo sintético de alta performance para motores flex",
        "valor": 45.00,
        "quantidadeEstoque": 10
    }

    resp_insumo = requests.post(f"{base_url}/api/insumos", json=insumo_payload, headers=headers, timeout=10)
    if resp_insumo.status_code not in (200, 201):
        print_error(f"Falha ao cadastrar insumo. HTTP {resp_insumo.status_code}: {resp_insumo.text}")
        return False

    insumo_data = resp_insumo.json()
    insumo_id = insumo_data.get("id")
    print_success(f"Insumo cadastrado com sucesso! ID: {insumo_id} | Estoque inicial: 10 unidades | Valor: R$ 45,00")

    # Caso TC-3.2: Cadastrar Serviço de Oficina
    servico_nome = f"Troca de Óleo e Filtros #{sufixo}"
    print_step(f"TC-3.2: Cadastrando serviço de catálogo ({servico_nome})...")
    servico_payload = {
        "nome": servico_nome,
        "descricao": "Substituição completa do óleo do cárter e filtros do motor",
        "valorTabelado": 120.00
    }

    resp_servico = requests.post(f"{base_url}/api/servicos", json=servico_payload, headers=headers, timeout=10)
    if resp_servico.status_code not in (200, 201):
        print_error(f"Falha ao cadastrar serviço. HTTP {resp_servico.status_code}: {resp_servico.text}")
        return False

    servico_data = resp_servico.json()
    servico_id = servico_data.get("id")
    print_success(f"Serviço cadastrado com sucesso! ID: {servico_id} | Valor Mão de Obra: R$ 120,00")

    # Salva no estado
    sm.update_section("tc03_catalogo", {
        "insumo_id": insumo_id,
        "insumo_nome": insumo_nome,
        "insumo_estoque_inicial": 10,
        "insumo_valor": 45.00,
        "servico_id": servico_id,
        "servico_nome": servico_nome,
        "servico_valor": 120.00,
        "status": "PASSED"
    })
    print_success("Subfase TC-03 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc03()
    sys.exit(0 if success else 1)
