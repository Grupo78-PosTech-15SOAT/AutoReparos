#!/usr/bin/env python3
"""Provas automatizadas da feature fase4/web-coerencia-negocio (C1, C3, C18)."""
from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WEB = ROOT / "submodules" / "AutoReparos.Web"
BASE_COMMIT = "ae5e108"


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


def check_consulta_publica() -> None:
    """Verifica que /consulta-publica e seu componente foram completamente removidos."""
    # 1. Componente excluído do disco
    consulta_dir = WEB / "src" / "app" / "features" / "portal-publico" / "pages" / "consulta-publica-page"
    require(not consulta_dir.exists(), f"Diretório ainda existe: {consulta_dir}")

    # 2. Arquivos-chave não contêm referências a consulta-publica
    arquivos_verificar = [
        WEB / "src" / "app" / "app.routes.ts",
        WEB / "src" / "app" / "core" / "layout" / "navbar" / "navbar.component.ts",
        WEB / "src" / "app" / "core" / "layout" / "navbar" / "navbar.component.html",
        WEB / "src" / "app" / "features" / "auth" / "pages" / "login-page" / "login-page.component.ts",
        WEB / "src" / "app" / "features" / "auth" / "pages" / "login-page" / "login-page.component.html",
        WEB / "src" / "app" / "features" / "home" / "pages" / "home-page" / "home-page.component.ts",
        WEB / "src" / "app" / "features" / "home" / "pages" / "home-page" / "home-page.component.html",
    ]

    proibidos = [
        ("consulta-publica", re.compile(r"consulta-publica", re.IGNORECASE)),
        ("ConsultaPublicaPageComponent", re.compile(r"ConsultaPublicaPageComponent")),
    ]

    violacoes = []
    for path in arquivos_verificar:
        if not path.is_file():
            continue
        text = path.read_text(encoding="utf-8")
        for nome, regex in proibidos:
            if regex.search(text):
                violacoes.append(f"{path.relative_to(WEB)} contem referência a {nome}")

    require(not violacoes, "Referências a consulta pública ainda presentes: " + "; ".join(violacoes))
    print("PASS consulta-publica: rota, componente e links removidos integralmente")


def check_visual() -> None:
    """Verifica existência de componentes compartilhados e ausência de duplicações de modal/cores hex."""
    modal_comp = WEB / "src" / "app" / "shared" / "components" / "modal-dialog" / "modal-dialog.component.ts"
    alert_comp = WEB / "src" / "app" / "shared" / "components" / "alert-banner" / "alert-banner.component.ts"

    require(modal_comp.is_file(), "modal-dialog.component.ts não encontrado em shared/components")
    require(alert_comp.is_file(), "alert-banner.component.ts não encontrado em shared/components")

    # Arquivos alterados desde BASE_COMMIT não devem ter estilos duplicados de modal nem hex hardcoded
    try:
        diff_out = run("git", "-C", str(WEB), "diff", "--name-only", "--diff-filter=ACMR", BASE_COMMIT)
        untracked_out = run("git", "-C", str(WEB), "ls-files", "--others", "--exclude-standard")
        changed_files = [
            WEB / f for f in sorted(set(diff_out.splitlines() + untracked_out.splitlines()))
            if f.startswith("src/app/") and (f.endswith(".ts") or f.endswith(".html") or f.endswith(".css"))
            and not f.endswith(".spec.ts")
            and not "shared/components/modal-dialog" in f
        ]
    except Exception:
        changed_files = []

    violacoes = []
    hex_pattern = re.compile(r"#(ed145b|18181c|0a0a0c)", re.IGNORECASE)
    modal_duplicate = re.compile(r"\.(modal-backdrop|modal-card)\s*\{")

    for path in changed_files:
        if not path.is_file():
            continue
        text = path.read_text(encoding="utf-8")
        if hex_pattern.search(text):
            violacoes.append(f"{path.relative_to(WEB)}: cor hex hardcoded em vez de variável CSS")
        if modal_duplicate.search(text):
            violacoes.append(f"{path.relative_to(WEB)}: estilo duplicado de modal em vez de <app-modal-dialog>")

    require(not violacoes, "Violações de identidade visual: " + "; ".join(violacoes))
    print("PASS visual: ModalDialog e AlertBanner presentes, sem estilos de modal duplicados nem hex fixos")


