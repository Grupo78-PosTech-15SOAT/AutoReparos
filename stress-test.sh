#!/bin/bash

# Define a URL base para o teste de carga
if [[ -z "$1" ]]; then
  echo "🔍 Buscando URL do Ingress automaticamente via kubectl..."
  # Tenta obter o hostname do LoadBalancer do Ingress no namespace default
  INGRESS_HOST=$(kubectl get ingress -o jsonpath='{.items[0].status.loadBalancer.ingress[0].hostname}' 2>/dev/null)
  
  if [[ -z "$INGRESS_HOST" ]]; then
    # Tenta obter o IP do LoadBalancer do Ingress
    INGRESS_HOST=$(kubectl get ingress -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}' 2>/dev/null)
  fi

  if [[ ! -z "$INGRESS_HOST" ]]; then
    BASE_URL="http://${INGRESS_HOST}"
    echo "✅ URL do Ingress detectada: $BASE_URL"
  else
    # Se falhar ou estiver no Kind local sem LoadBalancer externo, usa localhost como fallback
    echo "⚠️ Não foi possível obter o endereço do Ingress via kubectl. Usando http://localhost como fallback."
    BASE_URL="http://localhost"
  fi
else
  BASE_URL="$1"
  echo "👉 Usando a URL fornecida manualmente: $BASE_URL"
fi

URL="${BASE_URL}/api/ordem-servico/consulta?placa=AAA1A11"

# Número de processos paralelos (concorrência) - configurável por variável de ambiente
CONCURRENCY=${CONCURRENCY:-5}

# Tamanho do lote de requisições por worker (usando paralelismo do curl)
BATCH_SIZE=${BATCH_SIZE:-20}

# Adiciona o parâmetro de cachebuster para habilitar o globbing paralelizável do curl
if [[ "$URL" == *"?"* ]]; then
  CURL_URL="${URL}&cb=[1-${BATCH_SIZE}]"
else
  CURL_URL="${URL}?cb=[1-${BATCH_SIZE}]"
fi

echo "=========================================================="
echo "🔥 Iniciando teste de carga para HPA 🔥"
echo "=========================================================="
echo "URL Base: $URL"
echo "URL de Teste: $CURL_URL"
echo "Workers (Processos): $CONCURRENCY"
echo "Lote por Worker: $BATCH_SIZE requisições simultâneas"
echo "Carga por Ciclo: ~$((CONCURRENCY * BATCH_SIZE)) requisições"
echo "Acompanhe a subida dos pods no Grafana!"
echo "Pressione [CTRL+C] para parar o teste a qualquer momento."
echo "=========================================================="

# Arquivo temporário para guardar os PIDs dos workers
PID_FILE="/tmp/stress-test-pids.$$"

# Função de limpeza ao interromper (CTRL+C) ou sair
cleanup() {
    # Desativa traps para evitar recursão
    trap - SIGINT EXIT
    echo -e "\n\n🛑 Parando o teste de carga e finalizando os workers..."
    if [[ -f "$PID_FILE" ]]; then
        while read -r pid; do
            kill -9 "$pid" 2>/dev/null
        done < "$PID_FILE"
        rm -f "$PID_FILE" 2>/dev/null
    fi
    echo "✅ Concluído. O HPA deve começar a reduzir as réplicas em alguns minutos."
    exit 0
}

# Captura o sinal de interrupção (SIGINT / CTRL+C) e saída
trap cleanup SIGINT EXIT

# Função executada por cada worker em paralelo
run_worker() {
    # Garante que o worker saia imediatamente ao receber sinais de finalização
    trap 'exit 0' SIGINT SIGTERM
    while true; do
        # Executa o curl e descarta o output
        curl -s -Z --parallel-max "$BATCH_SIZE" --connect-timeout 3 --max-time 10 -o /dev/null "$CURL_URL"
        # Envia a quantidade enviada neste lote para a saída
        echo "$BATCH_SIZE"
    done
    return 0
}

# Função de contagem acumulada (mostra de 10.000 em 10.000)
counting_loop() {
    local total_sent=0
    local last_printed=0
    while read -r count; do
        total_sent=$((total_sent + count))
        local current_ten_thousands=$((total_sent / 10000))
        local last_ten_thousands=$((last_printed / 10000))
        if (( current_ten_thousands > last_ten_thousands )); then
            last_printed=$((current_ten_thousands * 10000))
            echo "🚀 Total de requisições enviadas: $last_printed"
        fi
    done
}

echo "Status do envio das requisições:"
# Inicializa os workers em background, guardando os PIDs e direcionando a saída para a contagem
for ((i=1; i<=CONCURRENCY; i++)); do
    run_worker &
    echo $! >> "$PID_FILE"
done | counting_loop
