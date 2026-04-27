#!/bin/bash

action=$1
name=${2:-InitialMigration}

case $action in
  "update-tool")
    dotnet tool update --global dotnet-ef ;;
  "run")
    dotnet run --project ./AutoReparos.API/ ;;
  "watch")
    dotnet watch --project ./AutoReparos.API/ ;;
  "db-update")
    dotnet ef database update --project ./AutoReparos.Infra/ --startup-project ./AutoReparos.API/ ;;
  "mig-add")
    dotnet ef migrations add $name --project ./AutoReparos.Infra/ --startup-project ./AutoReparos.API/ ;;
  "restore")
    dotnet restore ;;
  *)
    echo "Uso: ./dev.sh {run|watch|db-update|mig-add|update-tool|restore}" ;;
esac