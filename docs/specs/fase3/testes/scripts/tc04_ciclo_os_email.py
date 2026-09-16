#!/usr/bin/env python3
"""
TC-04: Ciclo E2E da Ordem de Serviço com Disparo Real SendGrid e Pausa Interativa
Cria OS -> Adiciona Itens -> Inicia Diagnóstico -> Envia p/ Aprovação (SendGrid)
-> Pausa interativa para o usuário clicar no e-mail real -> Inicia e Conclui Serviço
-> Entrega Veículo -> Valida Débito de Estoque de Insumo no PostgreSQL.
Salva 'os_id', 'status_final' e 'estoque_remanescente' na seção 'tc04_ordem_servico'.
"""

import sys
import time
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info, Colors


def run_tc04(interactive: bool = True) -> bool:
    sm = StateManager()
    base_url = sm.get_config("base_url_local", "http://localhost:8080")

    auth_data = sm.get_section("tc01_auth")
    token = auth_data.get("admin_token")
    if not token:
        print_error("Token de admin não encontrado! Execute tc01_auth_rbac.py primeiro.")
        return False

    cadastros = sm.get_section("tc02_cadastros")
    cliente_doc = cadastros.get("cliente_documento")
    cliente_email = cadastros.get("cliente_email")
    veiculo_placa = cadastros.get("veiculo_placa")
    if not (cliente_doc and veiculo_placa):
        print_error("Dados de cliente/veículo não encontrados! Execute tc02_cadastros_base.py primeiro.")
        return False

    catalogo = sm.get_section("tc03_catalogo")
    insumo_id = catalogo.get("insumo_id")
    servico_id = catalogo.get("servico_id")
    if not (insumo_id and servico_id):
        print_error("Insumo/Serviço não encontrados! Execute tc03_catalogo_estoque.py primeiro.")
        return False

    print_header("TC-04: Ciclo E2E da Ordem de Serviço com Envio Real de E-mail")
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    }

    # Caso TC-4.1: Criar Ordem de Serviço
    print_step("TC-4.1: Criando Ordem de Serviço em rascunho...")
    os_payload = {
        "documentoCliente": cliente_doc,
        "placaVeiculo": veiculo_placa,
        "observacao": "Revisão preventiva de 30.000 km e troca de óleo e filtros."
    }

    resp_os = requests.post(f"{base_url}/api/ordem-servico", json=os_payload, headers=headers, timeout=10)
    if resp_os.status_code not in (200, 201):
        print_error(f"Falha ao criar OS. HTTP {resp_os.status_code}: {resp_os.text}")
        return False

    os_data = resp_os.json()
    os_id = os_data.get("id")
    print_success(f"Ordem de Serviço criada com sucesso! ID: {os_id}")

    # Caso TC-4.2: Adicionar Serviço e Insumo à OS
    print_step("TC-4.2A: Vinculando serviço de catálogo à OS...")
    servico_payload = {
        "servicoId": servico_id,
        "valorCobrado": 120.00
    }
    resp_add_serv = requests.post(f"{base_url}/api/ordem-servico/{os_id}/servicos", json=servico_payload, headers=headers, timeout=10)
    if resp_add_serv.status_code not in (200, 204):
        print_error(f"Falha ao adicionar serviço. HTTP {resp_add_serv.status_code}: {resp_add_serv.text}")
        return False
    print_success("Serviço adicionado à OS com sucesso.")

    print_step("TC-4.2B: Vinculando 4 unidades do insumo (óleo) à OS...")
    insumo_payload = {
        "insumoId": insumo_id,
        "descricao": "Óleo 5W30 Sintético",
        "valorUnitario": 45.00,
        "quantidade": 4,
        "origem": 1  # Estoque = 1
    }
    resp_add_insumo = requests.post(f"{base_url}/api/ordem-servico/{os_id}/insumos", json=insumo_payload, headers=headers, timeout=10)
    if resp_add_insumo.status_code not in (200, 204):
        print_error(f"Falha ao adicionar insumo. HTTP {resp_add_insumo.status_code}: {resp_add_insumo.text}")
        return False
    print_success("Insumo (4 unidades) alocado na OS com sucesso.")

    # Caso TC-4.3: Iniciar Diagnóstico
    print_step("TC-4.3: Iniciando diagnóstico da OS...")
    resp_diag = requests.patch(f"{base_url}/api/ordem-servico/{os_id}/iniciar-diagnostico", headers=headers, timeout=10)
    if resp_diag.status_code not in (200, 204):
        print_error(f"Falha ao iniciar diagnóstico. HTTP {resp_diag.status_code}: {resp_diag.text}")
        return False
    print_success("Diagnóstico iniciado com sucesso.")

    # Caso TC-4.4: Enviar para Aprovação (Disparo SendGrid)
    print_step(f"TC-4.4: Enviando OS para aprovação do cliente (Disparo SendGrid para {cliente_email})...")
    resp_env = requests.patch(f"{base_url}/api/ordem-servico/{os_id}/enviar-para-aprovacao", headers=headers, timeout=15)
    if resp_env.status_code not in (200, 204):
        print_error(f"Falha ao enviar para aprovação. HTTP {resp_env.status_code}: {resp_env.text}")
        return False
    print_success("Ordem de serviço enviada para aprovação! Notificação SendGrid despachada.")

    # Caso TC-4.5: [PAUSA INTERATIVA] Aguardo de Aprovação Real pelo Usuário
    print("\n" + f"{Colors.WARNING}{Colors.BOLD}{'=' * 78}{Colors.ENDC}")
    print(f"{Colors.WARNING}{Colors.BOLD}[INTERVENÇÃO HUMANA REAL - NOTIFICAÇÃO SENDGRID ENVIADA]{Colors.ENDC}")
    print(f"{Colors.CYAN}O sistema despachou um e-mail com token assinado para:{Colors.ENDC}")
    print(f"👉 {Colors.BOLD}{cliente_email}{Colors.ENDC}\n")
    print("Passos para você:")
    print("1. Abra a caixa de entrada (ou pasta de Spam/Lixo) do seu e-mail;")
    print("2. Localize o e-mail do AutoReparos e clique no botão/link 'Aprovar Orçamento';")
    print("3. Após visualizar a confirmação de aprovação no navegador, volte aqui!")
    print(f"{Colors.WARNING}{Colors.BOLD}{'=' * 78}{Colors.ENDC}\n")

    if interactive:
        user_input = input("Pressione [ENTER] após clicar no link do e-mail (ou cole o link/token recebido caso prefira): ").strip()
        # Se o usuário colou uma URL ou token direto
        if user_input:
            token_param = user_input
            if "token=" in user_input:
                token_param = user_input.split("token=")[-1].split("&")[0]
            print_info(f"Processando token informado: {token_param[:20]}...")
            resp_token = requests.get(f"{base_url}/api/ordem-servico/aprovar?token={token_param}", timeout=10)
            if resp_token.status_code == 200:
                print_success("Aprovação processada com sucesso via token!")
            else:
                print_warning(f"Resposta do endpoint de aprovação: HTTP {resp_token.status_code}")

    # Valida se a OS está no status Aprovada / EmExecucao
    print_step("Consultando status atual da OS após aprovação...")
    max_retries = 5
    os_aprovada = False
    for attempt in range(max_retries):
        resp_check = requests.get(f"{base_url}/api/ordem-servico/{os_id}", headers=headers, timeout=10)
        if resp_check.status_code == 200:
            detalhes = resp_check.json()
            status = detalhes.get("status")
            print_info(f"Status atual da OS: {status}")
            # Status 4 = EmExecucao (após aprovar) ou string correspondente
            if str(status).lower() in ("emexecucao", "aprovada", "4", "3"):
                os_aprovada = True
                break
        time.sleep(2)

    if not os_aprovada:
        print_error("A OS ainda não consta como aprovada. Verifique se clicou no link do e-mail.")
        return False

    print_success("Confirmação de aprovação validada com sucesso! OS em execução.")

    # Obtém o ID do item de serviço da OS para iniciar e concluir
    resp_detalhes = requests.get(f"{base_url}/api/ordem-servico/{os_id}", headers=headers, timeout=10)
    detalhes = resp_detalhes.json()
    servicos_os = detalhes.get("servicos", [])
    if not servicos_os:
        print_error("Nenhum serviço listado na OS para execução.")
        return False

    os_servico_id = servicos_os[0].get("id")
    print_info(f"ID do item de serviço na OS: {os_servico_id}")

    # Caso TC-4.6: Iniciar e Concluir Serviço
    print_step("TC-4.6A: Iniciando execução do serviço na OS...")
    resp_init_serv = requests.patch(f"{base_url}/api/ordem-servico/{os_id}/servicos/{os_servico_id}/iniciar", headers=headers, timeout=10)
    if resp_init_serv.status_code not in (200, 204):
        print_error(f"Falha ao iniciar serviço. HTTP {resp_init_serv.status_code}: {resp_init_serv.text}")
        return False
    print_success("Serviço iniciado com sucesso.")

    print_step("TC-4.6B: Concluindo execução do serviço na OS...")
    resp_concluir_serv = requests.patch(f"{base_url}/api/ordem-servico/{os_id}/servicos/{os_servico_id}/concluir", headers=headers, timeout=10)
    if resp_concluir_serv.status_code not in (200, 204):
        print_error(f"Falha ao concluir serviço. HTTP {resp_concluir_serv.status_code}: {resp_concluir_serv.text}")
        return False
    print_success("Serviço concluído com sucesso (OS finalizada).")

    # Caso TC-4.7: Entregar Veículo e Validar Débito de Estoque
    print_step("TC-4.7A: Registrando entrega do veículo ao cliente...")
    resp_entregar = requests.patch(f"{base_url}/api/ordem-servico/{os_id}/entregar", headers=headers, timeout=10)
    if resp_entregar.status_code not in (200, 204):
        print_error(f"Falha ao entregar OS. HTTP {resp_entregar.status_code}: {resp_entregar.text}")
        return False
    print_success("Veículo entregue com sucesso! OS finalizada e entregue.")

    print_step("TC-4.7B: Validando débito físico de estoque do insumo...")
    resp_check_insumo = requests.get(f"{base_url}/api/insumos/{insumo_id}", headers=headers, timeout=10)
    if resp_check_insumo.status_code != 200:
        print_error(f"Falha ao consultar insumo. HTTP {resp_check_insumo.status_code}")
        return False

    insumo_atualizado = resp_check_insumo.json()
    estoque_atual = insumo_atualizado.get("quantidadeEstoque")
    print_info(f"Estoque inicial: 10 | Quantidade utilizada na OS: 4 | Estoque atual no banco: {estoque_atual}")

    if estoque_atual == 6:
        print_success("DÉBITO DE ESTOQUE CONFIRMADO: Estoque remanescente é exatamente 6 unidades!")
    else:
        print_error(f"Estoque remanescente inconsistente! Esperado: 6, Obtido: {estoque_atual}")
        return False

    # Salva no estado
    sm.update_section("tc04_ordem_servico", {
        "os_id": os_id,
        "status_final": "Entregue",
        "insumo_id": insumo_id,
        "estoque_remanescente": estoque_atual,
        "status": "PASSED"
    })
    print_success("Subfase TC-04 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc04()
    sys.exit(0 if success else 1)
