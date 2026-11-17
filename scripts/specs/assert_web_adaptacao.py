#!/usr/bin/env python3
"""Provas automatizadas da feature fase4/web-adaptacao (C2, C18, C19, C20)."""

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WEB = ROOT / "submodules" / "AutoReparos.Web"
BASE_COMMIT = "6975b7f"


def run(*args, cwd=ROOT, capture=True, env=None):
    result = subprocess.run(
        [str(arg) for arg in args],
        cwd=str(cwd),
        stdout=subprocess.PIPE if capture else None,
        stderr=subprocess.PIPE if capture else None,
        env=env,
        check=False,
    )
    if result.returncode != 0:
        detail = ((result.stderr or b"") + b"\n" + (result.stdout or b"")).decode("utf-8", "replace").strip()
        raise AssertionError(f"Comando falhou ({result.returncode}): {detail}")
    return (result.stdout or b"").decode("utf-8", "replace")


def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)


def _parse_nginx_locations(nginx_text: str) -> dict[str, str]:
    pattern = re.compile(r"location\s+([^{]+?)\s*\{([^}]*)\}", re.S)
    return {m.group(1).strip(): m.group(2).strip() for m in pattern.finditer(nginx_text)}


def check_proxy() -> None:
    nginx_path = WEB / "nginx.conf"
    entrypoint_path = WEB / "docker-entrypoint.sh"
    require(nginx_path.is_file(), "nginx.conf não encontrado")
    require(entrypoint_path.is_file(), "docker-entrypoint.sh não encontrado")

    nginx = nginx_path.read_text(encoding="utf-8")
    entrypoint = entrypoint_path.read_text(encoding="utf-8")

    # 1. Variáveis padrão de upstream no nginx.conf
    defaults = {
        "$os_url": "http://autoreparos-os:8080",
        "$cobranca_url": "http://autoreparos-cobranca:8080",
        "$execucao_url": "http://autoreparos-execucao:8080",
        "$cadastros_url": "http://autoreparos-cadastros:8080",
    }
    for var_name, default_url in defaults.items():
        require(
            re.search(rf'set\s+{re.escape(var_name)}\s+"{re.escape(default_url)}"\s*;', nginx) is not None,
            f"Variável {var_name} com padrão {default_url} ausente no nginx.conf",
        )

    # 2. Grupos de prefixos encaminhados ao serviço correspondente
    locations = _parse_nginx_locations(nginx)
    expected_prefixes = {
        "/api/v1/ordens-servico": "$os_url",
        "/api/v1/admin/sagas": "$os_url",
        "/api/v1/orcamentos": "$cobranca_url",
        "/api/v1/pagamentos": "$cobranca_url",
        "/api/v1/execucoes": "$execucao_url",
        "/api/v1/fila-execucao": "$execucao_url",
        "/api/v1/servicos": "$execucao_url",
        "/api/v1/insumos": "$execucao_url",
        "/api/v1/clientes": "$cadastros_url",
        "/api/v1/veiculos": "$cadastros_url",
        "/api/v1/usuarios": "$cadastros_url",
        "/api/auth": "$cadastros_url",
    }
    for prefix, target_var in expected_prefixes.items():
        body = locations.get(prefix)
        require(body is not None, f"location {prefix} ausente no nginx.conf")
        require(
            re.search(rf"proxy_pass\s+{re.escape(target_var)}\b", body) is not None,
            f"location {prefix} não encaminha para {target_var}",
        )

    # 3. Retorno 404 para prefixos desconhecidos em /api/v1/
    unknown_v1 = locations.get("/api/v1/")
    require(unknown_v1 is not None, "location /api/v1/ para fallback 404 ausente no nginx.conf")
    require(
        re.search(r"return\s+404\s*;", unknown_v1) is not None,
        "location /api/v1/ não retorna 404",
    )

    # 4. Preservação de /, /health e /v1/traces
    health = locations.get("= /health")
    require(health is not None and "return 200" in health, "location = /health (200) ausente no nginx.conf")
    root_loc = locations.get("/")
    require(
        root_loc is not None and "try_files $uri $uri/ /index.html;" in root_loc,
        "location / com try_files para SPA ausente no nginx.conf",
    )
    traces_loc = locations.get("/v1/traces")
    require(traces_loc is not None and "proxy_pass" in traces_loc, "location /v1/traces ausente no nginx.conf")

    # 5. Substituição de variáveis no docker-entrypoint.sh
    for env_var in ("OS_URL", "COBRANCA_URL", "EXECUCAO_URL", "CADASTROS_URL", "API_URL"):
        require(env_var in entrypoint, f"{env_var} ausente no docker-entrypoint.sh")

    with tempfile.TemporaryDirectory() as tmpdir:
        tmp = Path(tmpdir)
        conf_file = tmp / "default.conf"
        env_js_file = tmp / "env.js"
        conf_file.write_text(nginx, encoding="utf-8")
        env_js_file.write_text("", encoding="utf-8")

        script = entrypoint.replace("/etc/nginx/conf.d/default.conf", str(conf_file)).replace(
            "/usr/share/nginx/html/env.js", str(env_js_file)
        )
        script_file = tmp / "entrypoint.sh"
        script_file.write_text(script, encoding="utf-8")

        env = os.environ.copy()
        env.update(
            {
                "OS_URL": "http://os-custom:9001/",
                "COBRANCA_URL": "http://cobranca-custom:9002",
                "EXECUCAO_URL": "http://execucao-custom:9003",
                "CADASTROS_URL": "http://cadastros-custom:9004",
                "API_URL": "https://api.autoreparos.local",
            }
        )
        run("sh", script_file, "true", env=env)
        rendered = conf_file.read_text(encoding="utf-8")
        require('set $os_url "http://os-custom:9001";' in rendered, "OS_URL não foi aplicada ao default.conf")
        require('set $cobranca_url "http://cobranca-custom:9002";' in rendered, "COBRANCA_URL não foi aplicada ao default.conf")
        require('set $execucao_url "http://execucao-custom:9003";' in rendered, "EXECUCAO_URL não foi aplicada ao default.conf")
        require('set $cadastros_url "http://cadastros-custom:9004";' in rendered, "CADASTROS_URL não foi aplicada ao default.conf")
        require("window.__env.apiUrl" in env_js_file.read_text(encoding="utf-8"), "env.js não foi gerado com API_URL")

    print("PASS proxy: 4 grupos de prefixos, 404 em /api/v1/ desconhecido, / e /health 200")


