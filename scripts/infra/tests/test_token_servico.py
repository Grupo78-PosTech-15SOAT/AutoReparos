"""C37 (D-58): o token de serviço da Cobrança tem o formato dos tokens do Cadastros e é aceito pela chave pública."""
import base64
import json
import os
import subprocess
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import token_servico  # noqa: E402

EMISSOR = "https://abc123.execute-api.us-east-1.amazonaws.com/"


def decodificar(parte):
    return json.loads(base64.urlsafe_b64decode(parte + "=" * (-len(parte) % 4)))


class TokenServicoTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.dir = tempfile.TemporaryDirectory()
        cls.chave = os.path.join(cls.dir.name, "chave.pem")
        subprocess.run(["openssl", "genpkey", "-algorithm", "RSA", "-pkeyopt", "rsa_keygen_bits:2048",
                        "-out", cls.chave], check=True, capture_output=True)
        cls.agora = 1_800_000_000
        cls.token = token_servico.assinar(cls.chave, EMISSOR, dias=30, agora=cls.agora)
        cls.cabecalho, cls.corpo, cls.assinatura = cls.token.split(".")

    @classmethod
    def tearDownClass(cls):
        cls.dir.cleanup()

    def test_thumbprint_do_exemplo_da_rfc_7638(self):
        n = int.from_bytes(base64.urlsafe_b64decode(
            "0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMs"
            "tn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5h"
            "ajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw"
            "=="), "big")
        self.assertEqual(token_servico.thumbprint(n, 65537), "NzbLsXh8uDCcd-6MNwXF4W_7noWXFZAfHkxZsRGC9Xs")

    def test_cabecalho_rs256_com_kid_da_chave_publica(self):
        n, e = token_servico.componentes_publicos(self.chave)
        self.assertEqual(decodificar(self.cabecalho),
                         {"alg": "RS256", "kid": token_servico.thumbprint(n, e), "typ": "JWT"})

    def test_claims_de_servico_com_audiencia_e_validade_de_30_dias(self):
        corpo = decodificar(self.corpo)
        self.assertEqual(corpo["iss"], EMISSOR.rstrip("/"))
        self.assertEqual(corpo["aud"], "autoreparos")
        self.assertEqual(corpo["sub"], "cobranca")
        self.assertEqual(corpo["role"], "ServicoOS")
        self.assertEqual(corpo["exp"] - corpo["iat"], 30 * 86400)
        self.assertEqual(corpo["iat"], self.agora)

    def test_assinatura_confere_com_a_chave_publica(self):
        publica = os.path.join(self.dir.name, "publica.pem")
        assinatura = os.path.join(self.dir.name, "assinatura.bin")
        subprocess.run(["openssl", "rsa", "-in", self.chave, "-pubout", "-out", publica], check=True, capture_output=True)
        with open(assinatura, "wb") as arquivo:
            arquivo.write(base64.urlsafe_b64decode(self.assinatura + "=" * (-len(self.assinatura) % 4)))
        verificacao = subprocess.run(
            ["openssl", "dgst", "-sha256", "-verify", publica, "-signature", assinatura],
            input=f"{self.cabecalho}.{self.corpo}".encode(), capture_output=True)
        self.assertEqual(verificacao.returncode, 0, verificacao.stdout + verificacao.stderr)


if __name__ == "__main__":
    unittest.main()
