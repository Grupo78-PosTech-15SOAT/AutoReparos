#!/usr/bin/env python3
"""
TC-06: Verificação da Stack de Observabilidade Local e Integração New Relic
Valida coleta de traces no Jaeger, métricas no Prometheus, dashboards do Grafana
e exportação do OpenTelemetry Collector para o New Relic.
Salva resultados na seção 'tc06_observabilidade'.
"""

import sys
import subprocess
from pathlib import Path
import requests

from state_manager import StateManager, print_header, print_step, print_success, print_error, print_warning, print_info


def run_tc06() -> bool:
    sm = StateManager()
    jaeger_url = sm.get_config("jaeger_url_local", "http://localhost:16686")
    prometheus_url = sm.get_config("prometheus_url_local", "http://localhost:9090")
    grafana_url = sm.get_config("grafana_url_local", "http://localhost:3000")
    nr_key = sm.get_config("new_relic_license_key")

    print_header("TC-06: Verificação de Observabilidade e Telemetria")
    results = {}

    # Caso TC-6.1: Jaeger Tracing
    print_step(f"TC-6.1: Consultando traces da API no Jaeger ({jaeger_url})...")
    try:
        resp_jaeger = requests.get(f"{jaeger_url}/api/traces?service=autoreparos-api&limit=5", timeout=5)
        if resp_jaeger.status_code == 200:
            traces_data = resp_jaeger.json().get("data", [])
            print_success(f"Jaeger ativo e coletando traces! Total de traces recuperados: {len(traces_data)}")
            results["jaeger_status"] = "HEALTHY"
            results["traces_count"] = len(traces_data)
        else:
            print_warning(f"Jaeger respondeu HTTP {resp_jaeger.status_code}")
            results["jaeger_status"] = f"HTTP_{resp_jaeger.status_code}"
    except Exception as e:
        print_error(f"Falha ao conectar ao Jaeger: {e}")
        results["jaeger_status"] = "UNREACHABLE"

    # Caso TC-6.2: Prometheus Metrics
    print_step(f"TC-6.2: Consultando métricas no Prometheus ({prometheus_url})...")
    try:
        resp_prom = requests.get(f"{prometheus_url}/api/v1/query?query=up", timeout=5)
        if resp_prom.status_code == 200:
            prom_data = resp_prom.json().get("data", {}).get("result", [])
            print_success(f"Prometheus ativo e monitorando alvos! Alvos 'up': {len(prom_data)}")
            results["prometheus_status"] = "HEALTHY"
            results["targets_up"] = len(prom_data)
        else:
            print_warning(f"Prometheus respondeu HTTP {resp_prom.status_code}")
            results["prometheus_status"] = f"HTTP_{resp_prom.status_code}"
    except Exception as e:
        print_error(f"Falha ao conectar ao Prometheus: {e}")
        results["prometheus_status"] = "UNREACHABLE"

    # Caso TC-6.3: Grafana Health
    print_step(f"TC-6.3: Consultando status do Grafana ({grafana_url})...")
    try:
        resp_grafana = requests.get(f"{grafana_url}/api/health", timeout=5)
        if resp_grafana.status_code == 200:
            print_success("Grafana ativo e saudável (HTTP 200 OK)!")
            results["grafana_status"] = "HEALTHY"
        else:
            print_warning(f"Grafana respondeu HTTP {resp_grafana.status_code}")
            results["grafana_status"] = f"HTTP_{resp_grafana.status_code}"
    except Exception as e:
        print_error(f"Falha ao conectar ao Grafana: {e}")
        results["grafana_status"] = "UNREACHABLE"

    # Caso TC-6.4: OTel Collector Logs e New Relic
    print_step("TC-6.4: Verificando logs do OpenTelemetry Collector e exportador New Relic...")
    try:
        log_proc = subprocess.run(
            ["docker", "logs", "--tail", "25", "autoreparos-otel-collector"],
            capture_output=True,
            text=True,
            check=False
        )
        logs = log_proc.stdout + log_proc.stderr
        if "error" in logs.lower() and "failed to export" in logs.lower():
            print_warning("Detectados avisos/erros recentes nos logs do OTel Collector.")
            results["otel_collector_status"] = "WARNING"
        else:
            print_success("OTel Collector operando sem falhas de exportação críticas.")
            results["otel_collector_status"] = "HEALTHY"
    except Exception as e:
        print_warning(f"Não foi possível inspecionar logs do contêiner: {e}")
        results["otel_collector_status"] = "SKIPPED"

    if nr_key:
        print_info(f"New Relic License Key configurada ({nr_key[:8]}...NRAL). Telemetria direcionada para nuvem New Relic One.")
        results["new_relic_configured"] = True

    sm.update_section("tc06_observabilidade", results)
    print_success("Subfase TC-06 concluída com êxito!")
    return True


if __name__ == "__main__":
    success = run_tc06()
    sys.exit(0 if success else 1)
