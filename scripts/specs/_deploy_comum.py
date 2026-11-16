"""Utilitários comuns às provas de deploy-servicos (fase 4)."""
import subprocess

SERVICOS_IMPLANTADOS = ["OS", "Cobranca", "Execucao", "Cadastros"]


def executar(comando, diretorio=None):
    """Roda o comando e devolve (código de saída, saída combinada)."""
    processo = subprocess.run(comando, cwd=diretorio, capture_output=True, text=True, encoding="utf-8")
    return processo.returncode, (processo.stdout + processo.stderr).strip()


def ultimas_linhas(texto, quantidade=20):
    return "\n".join(texto.splitlines()[-quantidade:])
