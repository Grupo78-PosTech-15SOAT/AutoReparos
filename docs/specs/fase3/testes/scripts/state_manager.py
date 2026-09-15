#!/usr/bin/env python3
"""
State Manager para Testes Integrados AutoReparos (Docker Compose & Nuvem AWS)
Centraliza a leitura/gravação do arquivo test_state.json e parsing do .env raiz.
"""

import json
import os
import sys
from pathlib import Path
from typing import Any, Dict

# Diretório base do script e do repositório
SCRIPTS_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPTS_DIR.parents[4]  # Sobe docs/specs/fase3/testes/scripts -> raiz
STATE_FILE_PATH = SCRIPTS_DIR / "test_state.json"
ENV_FILE_PATH = REPO_ROOT / ".env"

# Cores ANSI para saída no terminal
class Colors:
    HEADER = "\033[95m"
    BLUE = "\033[94m"
    CYAN = "\033[96m"
    GREEN = "\033[92m"
    WARNING = "\033[93m"
    FAIL = "\033[91m"
    ENDC = "\033[0m"
    BOLD = "\033[1m"
    UNDERLINE = "\033[4m"


def print_header(title: str) -> None:
    print(f"\n{Colors.HEADER}{Colors.BOLD}{'=' * 78}{Colors.ENDC}")
    print(f"{Colors.HEADER}{Colors.BOLD}>>> {title}{Colors.ENDC}")
    print(f"{Colors.HEADER}{Colors.BOLD}{'=' * 78}{Colors.ENDC}")


def print_step(msg: str) -> None:
    print(f"{Colors.CYAN}[PASSO] {msg}{Colors.ENDC}")


def print_success(msg: str) -> None:
    print(f"{Colors.GREEN}✔ [SUCESSO] {msg}{Colors.ENDC}")


def print_error(msg: str) -> None:
    print(f"{Colors.FAIL}✖ [ERRO] {msg}{Colors.ENDC}")


def print_warning(msg: str) -> None:
    print(f"{Colors.WARNING}⚠ [ATENÇÃO] {msg}{Colors.ENDC}")


def print_info(msg: str) -> None:
    print(f"{Colors.BLUE}ℹ [INFO] {msg}{Colors.ENDC}")


def load_env() -> Dict[str, str]:
    """Carrega variáveis do arquivo .env raiz de forma robusta."""
    env_vars: Dict[str, str] = {}
    if not ENV_FILE_PATH.exists():
        print_warning(f"Arquivo .env não encontrado em: {ENV_FILE_PATH}")
        return env_vars

    with open(ENV_FILE_PATH, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            if "=" in line:
                key, val = line.split("=", 1)
                env_vars[key.strip()] = val.strip().strip('"').strip("'")
    return env_vars


class StateManager:
    """Gerencia o estado consolidado dos testes em test_state.json."""

    def __init__(self, state_file: Path = STATE_FILE_PATH):
        self.state_file = state_file
        self.env = load_env()
        self._ensure_initialized()

    def _ensure_initialized(self) -> None:
        """Inicializa o test_state.json se ainda não existir."""
        if not self.state_file.exists():
            initial_state: Dict[str, Any] = {
                "config": {
                    "base_url_local": "http://localhost:8080",
                    "web_url_local": "http://localhost:4200",
                    "swagger_url_local": "http://localhost:8080/swagger/index.html",
                    "grafana_url_local": "http://localhost:3000",
                    "jaeger_url_local": "http://localhost:16686",
                    "prometheus_url_local": "http://localhost:9090",
                    "test_cliente_email": self.env.get("TEST_CLIENTE_EMAIL", "josehenriquedotta61@gmail.com"),
                    "seed_user_email": self.env.get("SEED_USER_EMAIL", "admin@autoreparos.com"),
                    "seed_user_password": self.env.get("SEED_USER_PASSWORD", ""),
                    "new_relic_license_key": self.env.get("NEW_RELIC_LICENSE_KEY", ""),
                    "aws_region": self.env.get("AWS_REGION", "us-east-1"),
                    "aws_account_id": "683444362184",
                }
            }
            self.save_full_state(initial_state)

    def load_full_state(self) -> Dict[str, Any]:
        """Lê o estado completo do arquivo JSON."""
        if not self.state_file.exists():
            self._ensure_initialized()
        try:
            with open(self.state_file, "r", encoding="utf-8") as f:
                return json.load(f)
        except Exception as e:
            print_error(f"Falha ao ler {self.state_file}: {e}")
            return {}

    def save_full_state(self, state: Dict[str, Any]) -> None:
        """Grava o estado completo no arquivo JSON."""
        with open(self.state_file, "w", encoding="utf-8") as f:
            json.dump(state, f, indent=2, ensure_ascii=False)

    def get_section(self, section_name: str) -> Dict[str, Any]:
        """Obtém a seção de dados específica de uma subfase."""
        state = self.load_full_state()
        return state.get(section_name, {})

    def update_section(self, section_name: str, data: Dict[str, Any]) -> None:
        """Atualiza a seção de dados específica de uma subfase e persiste."""
        state = self.load_full_state()
        current_section = state.get(section_name, {})
        current_section.update(data)
        state[section_name] = current_section
        self.save_full_state(state)
        print_info(f"Estado atualizado para subfase [{section_name}] em {self.state_file.name}")

    def get_config(self, key: str, default: Any = None) -> Any:
        """Recupera valor de configuração global ou do .env."""
        config = self.get_section("config")
        if key in config:
            return config[key]
        return self.env.get(key, default)


if __name__ == "__main__":
    sm = StateManager()
    print_header("State Manager Inicializado")
    print_info(f"Arquivo de estado: {sm.state_file}")
    print(json.dumps(sm.load_full_state(), indent=2, ensure_ascii=False))
