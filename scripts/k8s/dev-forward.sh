#!/usr/bin/env bash
# dev-forward.sh — Port-forwards flexível e dinâmico para os serviços Kubernetes/Helm do AutoReparos
# Uso: ./scripts/k8s/dev-forward.sh [start|stop|status]

PID_FILE="/tmp/autoreparos-pf.pids"
NAMESPACE="${K8S_NAMESPACE:-default}"

start_forwards() {
  echo "🚀 Iniciando port-forwards em background (Namespace: $NAMESPACE)..."
  > "$PID_FILE"

  # Função auxiliar para resolver o nome exato do serviço no k8s (suporta com/sem prefixo de release Helm)
  _resolve_svc() {
    local candidate
    for candidate in "$1" "autoreparos-$1" "autoreparos-k8s-$1" "${1%-service}" "autoreparos-${1%-service}"; do
      if kubectl get svc "$candidate" -n "$NAMESPACE" >/dev/null 2>&1; then
        echo "$candidate"
        return 0
      fi
    done
    echo ""
  }

  _forward() {
    local label="$1"
    local raw_svc="$2"
    local ports="$3"
    local path="$4"

    local resolved_svc
    resolved_svc=$(_resolve_svc "$raw_svc")

    if [[ -z "$resolved_svc" ]]; then
      echo "  ⚠️  $label ($raw_svc) não encontrado no cluster/namespace."
      return 1
    fi

    nohup kubectl port-forward "svc/$resolved_svc" $ports --address 127.0.0.1 -n "$NAMESPACE" \
      > "/tmp/pf-$(echo "$label" | tr ' ' '-').log" 2>&1 &
    local pid=$!
    disown "$pid" 2>/dev/null || true
    sleep 1

    if kill -0 "$pid" 2>/dev/null; then
      local local_port="${ports%%:*}"
      echo "  ✅ $label ($resolved_svc) → http://localhost:${local_port}${path}"
      echo "$pid $label ($local_port)" >> "$PID_FILE"
    else
      echo "  ❌ $label falhou ao iniciar (log em /tmp/pf-$(echo "$label" | tr ' ' '-').log)"
    fi
  }

  echo "🔌 Conectando serviços..."
  _forward "API .NET"     "api-service"         "8080:8080"  "/swagger"
  _forward "Frontend Web" "web-service"         "4200:80"    "/"
  _forward "PostgreSQL"   "database-service"    "5432:5432"  ""
  _forward "Grafana"      "grafana"             "3000:3000"  ""
  _forward "Jaeger"       "jaeger"              "16686:16686" ""
  _forward "Prometheus"   "prometheus"          "9090:9090"  ""
  _forward "Loki"         "loki"                "3100:3100"  "/ready"

  echo ""
  echo "📌 URLs de Acesso Rápido:"
  echo "   🔵 API / Swagger  → http://localhost:8080/swagger"
  echo "   💻 Frontend Web   → http://localhost:4200"
  echo "   🗄️  PostgreSQL     → localhost:5432"
  echo "   📊 Grafana        → http://localhost:3000  (admin / admin)"
  echo "   🔍 Jaeger UI      → http://localhost:16686"
  echo "   📈 Prometheus     → http://localhost:9090"
  echo "   📋 Loki API       → http://localhost:3100/ready"
  echo ""
  echo "⚠️  Nota sobre o comportamento do 'kubectl port-forward':"
  echo "   O kubectl encerra as conexões quando a sessão do terminal pai é fechada."
  echo "   Para manter as portas abertas de forma contínua no terminal aberto, use: ./scripts/k8s/dev-forward.sh run"
  echo ""
  echo "   💡 Para encerrar as conexões em background: ./scripts/k8s/dev-forward.sh stop"
  return 0
}

run_foreground() {
  echo "🚀 Iniciando port-forwards interativos no terminal (pressione CTRL+C para encerrar)..."
  stop_forwards >/dev/null 2>&1 || true

  trap stop_forwards EXIT SIGINT SIGTERM

  kubectl port-forward svc/autoreparos-api 8080:8080 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-web 4200:80 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-database-service 5432:5432 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-grafana 3000:3000 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-jaeger 16686:16686 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-prometheus 9090:9090 >/dev/null 2>&1 &
  kubectl port-forward svc/autoreparos-loki 3100:3100 >/dev/null 2>&1 &

  echo ""
  echo "🟢 Conexões ativas:"
  echo "   🔵 API / Swagger  → http://localhost:8080/swagger"
  echo "   💻 Frontend Web   → http://localhost:4200"
  echo "   🗄️  PostgreSQL     → localhost:5432"
  echo "   📊 Grafana        → http://localhost:3000"
  echo "   🔍 Jaeger UI      → http://localhost:16686"
  echo "   📈 Prometheus     → http://localhost:9090"
  echo "   📋 Loki API       → http://localhost:3100/ready"
  echo ""
  echo "Mantendo portas abertas. Pressione [CTRL+C] para parar."
  wait
}

stop_forwards() {
  if [[ ! -f "$PID_FILE" ]]; then
    echo "⚠️  Nenhum port-forward ativo no momento."
    return
  fi
  echo "🛑 Parando port-forwards..."
  while read -r pid info; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" 2>/dev/null && echo "  ✅ $info (PID $pid) encerrado"
    else
      echo "  ⚠️  $info (PID $pid) já finalizado"
    fi
  done < "$PID_FILE"
  rm -f "$PID_FILE"
  pkill -f "kubectl port-forward" 2>/dev/null || true
}

status_forwards() {
  if [[ ! -f "$PID_FILE" ]]; then
    echo "ℹ️  Nenhum port-forward ativo."
    return
  fi
  echo "📡 Status das Conexões:"
  while read -r pid info; do
    if kill -0 "$pid" 2>/dev/null; then
      echo "  🟢 $info (PID $pid) — ativo"
    else
      echo "  🔴 $info (PID $pid) — inativo/caído"
    fi
  done < "$PID_FILE"
}

case "${1:-start}" in
  start)  start_forwards ;;
  run)    run_foreground ;;
  stop)   stop_forwards ;;
  status) status_forwards ;;
  *) echo "Uso: $0 [start|run|stop|status]"; exit 1 ;;
esac