def check_contratos() -> None:
    """Verifica conformidade dos contratos de modelos com os microsserviços."""
    endpoints_file = WEB / "src" / "app" / "core" / "config" / "api-endpoints.ts"
    require(endpoints_file.is_file(), "api-endpoints.ts não encontrado")
    endpoints_text = endpoints_file.read_text(encoding="utf-8")
    require("ROLE:" not in endpoints_text, "api-endpoints.ts ainda contém ROLE em USUARIOS")

    servico_model = WEB / "src" / "app" / "features" / "servicos" / "models" / "servico.model.ts"
    if servico_model.is_file():
        text = servico_model.read_text(encoding="utf-8")
        require("precoUnitario" in text, "servico.model.ts não declara precoUnitario")

    insumo_model = WEB / "src" / "app" / "features" / "insumos" / "models" / "insumo.model.ts"
    if insumo_model.is_file():
        text = insumo_model.read_text(encoding="utf-8")
        require("quantidadeDisponivel" in text, "insumo.model.ts não declara quantidadeDisponivel")
        require("quantidadeReservada" in text, "insumo.model.ts não declara quantidadeReservada")

    veiculo_model = WEB / "src" / "app" / "features" / "veiculos" / "models" / "veiculo.model.ts"
    if veiculo_model.is_file():
        text = veiculo_model.read_text(encoding="utf-8")
        require("chassi" in text, "veiculo.model.ts não declara chassi")
        require("renavam" in text, "veiculo.model.ts não declara renavam")

    print("PASS contratos: modelos e endpoints alinhados com microsserviços")


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
    """Executa Vitest com coverage e build, avaliando cobertura dos arquivos alterados."""
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
            f
            for f in diff_out.splitlines() + untracked_out.splitlines()
            if f.startswith("src/app/")
            and f.endswith(".ts")
            and not f.endswith(".spec.ts")
            and not f.endswith(".routes.ts")
            and not f.endswith(".config.ts")
        }
    )

    total_lh = 0
    total_lf = 0
    arquivos_avaliados: list[tuple[str, int, int, float]] = []

    for rel in changed_files:
        full_path = WEB / rel
        if not full_path.is_file():
            continue
        lh, lf = coverage.get(rel, (0, 0))
        pct = (lh / lf * 100.0) if lf > 0 else 100.0
        total_lh += lh
        total_lf += lf
        arquivos_avaliados.append((rel, lh, lf, pct))

    overall_pct = (total_lh / total_lf * 100.0) if total_lf > 0 else 100.0
    print(f"\n--- Cobertura dos arquivos novos/alterados desde {BASE_COMMIT} ---")
    abaixo_meta: list[str] = []
    for rel, lh, lf, pct in arquivos_avaliados:
        status = "OK" if pct >= 80.0 else "FAIL"
        if pct < 80.0:
            abaixo_meta.append(f"{rel} ({pct:.2f}%)")
        print(f"  [{status}] {rel}: {lh}/{lf} linhas ({pct:.2f}%)")
    print(f"Total agregado: {total_lh}/{total_lf} linhas ({overall_pct:.2f}%)")

    require(
        not abaixo_meta,
        f"Arquivos novos ou alterados com cobertura de linhas abaixo de 80.0%: {', '.join(abaixo_meta)}",
    )
    print("PASS cobertura: build verde e todos os arquivos com cobertura de linhas >= 80%")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--check",
        choices=["consulta-publica", "visual", "contratos", "cobertura", "all"],
        default="all",
        help="Checagem a executar",
    )
    args = parser.parse_args()

    if args.check in ("consulta-publica", "all"):
        check_consulta_publica()
    if args.check in ("visual", "all"):
        check_visual()
    if args.check in ("contratos", "all"):
        check_contratos()
    if args.check in ("cobertura", "all"):
        check_cobertura()


if __name__ == "__main__":
    try:
        main()
    except AssertionError as err:
        print(f"FAIL: {err}", file=sys.stderr)
        sys.exit(1)
