#!/bin/bash

# URL do endpoint de consulta da API
URL="http://af25409a2f61b4a43bcbca78623473fa-be8009ff32a736a2.elb.us-east-1.amazonaws.com/api/ordem-servico/consulta?placa=AAA1A11"

# Número de processos paralelos (concorrência)
CONCURRENCY=20

echo "=========================================================="
echo "🔥 Iniciando teste de carga para escalar a API via HPA 🔥"
echo "=========================================================="
echo "URL: $URL"
echo "Concorrência: $CONCURRENCY workers em paralelo"
echo "Acompanhe a subida dos pods no Grafana!"
echo "Pressione [CTRL+C] para parar o teste a qualquer momento."
echo "=========================================================="

# Lista de PIDs dos processos em background
PIDS=()

# Função de limpeza ao interromper (CTRL+C)
cleanup() {
    echo -e "\n\n🛑 Parando o teste de carga e finalizando os workers..."
    for pid in "${PIDS[@]}"; do
        kill -9 "$pid" 2>/dev/null
    done
    echo "✅ Concluído. O HPA deve começar a reduzir as réplicas em alguns minutos."
    exit 0
}

# Captura o sinal de interrupção (SIGINT / CTRL+C)
trap cleanup SIGINT

# Função executada por cada worker em paralelo
run_worker() {
    while true; do
        # Executa o curl silenciando a saída e descartando o corpo da resposta
        curl -s -o /dev/null -w "%{http_code} " "$URL"
    done
}

# Inicializa os workers em background
for ((i=1; i<=CONCURRENCY; i++)); do
    run_worker &
    PIDS+=($!)
done

echo "Status das requisições (HTTP Status Code):"
# Mantém o script rodando e exibe mensagens de status dos workers em background
wait
