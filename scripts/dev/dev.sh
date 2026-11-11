#!/bin/bash

action=$1
name=${2:-InitialMigration}

case $action in
  "update-tool")
    dotnet tool update --global dotnet-ef ;;
  "run")
    dotnet run --project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Api/ ;;
  "watch")
    dotnet watch --project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Api/ ;;
  "db-update")
    dotnet ef database update --project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Infrastructure/ --startup-project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Api/ ;;
  "mig-add")
    dotnet ef migrations add $name --project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Infrastructure/ --startup-project ./submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Api/ ;;
  "restore")
    dotnet restore ;;
  "kube-config")
    ./scripts/infra/update-kubeconfig.sh "$2" "$3" ;;
  *)
    echo "Uso: ./scripts/dev/dev.sh {run|watch|db-update|mig-add|update-tool|restore|kube-config}" ;;
esac