def check_legado() -> None:
    src_dir = WEB / "src"
    require(src_dir.is_dir(), "Diretório src não encontrado")

    # 1. Nenhum arquivo ou pasta de dashboard em src/
    dashboard_files = [p for p in src_dir.rglob("*") if "dashboard" in p.name.lower()]
    require(
        not dashboard_files,
        f"Arquivos/pastas de dashboard ainda existem: {[str(p.relative_to(WEB)) for p in dashboard_files]}",
    )

    # 2. Varredura dos arquivos de código (excluindo specs) para os 5 itens removidos
    prod_files = [
        p for p in src_dir.rglob("*")
        if p.is_file() and p.suffix in {".ts", ".html"} and not p.name.endswith(".spec.ts")
    ]

    proibidos = [
        ("dashboard", re.compile(r"(/dashboard\b|path:\s*['\"]dashboard['\"]|DASHBOARD\b)", re.I)),
        ("entregar", re.compile(r"(entregarVeiculo|/entregar\b)", re.I)),
        ("status manual", re.compile(r"(/ordens-servico/[^'\"\n]*/status\b|alterarStatus|atualizarStatus)", re.I)),
        (
            "itens soltos da OS",
            re.compile(r"(/(?:ordens-servico|ordem-servico)/[^'\"\n]*/(?:servicos|insumos))", re.I),
        ),
        (
            "busca por placa/CPF",
            re.compile(r"(consultarPorPlaca|consultarPorCpf|[?&](placa|cpf)=)", re.I),
        ),
    ]

    violacoes = []
    for path in prod_files:
        text = path.read_text(encoding="utf-8")
        for nome_item, regex in proibidos:
            match = regex.search(text)
            if match:
                violacoes.append(f"{path.relative_to(WEB)} ({nome_item}: {match.group(0)})")

    require(not violacoes, "Referências legadas encontradas: " + "; ".join(violacoes))
    print("PASS legado: nenhum vestígio de dashboard, entregar, status manual, itens soltos da OS ou busca por placa/CPF")


def _parse_lcov(lcov_path: Path) -> dict[str, tuple[int, int]]:
    coverage: dict[str, tuple[int, int]] = {}
    current_file: str | None = None
    lf = 0
    lh = 0
    for raw_line in lcov_path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if line.startswith("SF:"):
            sf = line[3:].strip().replace("\\", "/")
            if "src/" in sf:
                sf = "src/" + sf.split("src/", 1)[1]
            current_file = sf
            lf = 0
            lh = 0
        elif line.startswith("LF:"):
            lf = int(line[3:])
        elif line.startswith("LH:"):
            lh = int(line[3:])
        elif line == "end_of_record" and current_file:
            prev_lh, prev_lf = coverage.get(current_file, (0, 0))
            coverage[current_file] = (prev_lh + lh, prev_lf + lf)
            current_file = None
    return coverage


