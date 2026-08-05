#!/usr/bin/env bash
# dev-forward.sh — Port-forwards para todos os serviços do cluster AutoReparos
# Uso: ./dev-forward.sh [start|stop|status]

PID_FILE="/tmp/autoreparos-pf.pids"

start_forwards() {
  echo "🚀 Iniciando port-forwards em background..."
  > "$PID_FILE"

  _forward() {
    local name="$1"; local svc="$2"; local ports="$3"
    kubectl port-forward "$svc" $ports --address 127.0.0.1 \
      > "/tmp/pf-$(echo "$name" | tr ' ' '-').log" 2>&1 &
    local pid=$!
    sleep 1
    if kill -0 "$pid" 2>/dev/null; then
      echo "  ✅ $name → http://localhost:${ports%%:*}"
      echo "$pid $name" >> "$PID_FILE"
    else
      echo "  ❌ $name — falhou (log: /tmp/pf-$(echo "$name" | tr ' ' '-').log)"
    fi
  }

  _forward "API"        "svc/autoreparos-k8s-service" "8080:8080"
  _forward "Grafana"    "svc/grafana"                 "3000:3000"
  _forward "Jaeger"     "svc/jaeger"                  "16686:16686"
  _forward "Prometheus" "svc/prometheus"              "9090:9090"
  _forward "Loki"       "svc/loki"                    "3100:3100"

  echo ""
  echo "📌 URLs:"
  echo "   🔵 API / Swagger  → http://localhost:8080/swagger"
  echo "   📊 Grafana        → http://localhost:3000  (admin / admin)"
  echo "   🔍 Jaeger UI      → http://localhost:16686"
  echo "   📈 Prometheus     → http://localhost:9090"
  echo "   📋 Loki (API)     → http://localhost:3100/ready"
  echo ""
  echo "   Para parar tudo: ./dev-forward.sh stop"
}

stop_forwards() {
  if [ ! -f "$PID_FILE" ]; then
    echo "⚠️  Nenhum port-forward registrado."
    return
  fi
  echo "🛑 Parando port-forwards..."
  while read -r pid name; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" && echo "  ✅ $name (PID $pid) encerrado"
    else
      echo "  ⚠️  $name já estava parado"
    fi
  done < "$PID_FILE"
  rm -f "$PID_FILE"
}

status_forwards() {
  if [ ! -f "$PID_FILE" ]; then
    echo "ℹ️  Nenhum port-forward registrado."
    return
  fi
  echo "📡 Status:"
  while read -r pid name; do
    kill -0 "$pid" 2>/dev/null \
      && echo "  🟢 $name (PID $pid) — rodando" \
      || echo "  🔴 $name (PID $pid) — parado"
  done < "$PID_FILE"
}

case "${1:-start}" in
  start)  start_forwards ;;
  stop)   stop_forwards ;;
  status) status_forwards ;;
  *) echo "Uso: $0 [start|stop|status]"; exit 1 ;;
esac
