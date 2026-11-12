#!/usr/bin/env python3
"""Provas da extração do frontend para o submódulo Web (fase4/web)."""

import argparse
import os
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WEB = ROOT / "submodules" / "AutoReparos.Web"
CADASTROS = ROOT / "submodules" / "AutoReparos.Cadastros"
BASE_CADASTROS = "3d71ba8"
BASE_WEB = "91a1aa1"
OLD_PATH = "submodules/AutoReparos.Cadastros/AutoReparos.Web"
NEW_PATH = "submodules/AutoReparos.Web"
IMAGE = "autoreparos-web-spec-proof:local"


def run(*args, cwd=ROOT, capture=True):
    result = subprocess.run(
        [str(arg) for arg in args],
        cwd=str(cwd),
        stdout=subprocess.PIPE if capture else None,
        stderr=subprocess.PIPE if capture else None,
        check=False,
    )
    if result.returncode:
        detail = (result.stderr or result.stdout or b"").decode("utf-8", "replace")
        raise AssertionError("Comando falhou ({}): {}".format(result.returncode, detail.strip()))
    return result.stdout or b""


def git(repo, *args):
    return run("git", "-C", repo, *args).decode("utf-8", "replace").strip()


def require(condition, message):
    if not condition:
        raise AssertionError(message)


def source():
    require(git(WEB, "branch", "--show-current") == "develop", "Web não está na develop")
    require(git(WEB, "rev-parse", "main") == git(WEB, "rev-parse", BASE_WEB), "main do Web mudou")
    git(WEB, "merge-base", "--is-ancestor", BASE_WEB, "HEAD")
    paths = git(CADASTROS, "ls-tree", "-r", "--name-only", BASE_CADASTROS, "--", "AutoReparos.Web")
    files = [path for path in paths.splitlines() if path.startswith("AutoReparos.Web/")]
    require(len(files) == 98, "Baseline do Angular deveria conter 98 arquivos; encontrou {}".format(len(files)))
    ci_exceptions = {"README.md", "package.json", "yarn.lock", "angular.json", "docker-entrypoint.sh"}
    for original in files:
        relative = original[len("AutoReparos.Web/") :]
        destination = WEB / relative
        require(destination.is_file(), "Arquivo migrado ausente: {}".format(destination))
        git(WEB, "cat-file", "-e", "HEAD:{}".format(relative))
        if relative in ci_exceptions:
            continue  # Documentação e configuração/dependência de cobertura do CI são ajustes aprovados.
        expected = run("git", "-C", CADASTROS, "show", "{}:{}".format(BASE_CADASTROS, original))
        committed = run("git", "-C", WEB, "show", "HEAD:{}".format(relative))
        require(committed == expected, "Conteúdo original mudou: {}".format(original))
    print("PASS source: {} arquivos migrados; {} intactos".format(len(files), len(files) - len(ci_exceptions)))


def cadastros():
    require(not git(CADASTROS, "ls-tree", "HEAD", "--", "AutoReparos.Web"), "Cadastros ainda rastreia o frontend")
    require(not (CADASTROS / "AutoReparos.Web").exists(), "Pasta Web ainda existe no Cadastros")
    for name in (".github/workflows/ci-cd.yml", "docker-compose.yml"):
        require("AutoReparos.Web" not in (CADASTROS / name).read_text(encoding="utf-8"), "Referência Web antiga em {}".format(name))
    print("PASS cadastros: cópia e referências locais removidas")


def gitlink():
    modules = (ROOT / ".gitmodules").read_text(encoding="utf-8")
    block = modules.split('[submodule "submodules/AutoReparos.Web"]', 1)[-1].split("[submodule", 1)[0]
    require("branch = develop" in block, ".gitmodules não fixa branch develop do Web")
    entry = git(ROOT, "ls-tree", "HEAD", "--", NEW_PATH)
    match = re.match(r"160000 commit ([0-9a-f]{40})\s+", entry)
    require(match is not None, "Ponteiro Web ausente no HEAD do pai")
    sha = match.group(1)
    require(sha == git(WEB, "rev-parse", "HEAD"), "Ponteiro do pai difere do HEAD Web")
    git(WEB, "merge-base", "--is-ancestor", sha, "origin/develop")
    print("PASS gitlink: {} em origin/develop".format(sha[:7]))


def yarn():
    npm = shutil.which("npm") or shutil.which("npm.cmd")
    require(npm is not None, "npm não encontrado para executar Node 22 e Yarn 1.22.22")
    for args in (("install", "--frozen-lockfile"), ("build",), ("test", "--watch=false"), ("test", "--watch=false", "--coverage")):
        print("Yarn {}".format(" ".join(args)), flush=True)
        run(npm, "exec", "--yes", "--package=node@22", "--package=yarn@1.22.22", "--", "yarn", *args, cwd=WEB, capture=False)
    require((WEB / "dist/app-temp/browser/index.html").is_file(), "index.html não foi gerado")
    print("PASS yarn: install, build e test")


