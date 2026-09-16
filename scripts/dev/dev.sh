#!/bin/bash

action=$1
name=${2:-InitialMigration}

case $action in
  "update-tool")
    dotnet tool update --global dotnet-ef ;;
  "run")
    dotnet run --project ./submodules/AutoReparos.App/AutoReparos.API/ ;;
  "watch")
    dotnet watch --project ./submodules/AutoReparos.App/AutoReparos.API/ ;;
  "db-update")
    dotnet ef database update --project ./submodules/AutoReparos.App/AutoReparos.Infra/ --startup-project ./submodules/AutoReparos.App/AutoReparos.API/ ;;
  "mig-add")
    dotnet ef migrations add $name --project ./submodules/AutoReparos.App/AutoReparos.Infra/ --startup-project ./submodules/AutoReparos.App/AutoReparos.API/ ;;
  "restore")
    dotnet restore ;;
  "kube-config")
    ./scripts/infra/update-kubeconfig.sh "$2" "$3" ;;
  *)
    echo "Uso: ./scripts/dev/dev.sh {run|watch|db-update|mig-add|update-tool|restore|kube-config}" ;;
esac