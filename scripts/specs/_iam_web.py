"""Casos de simular_iam.py --caso web (ver o docstring de simular_iam.py para o formato).

C2 de web-deploy: a role `autoreparos-deploy-web` (modelo `deploy-web.json.tftpl` da plataforma) publica só no
ECR `autoreparos-web`, descreve só o cluster `AutoReparosEKS` e é negada em SQS, SNS, DynamoDB, Secrets
Manager, IAM, SSM, no state do Terraform e no provisionador de database.
"""
CONTA = "198903849859"
REGIAO = "us-east-1"
CLUSTER = "AutoReparosEKS"
MODELO = "submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-web.json.tftpl"
VARIAVEIS = {"conta": CONTA, "regiao": REGIAO, "cluster": CLUSTER}


def ecr(repositorio):
    return f"arn:aws:ecr:{REGIAO}:{CONTA}:repository/{repositorio}"


def casos():
    def caso(rotulo, acao, recurso, permitido):
        return (rotulo, MODELO, VARIAVEIS, acao, recurso, permitido, None)

    return [
        caso("web faz login no ECR", "ecr:GetAuthorizationToken", "*", True),
        caso("web publica em autoreparos-web", "ecr:PutImage", ecr("autoreparos-web"), True),
        caso("web envia camadas a autoreparos-web", "ecr:InitiateLayerUpload", ecr("autoreparos-web"), True),
        caso("web lê imagem de autoreparos-web", "ecr:BatchGetImage", ecr("autoreparos-web"), True),
        caso("web descreve o cluster", "eks:DescribeCluster",
             f"arn:aws:eks:{REGIAO}:{CONTA}:cluster/{CLUSTER}", True),
        caso("web publica em autoreparos-os", "ecr:PutImage", ecr("autoreparos-os"), False),
        caso("web publica em autoreparos-web-outro", "ecr:PutImage", ecr("autoreparos-web-outro"), False),
        caso("web descreve outro cluster", "eks:DescribeCluster",
             f"arn:aws:eks:{REGIAO}:{CONTA}:cluster/outro", False),
        caso("web envia à fila do OS", "sqs:SendMessage",
             f"arn:aws:sqs:{REGIAO}:{CONTA}:autoreparos-os-saga", False),
        caso("web publica no tópico da Execução", "sns:Publish",
             f"arn:aws:sns:{REGIAO}:{CONTA}:autoreparos-execucao-eventos", False),
        caso("web grava na tabela da Saga", "dynamodb:PutItem",
             f"arn:aws:dynamodb:{REGIAO}:{CONTA}:table/autoreparos-os-saga", False),
        caso("web lê segredo de database", "secretsmanager:GetSecretValue",
             f"arn:aws:secretsmanager:{REGIAO}:{CONTA}:secret:autoreparos/production/os/database-abc123", False),
        caso("web cria role", "iam:CreateRole", f"arn:aws:iam::{CONTA}:role/autoreparos-web-irsa", False),
        caso("web passa role", "iam:PassRole", f"arn:aws:iam::{CONTA}:role/autoreparos-deploy-web", False),
        caso("web lê parâmetro SSM", "ssm:GetParameter",
             f"arn:aws:ssm:{REGIAO}:{CONTA}:parameter/autoreparos/production/eks/cluster_name", False),
        caso("web lê o state do Terraform", "s3:GetObject",
             f"arn:aws:s3:::autoreparos-tfstate-{CONTA}/network/production/terraform.tfstate", False),
        caso("web chama o provisionador", "lambda:InvokeFunctionUrl",
             f"arn:aws:lambda:{REGIAO}:{CONTA}:function:autoreparos-provisionador-db", False),
    ]
