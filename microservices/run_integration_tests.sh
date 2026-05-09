#!/bin/bash
# IoBuild - Integration Tests Script
# Ejecuta tests de integración contra las APIs reales en runtime

echo "=============================================="
echo "  IoBuild - Integration Tests (Runtime)"
echo "=============================================="
echo ""

PASSED=0
FAILED=0

# ============================================
# 1. VERIFICAR O INICIAR SERVICIOS
# ============================================
echo "1. Verificando servicios..."

# Check if services are already running
if curl -s http://localhost:8080/health > /dev/null 2>&1; then
    echo "   Servicios ya están corriendo"
    SKIP_START=true
else
    echo "   Iniciando servicios..."
    cd "$(dirname "$0")"
    ./start_all.sh > /dev/null 2>&1
    SKIP_START=false
    
    # Wait for services
    echo "   Esperando inicialización..."
    sleep 15
fi

# Verify health
if ! curl -s http://localhost:8080/health > /dev/null 2>&1; then
    echo "ERROR: Los servicios no están disponibles"
    exit 1
fi

echo "   Servicios listos"
echo ""

# ============================================
# 2. EJECUTAR TESTS
# ============================================
echo "2. Ejecutando tests..."
echo ""

# Helper function
run_test() {
    local name="$1"
    local cmd="$2"
    local expected="$3"
    
    result=$(eval "$cmd" 2>/dev/null)
    code=$(eval "$cmd" 2>/dev/null | grep -o "[0-9]\{3\}" | head -1)
    
    if [[ "$code" == "$expected" ]]; then
        echo "[PASS] $name"
        ((PASSED++))
    else
        echo "[FAIL] $name (expected: $expected, got: $code)"
        ((FAILED++))
    fi
}

# Test 1: Health Check
echo "--- Test 1: Health Check Global ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
if [[ "$HTTP_CODE" == "200" ]]; then
    echo "[PASS] Gateway health check"
    ((PASSED++))
else
    echo "[FAIL] Gateway health check (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 2: Sign Up (200 = nuevo, 409 = ya existia)
echo "--- Test 2: Registro (Sign Up) ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8080/api/v1/authentication/sign-up \
    -H "Content-Type: application/json" \
    -d '{"email":"testuser@iobuild.com","password":"Test123!","role":"PropertyManager"}')
if [[ "$HTTP_CODE" == "200" || "$HTTP_CODE" == "409" ]]; then
    echo "[PASS] Sign Up"
    ((PASSED++))
else
    echo "[FAIL] Sign Up (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 3: Sign In
echo "--- Test 3: Login (Sign In) ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8080/api/v1/authentication/sign-in \
    -H "Content-Type: application/json" \
    -d '{"email":"testuser@iobuild.com","password":"Test123!"}')
if [[ "$HTTP_CODE" == "200" ]]; then
    echo "[PASS] Sign In"
    ((PASSED++))
    # Get token
    TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/authentication/sign-in \
        -H "Content-Type: application/json" \
        -d '{"email":"testuser@iobuild.com","password":"Test123!"}' | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
else
    echo "[FAIL] Sign In (got: $HTTP_CODE)"
    ((FAILED++))
    TOKEN=""
fi

# Test 4: Sign In wrong password
echo "--- Test 4: Login fallido ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8080/api/v1/authentication/sign-in \
    -H "Content-Type: application/json" \
    -d '{"email":"testuser@iobuild.com","password":"Wrong!"}')
if [[ "$HTTP_CODE" == "401" ]]; then
    echo "[PASS] Sign In wrong password"
    ((PASSED++))
else
    echo "[FAIL] Sign In wrong password (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 5: Create project (with auth)
echo "--- Test 5: Crear Proyecto ---"
if [[ -n "$TOKEN" ]]; then
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8080/api/v1/projects \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $TOKEN" \
        -d '{"name":"Test Project","description":"Test","location":"Lima","totalUnits":5,"imageUrl":"https://example.com/img.jpg"}')
    if [[ "$HTTP_CODE" == "201" ]]; then
        echo "[PASS] Create project"
        ((PASSED++))
    else
        echo "[FAIL] Create project (got: $HTTP_CODE)"
        ((FAILED++))
    fi
else
    echo "[SKIP] No token available"
fi

# Test 6: List projects (with auth)
echo "--- Test 6: Listar Proyectos ---"
if [[ -n "$TOKEN" ]]; then
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/v1/projects \
        -H "Authorization: Bearer $TOKEN")
    if [[ "$HTTP_CODE" == "200" ]]; then
        echo "[PASS] List projects"
        ((PASSED++))
    else
        echo "[FAIL] List projects (got: $HTTP_CODE)"
        ((FAILED++))
    fi
else
    echo "[SKIP] No token"
fi

# Test 7: List projects without auth
echo "--- Test 7: Sin auth - proyectos ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/v1/projects)
if [[ "$HTTP_CODE" == "401" ]]; then
    echo "[PASS] List projects without auth"
    ((PASSED++))
else
    echo "[FAIL] List projects without auth (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 8: List devices without auth
echo "--- Test 8: Sin auth - dispositivos ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/v1/devices)
if [[ "$HTTP_CODE" == "401" ]]; then
    echo "[PASS] List devices without auth"
    ((PASSED++))
else
    echo "[FAIL] List devices without auth (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 9: List plans (public)
echo "--- Test 9: Listar Planes (publico) ---"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/v1/plans)
if [[ "$HTTP_CODE" == "200" ]]; then
    echo "[PASS] List plans (public)"
    ((PASSED++))
else
    echo "[FAIL] List plans (public) (got: $HTTP_CODE)"
    ((FAILED++))
fi

# Test 10: Health checks all services
echo "--- Test 10: Health Checks individuales ---"
SERVICES_OK=0
for port in 5001 5002 5003 5004 5005; do
    CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$port/health 2>/dev/null)
    if [[ "$CODE" == "200" ]]; then
        ((SERVICES_OK++))
    fi
done
if [[ $SERVICES_OK -eq 5 ]]; then
    echo "[PASS] All 5 services healthy"
    ((PASSED++))
else
    echo "[FAIL] Only $SERVICES_OK/5 services healthy"
    ((FAILED++))
fi

# ============================================
# 3. RESUMEN
# ============================================
echo ""
echo "=============================================="
echo "  RESUMEN"
echo "=============================================="
echo "Pasados: $PASSED"
echo "Fallidos: $FAILED"
echo ""

if [[ $FAILED -eq 0 ]]; then
    echo "TODOS LOS TESTS PASARON!"
    RESULT=0
else
    echo "ALGUNOS TESTS FALLARON"
    RESULT=1
fi

# ============================================
# 4. LIMPIEZA
# ============================================
echo ""
if [[ "$SKIP_START" == "true" ]]; then
    echo "Servicios ya estaban corriendo - no se detienen"
else
    echo "Deteniendo servicios..."
    ./kill_all.sh > /dev/null 2>&1
    sleep 2
    echo "Listo"
fi

exit $RESULT