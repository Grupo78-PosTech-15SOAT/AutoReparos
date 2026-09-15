#!/usr/bin/env python3
"""
TC-10: Consolidação e Relatório de Links da Aplicação (Docker & AWS)
Reúne e apresenta de forma destacada todos os links da aplicação:
- Frontend Angular Web (Entrada do Usuário);
- Backend Swagger / OpenAPI;
- AWS API Gateway HTTP API v2;
- Painel New Relic One (APM & Dashboards);
- Grafana, Jaeger e Prometheus.
Salva a lista na seção 'aws_links' de test_state.json.
"""

import sys
from pathlib import Path

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info, Colors


def run_tc10() -> bool:
    sm = StateManager()
    aws_infra = sm.get_section("aws_infra")
    aws_deploy = sm.get_section("aws_deploy")

    api_gw_endpoint = aws_infra.get("api_gateway_endpoint", "http://localhost:8080")
    ingress_hostname = aws_deploy.get("ingress_hostname")

    web_local = sm.get_config("web_url_local", "http://localhost:4200")
    grafana_local = sm.get_config("grafana_url_local", "http://localhost:3000")
    jaeger_local = sm.get_config("jaeger_url_local", "http://localhost:16686")
    prom_local = sm.get_config("prometheus_url_local", "http://localhost:9090")

    # Links Finais
    frontend_entrance = f"http://{ingress_hostname}" if (ingress_hostname and ingress_hostname != "pending") else web_local
    backend_swagger = f"{api_gw_endpoint}/swagger/index.html"
    new_relic_portal = "https://one.newrelic.com"

    links = {
        "frontend_web_entrance": frontend_entrance,
        "backend_swagger_ui": backend_swagger,
        "api_gateway_endpoint": api_gw_endpoint,
        "new_relic_one": new_relic_portal,
        "grafana_dashboards": grafana_local,
        "jaeger_tracing": jaeger_local,
        "prometheus_metrics": prom_local
    }

    print_header("TC-10: Relatório Consolidado de Links e Acessos")
    print(f"\n{Colors.GREEN}{Colors.BOLD}{'=' * 78}{Colors.ENDC}")
    print(f"{Colors.GREEN}{Colors.BOLD}🌐 LINKS OFICIAIS DO SISTEMA AUTOREPAROS{Colors.ENDC}")
    print(f"{Colors.GREEN}{Colors.BOLD}{'=' * 78}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}Frontend Web (Entrada da Aplicação):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{frontend_entrance}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}Backend Swagger (Documentação & Testes Interativos):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{backend_swagger}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}AWS API Gateway v2 (Ponto de Entrada Nuvem):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{api_gw_endpoint}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}New Relic One (Observabilidade & APM em Nuvem):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{new_relic_portal}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}Grafana (Dashboards Mandatórios):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{grafana_local}{Colors.ENDC} (user: admin / pass: admin)\n")

    print(f"🔹 {Colors.BOLD}Jaeger (Distributed Tracing):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{jaeger_local}{Colors.ENDC}\n")

    print(f"🔹 {Colors.BOLD}Prometheus (Métricas em Tempo Real):{Colors.ENDC}")
    print(f"   👉 {Colors.CYAN}{prom_local}{Colors.ENDC}\n")

    print(f"{Colors.GREEN}{Colors.BOLD}{'=' * 78}{Colors.ENDC}\n")

    sm.update_section("aws_links", {
        "links": links,
        "status": "GENERATED"
    })
    return True


if __name__ == "__main__":
    success = run_tc10()
    sys.exit(0 if success else 1)
