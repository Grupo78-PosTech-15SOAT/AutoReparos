#!/bin/sh

# Injeta a variável API_URL no env.js se configurada
if [ -n "$API_URL" ]; then
  echo "Setting API URL to: $API_URL"
  echo "(function(window) { window.__env = window.__env || {}; window.__env.apiUrl = '$API_URL'; })(this);" > /usr/share/nginx/html/env.js
fi

exec "$@"
