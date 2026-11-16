#!/usr/bin/env python3
"""Assina o JWT de serviço da Cobrança (D-58) com a chave RSA do Cadastros.

Uso: python3 scripts/infra/token_servico.py --chave <pem> --emissor <url do API Gateway> [--dias 30]

Imprime o token na saída padrão. O token tem o mesmo formato dos emitidos pelo Cadastros
(`JwtService`): RS256, `kid` = thumbprint RFC 7638 da chave pública (o mesmo do JWKS em
`/.well-known`), `iss` = emissor sem barra final, `aud = autoreparos` (D-57), `sub = cobranca` e
papel `ServicoOS` (aceito pela política `OperarOS` do OS; a listagem de serviços e insumos da
Execução só exige autenticação). Só biblioteca padrão e `openssl`.
"""
import argparse
import base64
import hashlib
import json
import re
import subprocess
import sys
import time

AUDIENCIA = "autoreparos"
SUJEITO = "cobranca"
PAPEL = "ServicoOS"
# Mesmo tipo de claim que o JwtSecurityTokenHandler grava para ClaimTypes.Role.
CLAIM_PAPEL = "role"


def b64url(dados):
    return base64.urlsafe_b64encode(dados).rstrip(b"=").decode("ascii")


def inteiro_b64url(valor):
    return b64url(valor.to_bytes((valor.bit_length() + 7) // 8, "big"))


def componentes_publicos(caminho_chave):
    """(n, e) da chave RSA privada em PEM, lidos pelo openssl."""
    saida = subprocess.run(["openssl", "rsa", "-in", caminho_chave, "-noout", "-text"],
                           capture_output=True, text=True, check=True).stdout
    bloco = re.search(r"modulus:\s*\n((?:\s+[0-9a-f:]+\n)+)", saida)
    expoente = re.search(r"publicExponent:\s*(\d+)", saida)
    if not bloco or not expoente:
        raise ValueError("não foi possível ler módulo e expoente da chave")
    modulo = int(re.sub(r"[\s:]", "", bloco.group(1)), 16)
    return modulo, int(expoente.group(1))


def thumbprint(n, e):
    """Thumbprint RFC 7638 da JWK RSA: SHA-256 do JSON canônico {"e","kty","n"} em base64url."""
    canonico = json.dumps({"e": inteiro_b64url(e), "kty": "RSA", "n": inteiro_b64url(n)},
                          separators=(",", ":"), sort_keys=True)
    return b64url(hashlib.sha256(canonico.encode("ascii")).digest())


def assinar(caminho_chave, emissor, dias=30, agora=None):
    agora = int(time.time() if agora is None else agora)
    n, e = componentes_publicos(caminho_chave)
    cabecalho = {"alg": "RS256", "kid": thumbprint(n, e), "typ": "JWT"}
    corpo = {"iss": emissor.rstrip("/"), "aud": AUDIENCIA, "sub": SUJEITO, "nameid": SUJEITO,
             CLAIM_PAPEL: PAPEL, "iat": agora, "nbf": agora, "exp": agora + dias * 86400}
    entrada = ".".join(b64url(json.dumps(p, separators=(",", ":")).encode()) for p in (cabecalho, corpo))
    assinatura = subprocess.run(["openssl", "dgst", "-sha256", "-sign", caminho_chave],
                                input=entrada.encode("ascii"), capture_output=True, check=True).stdout
    return f"{entrada}.{b64url(assinatura)}"


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--chave", required=True)
    parser.add_argument("--emissor", required=True)
    parser.add_argument("--dias", type=int, default=30)
    args = parser.parse_args()
    if not args.emissor.startswith("https://"):
        print("emissor precisa ser a URL https do API Gateway", file=sys.stderr)
        return 2
    print(assinar(args.chave, args.emissor, args.dias))
    return 0


if __name__ == "__main__":
    sys.exit(main())
