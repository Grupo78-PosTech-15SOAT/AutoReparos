#!/usr/bin/env python3
"""Provas estáticas da feature fase4/web-ui-polimento (C8, C25, C26, C27)."""
from __future__ import annotations

import argparse
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
WEB = ROOT / "submodules" / "AutoReparos.Web"
APP = WEB / "src" / "app"
BASE_COMMIT = "43d595f"
COBERTURA_MINIMA = 80.0


def run(*args, cwd=ROOT, capture=True):
    result = subprocess.run(
        [str(a) for a in args],
        cwd=str(cwd),
        stdout=subprocess.PIPE if capture else None,
        stderr=subprocess.PIPE if capture else None,
        check=False,
    )
    if result.returncode != 0:
        detail = ((result.stderr or b"") + b"\n" + (result.stdout or b"")).decode("utf-8", "replace").strip()
        raise AssertionError(f"Comando falhou ({result.returncode}): {' '.join(map(str, args))}\n{detail[-4000:]}")
    return (result.stdout or b"").decode("utf-8", "replace")


def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)


def ler(caminho: Path) -> str:
    require(caminho.is_file(), f"Arquivo não encontrado: {caminho.relative_to(ROOT)}")
    return caminho.read_text(encoding="utf-8")


def bloco_css(css: str, seletor: str) -> str | None:
    """Corpo da primeira regra cujo seletor é exatamente `seletor`."""
    padrao = re.compile(r"(?:^|\})\s*" + re.escape(seletor) + r"\s*\{([^}]*)\}", re.MULTILINE)
    achado = padrao.search(css)
    return achado.group(1) if achado else None


def check_estilos() -> None:
    css = ler(WEB / "src" / "styles.css")
    for seletor in (".btn-ghost", ".btn-sm", ".btn-icon", ".filter-bar"):
        require(bloco_css(css, seletor) is not None, f"styles.css não define {seletor}")
    form_group = bloco_css(css, ".filter-bar .form-group")
    require(form_group is not None, "styles.css não define .filter-bar .form-group")
    require(re.search(r"margin-bottom\s*:\s*0\s*;", form_group or "") is not None,
            ".filter-bar .form-group precisa de margin-bottom: 0")
    for token in ("--accent-green", "--accent-red", "--accent-cyan", "--accent-yellow"):
        require(re.search(re.escape(token) + r"\s*:", css) is not None, f"styles.css não define o token {token}")
    card = bloco_css(css, ".card-panel")
    require(card is not None, "styles.css não define .card-panel")
    require("backdrop-filter" not in (card or ""), ".card-panel não pode usar backdrop-filter")
    print("PASS estilos: botões, filter-bar, tokens accent e card-panel sem backdrop-filter")


def check_home() -> None:
    home = ler(APP / "features" / "home" / "pages" / "home-page" / "home-page.component.ts")
    template = home.split("template:", 1)[1].split("styles:", 1)[0] if "template:" in home else home
    links = re.findall(r"<a\b[^>]*>(.*?)</a>", template, re.DOTALL)
    require(links, "Home sem links")
    com_kanban = [re.sub(r"\s+", " ", re.sub(r"<[^>]+>", " ", l)).strip() for l in links if "kanban" in l.lower()]
    require(not com_kanban, f"Links da Home ainda citam Kanban: {com_kanban}")
    destinos = re.findall(r'routerLink="(/ordens-servico[^"]*)"', template)
    require(destinos, "Home sem link para /ordens-servico")
    require(all(d == "/ordens-servico" for d in destinos),
            f"Links de OS da Home devem apontar para /ordens-servico: {destinos}")
    print(f"PASS home: {len(destinos)} link(s) para /ordens-servico e nenhum 'Kanban'")


CLASSES_PROIBIDAS = (".btn-sm", ".form-control", ".form-group", ".form-label", ".card-panel")


def _estilos_do_componente(texto: str) -> str:
    achado = re.search(r"styles\s*:\s*\[(.*?)\]\s*,?\s*\n\}\)", texto, re.DOTALL)
    return achado.group(1) if achado else ""


def _redefine(estilos: str, classe: str) -> bool:
    """Uma regra cujo seletor (num item da lista) é a própria classe, com ou sem pseudo-classe."""
    for seletores in re.findall(r"([^{}]+)\{", estilos):
        for item in seletores.split(","):
            item = item.strip()
            if re.fullmatch(re.escape(classe) + r"(?::[\w-]+(?:\([^)]*\))?)*", item):
                return True
    return False


