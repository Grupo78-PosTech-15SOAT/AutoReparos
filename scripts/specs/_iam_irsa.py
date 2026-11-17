"""Casos de simular_iam.py --caso irsa (ver o docstring de simular_iam.py para o formato).

C13: as políticas IRSA (infra/terraform/politicas/irsa.json.tftpl de cada serviço) só deixam cada serviço
publicar nas filas que o código dele usa. Topologia (a fila pertence ao consumidor):
OS -> autoreparos-cobranca-comandos, autoreparos-execucao-liberadas, autoreparos-cadastros-notificacoes;
Cobrança -> autoreparos-os-saga; Execução -> autoreparos-os-saga, autoreparos-cobranca-execucoes.
"""

CONTA = "198903849859"
REGIAO = "us-east-1"
VARIAVEIS = {"conta": CONTA, "regiao": REGIAO}


def _modelo(repo):
    return f"submodules/AutoReparos.{repo}/infra/terraform/politicas/irsa.json.tftpl"


def _fila(nome):
    return f"arn:aws:sqs:{REGIAO}:{CONTA}:{nome}"


def _tabela(nome):
    return f"arn:aws:dynamodb:{REGIAO}:{CONTA}:table/{nome}"


def _envio(repo, fila, permitido):
    servico = repo.lower()
    efeito = "envia" if permitido else "não envia"
    return (f"{servico} {efeito} a {fila}", _modelo(repo), VARIAVEIS, "sqs:SendMessage", _fila(fila), permitido, None)


def casos():
    return [
        # OS: filas de entrada de Cobrança, Execução e Cadastros e as próprias; nada fora da lista.
        _envio("OS", "autoreparos-cobranca-comandos", True),
        _envio("OS", "autoreparos-execucao-liberadas", True),
        _envio("OS", "autoreparos-cadastros-notificacoes", True),
        _envio("OS", "autoreparos-os-saga", True),
        _envio("OS", "autoreparos-os-saga-dlq", True),
        _envio("OS", "autoreparos-cobranca-execucoes", False),
        _envio("OS", "autoreparos-execucao-saida-dlq", False),
        # C14 de infra-ci-observabilidade (D-66): o gauge saga_dlq_mensagens lê a profundidade da DLQ da Saga.
        ("os lê a profundidade da DLQ da Saga", _modelo("OS"), VARIAVEIS, "sqs:GetQueueAttributes",
         _fila("autoreparos-os-saga-dlq"), True, None),
        ("os não lê a profundidade da DLQ da Execução", _modelo("OS"), VARIAVEIS, "sqs:GetQueueAttributes",
         _fila("autoreparos-execucao-saida-dlq"), False, None),
        ("os lê a tabela da Saga", _modelo("OS"), VARIAVEIS, "dynamodb:GetItem",
         _tabela("autoreparos-os-saga"), True, None),
        # Cobrança: só a fila da Saga entre as filas de outros serviços.
        _envio("Cobranca", "autoreparos-os-saga", True),
        _envio("Cobranca", "autoreparos-os-saga-dlq", False),
        _envio("Cobranca", "autoreparos-execucao-liberadas", False),
        _envio("Cobranca", "autoreparos-cadastros-notificacoes", False),
        # Execução: só as filas do OS e da Cobrança, e nunca a tabela da Saga.
        _envio("Execucao", "autoreparos-os-saga", True),
        _envio("Execucao", "autoreparos-cobranca-execucoes", True),
        _envio("Execucao", "autoreparos-cadastros-notificacoes", False),
        _envio("Execucao", "autoreparos-os-saga-dlq", False),
        ("execucao não lê a tabela da Saga", _modelo("Execucao"), VARIAVEIS, "dynamodb:GetItem",
         _tabela("autoreparos-os-saga"), False, None),
        ("execucao não grava a tabela da Saga", _modelo("Execucao"), VARIAVEIS, "dynamodb:PutItem",
         _tabela("autoreparos-os-saga"), False, None),
        # Cadastros: consome a própria fila e não publica em filas de outros serviços.
        ("cadastros consome autoreparos-cadastros-notificacoes", _modelo("Cadastros"), VARIAVEIS,
         "sqs:ReceiveMessage", _fila("autoreparos-cadastros-notificacoes"), True, None),
        _envio("Cadastros", "autoreparos-os-saga", False),
    ]
