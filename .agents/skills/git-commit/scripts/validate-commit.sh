#!/usr/bin/env bash
set -e

echo "==============================================="
echo "   AutoReparos Pre-Commit Validation Runner   "
echo "==============================================="

# 1. Check for staged secrets or forbidden files
echo "--> Checking staged files for secrets..."
STAGED_FILES=$(git diff --cached --name-only)

if [ -z "$STAGED_FILES" ]; then
  echo "❌ Error: No files staged for commit."
  exit 1
fi

FORBIDDEN_PATTERNS="(\.env|local\.json|appsettings\.Local\.json|\.pem|\.key)"
if echo "$STAGED_FILES" | grep -E -q "$FORBIDDEN_PATTERNS"; then
  echo "❌ Error: Forbidden secret or local config file staged:"
  echo "$STAGED_FILES" | grep -E "$FORBIDDEN_PATTERNS"
  echo "Unstage these files before committing!"
  exit 1
fi

# 2. Check for hardcoded secrets in staged diff
echo "--> Checking diff for hardcoded secret keywords..."
if git diff --cached | grep -iE "(password|secret|jwtsecret|sendgrid.*key).*=.*[\"'][a-zA-Z0-9_\-]{8,}[\"']" | grep -v "appsettings.Development.json"; then
  echo "⚠️ Warning: Potential hardcoded secret detected in staged diff! Double check before committing."
fi

# 3. .NET Build Validation
echo "--> Building .NET solution (AutoReparos.slnx)..."
dotnet build AutoReparos.slnx -c Release --verbosity quiet
echo "✅ .NET Build succeeded."

# 4. Run affected unit tests
echo "--> Running Domain & Application unit tests..."
dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj -c Release --verbosity quiet
dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj -c Release --verbosity quiet
echo "✅ Unit tests passed."

# 5. Angular Web build validation if web folder touched
if echo "$STAGED_FILES" | grep -q "^AutoReparos.Web/"; then
  echo "--> Validating Angular Frontend build..."
  (cd AutoReparos.Web && yarn build --configuration development)
  echo "✅ Angular build succeeded."
fi

echo "==============================================="
echo "✅ Pre-Commit Validation Complete! Ready to commit."
echo "==============================================="