def docker_build():
    docker = shutil.which("docker")
    require(docker is not None, "Docker não encontrado")
    run(docker, "build", "-t", IMAGE, "-f", WEB / "Dockerfile", WEB, capture=False)
    return docker


def container(docker, api_url=None):
    args = [docker, "run", "--rm", "-d"]
    if api_url is not None:
        args.extend(("-e", "API_URL={}".format(api_url)))
    args.append(IMAGE)
    cid = run(*args).decode("ascii").strip()
    try:
        yield cid
    finally:
        subprocess.run([docker, "rm", "-f", cid], stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)


def docker():
    from contextlib import contextmanager

    docker_cmd = docker_build()
    with contextmanager(container)(docker_cmd) as cid:
        for route in ("/", "/login"):
            for attempt in range(10):
                try:
                    output = run(docker_cmd, "exec", cid, "wget", "-qO-", "http://127.0.0.1:8080" + route)
                    break
                except AssertionError:
                    if attempt == 9:
                        raise
                    time.sleep(1)
            require(b"<html" in output.lower(), "HTML ausente em {}".format(route))
    print("PASS docker: / e /login servem HTML 200")


def runtime_env():
    from contextlib import contextmanager

    docker_cmd = docker_build()
    with contextmanager(container)(docker_cmd, "https://api.example.test") as cid:
        content = run(docker_cmd, "exec", cid, "cat", "/usr/share/nginx/html/env.js")
        require(b"window.__env.apiUrl = 'https://api.example.test'" in content, "API_URL não foi injetada")
    with contextmanager(container)(docker_cmd) as cid:
        content = run(docker_cmd, "exec", cid, "cat", "/usr/share/nginx/html/env.js")
        original = (WEB / "public/env.js").read_bytes()
        require(content == original, "env.js padrão mudou sem API_URL")
    print("PASS runtime-env: URL injetada e padrão preservado")


def web_ci():
    workflows = list((WEB / ".github/workflows").glob("*.yml"))
    require(workflows, "Workflow Web ausente")
    content = "\n".join(path.read_text(encoding="utf-8") for path in workflows)
    require("pull_request:" in content and "push:" in content, "Gatilhos PR/push ausentes")
    for branch in ("main", "develop"):
        require(branch in content, "Branch {} ausente no workflow".format(branch))
    for job in ("build", "test", "quality"):
        require(re.search(r"^  {}:\s*$".format(job), content, re.MULTILINE), "Job {} ausente".format(job))
    require("secrets.SONAR_TOKEN" in content, "SONAR_TOKEN não vem de secrets")
    require(re.search(r"node-version:\s*['\"]?22['\"]?", content), "Workflow Web não usa Node 22")
    print("PASS web-ci: gatilhos, jobs e secret")


def parent_paths():
    paths = (
        "docker-compose.yml",
        ".github/workflows/ci.yml",
        ".github/workflows/deploy-app.yml",
        ".github/workflows/provision-aws.yml",
        "scripts/infra/build-push-images.sh",
    )
    for name in paths:
        content = (ROOT / name).read_text(encoding="utf-8")
        require(NEW_PATH in content, "Caminho Web novo ausente em {}".format(name))
        require(OLD_PATH not in content, "Caminho Web antigo em {}".format(name))
    print("PASS parent-paths: {} consumidores usam o Web".format(len(paths)))


def cadastros_ci():
    content = (CADASTROS / ".github/workflows/ci-cd.yml").read_text(encoding="utf-8")
    require("  frontend:" not in content, "Cadastros ainda tem job frontend")
    require("AutoReparos.Web" not in content, "Cadastros CI ainda depende do Web")
    require("dotnet build" in content and "dotnet test" in content, "Build/test backend ausentes")
    print("PASS cadastros-ci: somente backend")


def readme():
    content = (WEB / "README.md").read_text(encoding="utf-8")
    for item in ("yarn install --frozen-lockfile", "yarn build", "yarn test --watch=false", "API_URL", "develop", "paths HTTP"):
        require(item in content, "README Web sem {}".format(item))
    print("PASS readme: operação e pendência documentadas")


CHECKS = {
    "source": source,
    "cadastros": cadastros,
    "gitlink": gitlink,
    "yarn": yarn,
    "docker": docker,
    "runtime-env": runtime_env,
    "web-ci": web_ci,
    "parent-paths": parent_paths,
    "cadastros-ci": cadastros_ci,
    "readme": readme,
}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", required=True, choices=CHECKS)
    args = parser.parse_args()
    try:
        CHECKS[args.check]()
    except (AssertionError, OSError) as exc:
        print("FAIL {}: {}".format(args.check, exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