def check_consistencia() -> None:
    violacoes: list[str] = []
    componentes = sorted(p for p in (APP / "features").rglob("*.component.ts"))
    require(componentes, "Nenhum componente de feature encontrado")
    for caminho in componentes:
        texto = caminho.read_text(encoding="utf-8")
        rel = caminho.relative_to(WEB)
        estilos = _estilos_do_componente(texto)
        for classe in CLASSES_PROIBIDAS:
            if _redefine(estilos, classe):
                violacoes.append(f"{rel}: redefine {classe} localmente")
        if re.search(r"number\s*:\s*'1\.2-2'", texto):
            violacoes.append(f"{rel}: dinheiro com number:'1.2-2' (use | moeda)")

    for nome, pasta in (("Clientes", "clientes"), ("Veículos", "veiculos"), ("Usuários", "usuarios")):
        caminho = APP / "features" / pasta / "pages" / f"{pasta}-page" / f"{pasta}-page.component.ts"
        texto = ler(caminho)
        tabela = re.search(r"<tbody>(.*?)</tbody>", texto, re.DOTALL)
        require(tabela is not None, f"{nome}: tabela sem <tbody>")
        corpo = tabela.group(1)
        botoes = re.findall(r"<button\b[^>]*>", corpo, re.DOTALL)
        excluir = [b for b in botoes if re.search(r"aria-label\]?=\"'Excluir ", b)]
        require(excluir, f"{nome}: nenhum botão com aria-label 'Excluir <nome>' na linha")
        if pasta != "usuarios":
            editar = [b for b in botoes if re.search(r"aria-label\]?=\"'Editar ", b)]
            require(editar, f"{nome}: nenhum botão com aria-label 'Editar <nome>' na linha")
        for b in botoes:
            classes = re.search(r'class="([^"]*)"', b)
            classes_lista = (classes.group(1) if classes else "").split()
            if "btn-danger" in classes_lista and "btn-ghost" not in classes_lista:
                violacoes.append(f"{nome}: btn-danger sólido na linha da tabela")
        for b in excluir:
            classes = re.search(r'class="([^"]*)"', b)
            lista = (classes.group(1) if classes else "").split()
            if not {"btn-ghost", "btn-icon"} <= set(lista):
                violacoes.append(f"{nome}: botão Excluir sem btn-ghost btn-icon")

    aprovacao = ler(APP / "features" / "portal-publico" / "pages" / "aprovacao-orcamento-page"
                    / "aprovacao-orcamento-page.component.ts")
    estilos_aprov = _estilos_do_componente(aprovacao)
    for classe in (".btn-success", ".btn-primary"):
        if _redefine(estilos_aprov, classe):
            violacoes.append(f"Aprovação redefine {classe} localmente")
    require(re.search(r"<app-id-badge[^>]*pagamentoId", aprovacao) is not None,
            "Aprovação deve mostrar o pagamentoId em <app-id-badge>")

    nova = ler(APP / "features" / "ordens-servico" / "pages" / "os-nova-page" / "os-nova-page.component.ts")
    require(re.search(r"placa", nova.split("veiculoOptions", 1)[-1] if "veiculoOptions" in nova else nova, re.IGNORECASE)
            is not None, "Nova OS sem placa nas opções de veículo")

    require(not violacoes, "Violações de consistência:\n- " + "\n- ".join(violacoes))
    print(f"PASS consistencia: {len(componentes)} componentes sem redefinições locais; ações de linha e Aprovação conformes")


def _parse_lcov(lcov: Path) -> dict[str, tuple[int, int]]:
    cobertura: dict[str, tuple[int, int]] = {}
    atual = None
    lf = lh = 0
    for linha in lcov.read_text(encoding="utf-8").splitlines():
        linha = linha.strip()
        if linha.startswith("SF:"):
            sf = linha[3:].replace("\\", "/")
            atual = "src/" + sf.split("src/", 1)[1] if "src/" in sf else sf
            lf = lh = 0
        elif linha.startswith("LF:"):
            lf = int(linha[3:])
        elif linha.startswith("LH:"):
            lh = int(linha[3:])
        elif linha == "end_of_record" and atual:
            a, b = cobertura.get(atual, (0, 0))
            cobertura[atual] = (a + lh, b + lf)
            atual = None
    return cobertura


def check_cobertura() -> None:
    run("npx", "--offline", "yarn@1.22.22", "--cwd", str(WEB), "test", "--watch=false", "--coverage", capture=False)
    run("npx", "--offline", "yarn@1.22.22", "--cwd", str(WEB), "build", capture=False)
    require((WEB / "dist" / "app-temp" / "browser" / "index.html").is_file(), "Build não gerou index.html")

    lcov = WEB / "coverage" / "app-temp" / "lcov.info"
    if not lcov.is_file():
        lcov = WEB / "coverage" / "lcov.info"
    require(lcov.is_file(), "lcov.info não gerado")
    cobertura = _parse_lcov(lcov)

    alterados = run("git", "-C", str(WEB), "diff", "--name-only", "--diff-filter=ACMR", BASE_COMMIT).splitlines()
    novos = run("git", "-C", str(WEB), "ls-files", "--others", "--exclude-standard").splitlines()
    arquivos = sorted({f for f in alterados + novos
                       if f.startswith("src/app/") and f.endswith(".ts") and not f.endswith(".spec.ts")
                       and not f.endswith(".model.ts") and not f.endswith("app.config.ts")})
    require(arquivos, "Nenhum arquivo .ts alterado desde a base")

    abaixo = []
    total_h = total_f = 0
    for arquivo in arquivos:
        h, f = cobertura.get(arquivo, (0, 0))
        if f == 0:
            continue
        total_h += h
        total_f += f
        pct = 100.0 * h / f
        print(f"  {pct:6.2f}%  {arquivo}")
        if pct < COBERTURA_MINIMA:
            abaixo.append(f"{arquivo} ({pct:.1f}%)")
    require(not abaixo, f"Arquivos abaixo de {COBERTURA_MINIMA:.0f}%: " + ", ".join(abaixo))
    print(f"PASS cobertura: {len(arquivos)} arquivos, agregado {100.0 * total_h / max(total_f, 1):.2f}% e build ok")


CHECKS = {
    "estilos": check_estilos,
    "home": check_home,
    "consistencia": check_consistencia,
    "cobertura": check_cobertura,
}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", choices=[*CHECKS, "all"], required=True)
    args = parser.parse_args()
    nomes = list(CHECKS) if args.check == "all" else [args.check]
    try:
        for nome in nomes:
            CHECKS[nome]()
    except AssertionError as erro:
        print(f"FAIL {erro}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
