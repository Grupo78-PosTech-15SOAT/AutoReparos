#!/usr/bin/env python3
"""
Orquestrador Central de Testes Integrados: AutoReparos (Docker & AWS)
Executa e orquestra as subfases através de chamadas modulares, consumindo e
persistindo dados compartilhados no arquivo único 'test_state.json'.

Uso:
  python3 run_all.py                  # Executa toda a Etapa 1 (Docker Compose)
  python3 run_all.py --phase docker   # Executa apenas a Etapa 1 (TC-01 a TC-06)
  python3 run_all.py --phase aws      # Executa apenas a Etapa 2 (TC-07 a TC-10)
  python3 run_all.py --step tc01      # Executa apenas um teste específico
  python3 run_all.py --status         # Exibe o status consolidado do test_state.json
"""

import argparse
import json
import sys
from pathlib import Path

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info, Colors

# Import das subfases
import tc01_auth_rbac
import tc02_cadastros_base
import tc03_catalogo_estoque
import tc04_ciclo_os_email
import tc05_testes_negativos
import tc06_observabilidade
import tc07_aws_infra_provision
import tc08_aws_deploy_k8s
import tc09_aws_smoke_tests
import tc10_aws_links_report

STEPS_MAP = {
    "tc01": ("TC-01: Autenticação & RBAC", tc01_auth_rbac.run_tc01),
    "tc02": ("TC-02: Cadastros Base & Value Objects", tc02_cadastros_base.run_tc02),
    "tc03": ("TC-03: Catálogo de Serviços & Insumos", tc03_catalogo_estoque.run_tc03),
    "tc04": ("TC-04: Ciclo E2E da OS & E-mail Real", tc04_ciclo_os_email.run_tc04),
    "tc05": ("TC-05: Testes Negativos & Invariantes", tc05_testes_negativos.run_tc05),
    "tc06": ("TC-06: Observabilidade Local & New Relic", tc06_observabilidade.run_tc06),
    "tc07": ("TC-07: Provisionamento IaC Terraform (AWS)", tc07_aws_infra_provision.run_tc07),
    "tc08": ("TC-08: Deploy no EKS (ECR & Helm)", tc08_aws_deploy_k8s.run_tc08),
    "tc09": ("TC-09: Smoke Tests na Nuvem AWS", tc09_aws_smoke_tests.run_tc09),
    "tc10": ("TC-10: Relatório de Links e Acessos", tc10_aws_links_report.run_tc10),
}


def show_status(sm: StateManager) -> None:
    print_header("Status Consolidado dos Testes (test_state.json)")
    state = sm.load_full_state()
    print(json.dumps(state, indent=2, ensure_ascii=False))


def run_phase(steps: list[str]) -> bool:
    total = len(steps)
    passed = 0

    for idx, step_key in enumerate(steps, start=1):
        name, func = STEPS_MAP[step_key]
        print(f"\n{Colors.BOLD}[Subfase {idx}/{total}] Iniciando {name}...{Colors.ENDC}")
        try:
            success = func()
            if not success:
                print_error(f"A subfase [{step_key}] falhou! Interrompendo execução.")
                return False
            passed += 1
        except Exception as e:
            print_error(f"Exceção não tratada na subfase [{step_key}]: {e}")
            import traceback
            traceback.print_exc()
            return False

    print(f"\n{Colors.GREEN}{Colors.BOLD}✔ Todas as {passed}/{total} subfases foram concluídas com sucesso!{Colors.ENDC}")
    return True


def main() -> None:
    parser = argparse.ArgumentParser(description="Orquestrador de Testes Integrados AutoReparos")
    parser.add_argument("--phase", choices=["docker", "aws", "all"], default="docker", help="Fase de testes a executar")
    parser.add_argument("--step", choices=list(STEPS_MAP.keys()), help="Executar apenas uma subfase específica")
    parser.add_argument("--status", action="store_true", help="Visualizar estado atual de test_state.json")

    args = parser.parse_args()
    sm = StateManager()

    if args.status:
        show_status(sm)
        return

    if args.step:
        name, func = STEPS_MAP[args.step]
        print_header(f"Executando subfase isolada: {name}")
        ok = func()
        sys.exit(0 if ok else 1)

    docker_steps = ["tc01", "tc02", "tc03", "tc04", "tc05", "tc06"]
    aws_steps = ["tc07", "tc08", "tc09", "tc10"]

    if args.phase == "docker":
        print_header("Iniciando ETAPA 1: Testes no Docker Compose Local")
        ok = run_phase(docker_steps)
        sys.exit(0 if ok else 1)
    elif args.phase == "aws":
        print_header("Iniciando ETAPA 2: Provisionamento e Testes na Nuvem AWS")
        ok = run_phase(aws_steps)
        sys.exit(0 if ok else 1)
    elif args.phase == "all":
        print_header("Iniciando Ciclo Completo (Docker Compose + Nuvem AWS)")
        ok = run_phase(docker_steps)
        if ok:
            print_header("Etapa 1 (Docker Compose) 100% aprovada. Iniciando Etapa 2 (AWS)...")
            ok = run_phase(aws_steps)
        sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
