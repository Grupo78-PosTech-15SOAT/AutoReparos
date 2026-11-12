"""Emissor JWT local de teste para a prova de isolamento do os-service.

Serve discovery e JWKS em http://localhost:8099 e gera tokens assinados RS256.
Chaves e tokens são descartáveis e ficam apenas no diretório de scratchpad.
"""
import base64
import json
import sys
import time
from http.server import BaseHTTPRequestHandler, HTTPServer

import jwt
from cryptography.hazmat.primitives.asymmetric import rsa
from jwt.algorithms import RSAAlgorithm

ISSUER = "http://localhost:8099"
AUDIENCE = "autoreparos-os"
KID = "proof-key"

private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
public_jwk = json.loads(RSAAlgorithm.to_jwk(private_key.public_key()))
public_jwk.update({"kid": KID, "use": "sig", "alg": "RS256"})


def token(sub, roles):
    now = int(time.time())
    payload = {"iss": ISSUER, "aud": AUDIENCE, "sub": sub, "role": roles,
               "iat": now, "nbf": now, "exp": now + 3600}
    return jwt.encode(payload, private_key, algorithm="RS256", headers={"kid": KID})


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == "/.well-known/openid-configuration":
            body = {"issuer": ISSUER, "jwks_uri": f"{ISSUER}/jwks"}
        elif self.path == "/jwks":
            body = {"keys": [public_jwk]}
        elif self.path == "/tokens":
            body = {
                "admin": token("admin-proof", ["Administrador"]),
                "billing": token("billing-proof", ["ServicoOS"]),
                "execucao": token("execucao-proof", ["ServicoOS"]),
            }
        else:
            self.send_response(404)
            self.end_headers()
            return
        data = json.dumps(body).encode()
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def log_message(self, *args):
        pass


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "tokens":
        out = {
            "admin": token("admin-proof", ["Administrador"]),
            "billing": token("billing-proof", ["ServicoOS"]),
            "execucao": token("execucao-proof", ["ServicoOS"]),
        }
        print(json.dumps(out))
    else:
        HTTPServer(("localhost", 8099), Handler).serve_forever()
