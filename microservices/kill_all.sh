#!/bin/bash
# IoBuild Microservices - Kill Script (Bash, Cross-Platform)
# Uso: ./kill_all.sh
# Funciona en: Windows (Git Bash/WSL), macOS, Linux

echo "=========================================="
echo "  IoBuild Microservices - Shutdown"
echo "=========================================="
echo ""

# Detectar SO
OS="$(uname -s)"
echo "Sistema detectado: $OS"

# Matar procesos segun el SO
if [[ "$OS" == *"MINGW"* ]] || [[ "$OS" == *"CYGWIN"* ]] || [[ "$OS" == *"MSYS"* ]]; then
    # Windows (Git Bash)
    echo "Ejecutando taskkill (Windows)..."
    taskkill //F //IM dotnet.exe 2>/dev/null
elif [[ "$OS" == "Darwin" ]]; then
    # macOS
    echo "Ejecutando pkill (macOS)..."
    pkill -f "dotnet.*IoBuild" 2>/dev/null
else
    # Linux
    echo "Ejecutando pkill (Linux)..."
    pkill -f "dotnet.*IoBuild" 2>/dev/null
fi

# Verificar puertos
echo ""
echo "Verificando puertos..."

for port in 8080 5001 5002 5003 5004 5005; do
    if netstat -ano 2>/dev/null | grep ":$port " | grep LISTENING >/dev/null; then
        echo "Advertencia: puerto $port aun ocupado"
    else
        echo "Puerto $port liberado"
    fi
done

echo ""
sleep 2

if curl -s --connect-timeout 2 http://localhost:8080/health >/dev/null 2>&1; then
    echo "[WARN] Algunos servicios pueden seguir activos"
else
    echo "[OK] Todos los servicios fueron detenidos"
fi