def check_cobertura() -> None:
    run(
        "npx",
        "--offline",
        "yarn@1.22.22",
        "--cwd",
        str(WEB),
        "test",
        "--watch=false",
        "--coverage",
        capture=False,
    )
    run(
        "npx",
        "--offline",
        "yarn@1.22.22",
        "--cwd",
        str(WEB),
        "build",
        capture=False,
    )

    index_html = WEB / "dist" / "app-temp" / "browser" / "index.html"
    require(index_html.is_file(), "Build não gerou dist/app-temp/browser/index.html")

    lcov_path = WEB / "coverage" / "app-temp" / "lcov.info"
    if not lcov_path.is_file():
        lcov_path = WEB / "coverage" / "lcov.info"
    require(lcov_path.is_file(), "Arquivo lcov.info não foi gerado em coverage/")
    coverage = _parse_lcov(lcov_path)

    diff_out = run("git", "-C", str(WEB), "diff", "--name-only", "--diff-filter=ACMR", BASE_COMMIT)
    untracked_out = run("git", "-C", str(WEB), "ls-files", "--others", "--exclude-standard")
    changed_files = sorted(
        {
            f.strip()
            for f in (diff_out.splitlines() + untracked_out.splitlines())
            if f.strip().startswith("src/")
            and f.strip().endswith(".ts")
            and not f.strip().endswith(".spec.ts")
        }
    )
    require(bool(changed_files), "Nenhum arquivo novo ou alterado encontrado em relação à base")

    total_lh = 0
    total_lf = 0
    measured = []
    for rel in changed_files:
        if rel in coverage:
            lh, lf = coverage[rel]
            if lf > 0:
                total_lh += lh
                total_lf += lf
                pct = (lh * 100.0) / lf
                measured.append((rel, lh, lf, pct))

    require(total_lf > 0, "Nenhuma linha instrumentada encontrada no lcov.info para os arquivos alterados")
    overall_pct = (total_lh * 100.0) / total_lf
    require(
        overall_pct >= 80.0,
        f"Cobertura de linhas nos arquivos novos/alterados ({overall_pct:.2f}%) ficou abaixo de 80%",
    )
    print(
        f"PASS cobertura: {overall_pct:.2f}% de linhas ({total_lh}/{total_lf}) em {len(measured)} arquivos novos/alterados e build OK"
    )


def check_node() -> None:
    setup_file = WEB / "src" / "test-setup.ts"
    angular_json_path = WEB / "angular.json"
    package_json_path = WEB / "package.json"

    require(setup_file.is_file(), "src/test-setup.ts não encontrado")
    setup_text = setup_file.read_text(encoding="utf-8")
    require("localStorage" in setup_text and "sessionStorage" in setup_text, "src/test-setup.ts não configura localStorage/sessionStorage")

    angular_cfg = json.loads(angular_json_path.read_text(encoding="utf-8"))
    setup_files = (
        angular_cfg.get("projects", {})
        .get("app-temp", {})
        .get("architect", {})
        .get("test", {})
        .get("options", {})
        .get("setupFiles", [])
    )
    require("src/test-setup.ts" in setup_files, "src/test-setup.ts não está registrado em angular.json test.options.setupFiles")

    pkg = json.loads(package_json_path.read_text(encoding="utf-8"))
    test_script = pkg.get("scripts", {}).get("test", "")
    require(
        "--localstorage-file" not in test_script and "--no-experimental-webstorage" not in test_script,
        f"scripts.test em package.json não deve exigir flags extras de Node: {test_script}",
    )

    run(
        "npx",
        "--offline",
        "yarn@1.22.22",
        "--cwd",
        str(WEB),
        "test",
        "--watch=false",
        "--include",
        "src/app/app.spec.ts",
    )
    print("PASS node: src/test-setup.ts configurado no angular.json e suíte executa sem flags extras")


CHECKS = {
    "proxy": check_proxy,
    "legado": check_legado,
    "cobertura": check_cobertura,
    "node": check_node,
}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", required=True, choices=sorted(CHECKS.keys()))
    args = parser.parse_args()
    try:
        CHECKS[args.check]()
    except (AssertionError, OSError) as exc:
        print(f"FAIL {args.check}: {exc}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
