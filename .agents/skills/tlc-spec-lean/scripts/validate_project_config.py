#!/usr/bin/env python3
"""Check project TLC configuration and its bundled workflow dependencies."""
from pathlib import Path
import re
import sys


def main() -> int:
    root = Path(__file__).resolve().parents[4]
    feature = sys.argv[1] if len(sys.argv) > 1 else ""
    config = (root / ".agents" / "AGENTS.md").read_text(encoding="utf-8")
    checks_path = root / ".specs" / "features" / feature / "checks.md"
    if not feature or not checks_path.is_file():
        print("usage: validate_project_config.py <feature>", file=sys.stderr)
        return 2
    match = re.search(r"(?ms)^## tlc-spec-lean\s*\n\s*mutations:\s*(\d+)\s*$", config)
    checks = checks_path.read_text(encoding="utf-8")
    declared = re.search(r"(?m)^Mutations:\s*(\d+)\s*$", checks)
    if not match or not declared:
        print("missing TLC mutations declaration in project config or checks.md", file=sys.stderr)
        return 1
    if match.group(1) != declared.group(1):
        print(f"mutation count mismatch: project={match.group(1)}, checks={declared.group(1)}", file=sys.stderr)
        return 1
    package = root / ".agents" / "skills" / "tlc-spec-lean"
    required = [
        "SKILL.md", "NOTICE.md",
        "references/plan.md", "references/checks.md", "references/build.md",
        "references/verify.md", "references/verify-screens.md", "references/memory.md",
        "scripts/validate_plan.py", "scripts/validate_checks.py", "scripts/validate_verification.py",
        "scripts/validate_project_config.py", "scripts/check_proofs.py", "scripts/mutate.py",
        "scripts/lessons.py", "scripts/check_commit.py", "scripts/lean_config.py",
        "scripts/runners.py", "scripts/selftest.py",
        "scripts/fixtures/plan.md", "scripts/fixtures/checks.md",
        "scripts/fixtures/proofs.md", "scripts/fixtures/verification.md",
    ]
    missing = [item for item in required if not (package / item).is_file()]
    skill_text = (package / "SKILL.md").read_text(encoding="utf-8")
    linked = re.findall(r"\]\((references/[^)#]+)", skill_text)
    missing.extend(item for item in linked if not (package / item).is_file())
    if missing:
        print("TLC package files missing: " + ", ".join(sorted(set(missing))), file=sys.stderr)
        return 1
    print(f"TLC config valid for {feature}: mutations={declared.group(1)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
