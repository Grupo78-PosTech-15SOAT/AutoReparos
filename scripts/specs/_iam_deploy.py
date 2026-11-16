"""Casos de simular_iam.py --caso deploy (ver o docstring de simular_iam.py para o formato).

C24: a role `autoreparos-deploy-cobranca` (modelo `deploy-servico.json.tftpl` da plataforma) é negada fora
dos próprios recursos e ao criar role sem o boundary, e alcança as próprias filas e o provisionador.
"""
CONTA = "198903849859"
REGIAO = "us-east-1"
MODELO = "submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-servico.json.tftpl"
BOUNDARY = f"arn:aws:iam::{CONTA}:policy/autoreparos-servico-boundary"
PROVISIONADOR = f"arn:aws:lambda:{REGIAO}:{CONTA}:function:autoreparos-provisionador-db"


def variaveis(servico):
    """Os mesmos valores que o stacks/base passa ao templatefile()."""
    return {
        "servico": servico,
        "conta": CONTA,
        "regiao": REGIAO,
        "bucket": f"autoreparos-tfstate-{CONTA}",
        "tabela_lock": "autoreparos-terraform-locks",
        "cluster": "autoreparos-cluster",
    }


def fila(nome):
    return f"arn:aws:sqs:{REGIAO}:{CONTA}:{nome}"


def role(nome):
    return f"arn:aws:iam::{CONTA}:role/{nome}"


def casos():
    cobranca = variaveis("cobranca")
    return [
        ("cobranca cria fila do OS", MODELO, cobranca,
         "sqs:CreateQueue", fila("autoreparos-os-eventos-saga"), False, None),
        ("cobranca cria role sem boundary", MODELO, cobranca,
         "iam:CreateRole", role("autoreparos-cobranca-irsa"), False, None),
        ("cobranca cria role com outro boundary", MODELO, cobranca,
         "iam:CreateRole", role("autoreparos-cobranca-irsa"), False,
         {"iam:PermissionsBoundary": ["arn:aws:iam::aws:policy/AdministratorAccess"]}),
        ("cobranca remove o boundary da própria role", MODELO, cobranca,
         "iam:DeleteRolePermissionsBoundary", role("autoreparos-cobranca-irsa"), False, None),
        ("cobranca cria a própria fila", MODELO, cobranca,
         "sqs:CreateQueue", fila("autoreparos-cobranca-solicitacoes"), True, None),
        ("cobranca cria a própria role IRSA com o boundary", MODELO, cobranca,
         "iam:CreateRole", role("autoreparos-cobranca-irsa"), True,
         {"iam:PermissionsBoundary": [BOUNDARY], "aws:RequestTag/Servico": ["cobranca"]}),
        ("cobranca chama a Function URL do provisionador", MODELO, cobranca,
         "lambda:InvokeFunctionUrl", PROVISIONADOR, True, None),
    ]
