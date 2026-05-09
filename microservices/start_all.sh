#!/bin/bash
# IoBuild Microservices - Startup Script (Bash)
# Uso: ./start_all.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================="
echo "  IoBuild Microservices - Startup"
echo "=========================================="
echo ""

# Cargar variables desde .env
if [ ! -f ".env" ]; then
    echo "[ERROR] No se encontró el archivo .env"
    echo "Copia .env.example a .env:"
    echo "  cp .env.example .env"
    exit 1
fi

echo "Cargando variables desde .env..."
export $(grep -v '^#' .env | xargs)

echo "DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_USER=$DB_USER"
echo ""

# Servicios a iniciar
declare -a services=(
  "src/IoBuild.Gateway:8080:iobuild_iam:true"
  "src/IoBuild.IAM:5001:iobuild_iam:false"
  "src/IoBuild.Devices:5002:iobuild_devices:false"
  "src/IoBuild.Projects:5003:iobuild_projects:false"
  "src/IoBuild.Subscriptions:5004:iobuild_subscriptions:false"
  "src/IoBuild.Analytics:5005:iobuild_analytics:false"
)

for svc in "${services[@]}"; do
  IFS=':' read -r project port db no_db <<< "$svc"
  
  # Verificar si el puerto ya está en uso
  if lsof -i:$port >/dev/null 2>&1; then
    echo "⚠️  Puerto $port ya está en uso, omitiendo..."
    continue
  fi
  
  echo "▶️  Iniciando $(basename $project) en puerto $port..."
  
  # Exportar DB_NAME si no es el gateway
  if [ "$no_db" != "true" ]; then
    export DB_NAME="$db"
  fi
  
  # Iniciar en background
  dotnet run --project "$project" > /dev/null 2>&1 &
  
  sleep 2
done

echo ""
echo "=========================================="
echo "  ✅ Todos los servicios fueron lanzados!"
echo "=========================================="
echo ""
echo "Gateway: http://localhost:8080"
echo "Health:  curl http://localhost:8080/health"
echo ""
echo "Para verificar cada servicio:"
echo "  curl http://localhost:5001/health   (IAM)"
echo "  curl http://localhost:5002/health   (Devices)"
echo "  curl http://localhost:5003/health   (Projects)"
echo "  curl http://localhost:5004/health   (Subscriptions)"
echo "  curl http://localhost:5005/health   (Analytics)"
echo ""
echo "Para detener todo: ./kill_all.sh"