# IoBuild - Integration Tests Script (PowerShell)
# Ejecuta tests de integración contra las APIs reales en runtime
# Uso: .\run_integration_tests.ps1

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  IoBuild - Integration Tests (Runtime)" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# ============================================
# 1. LEVANTAR SERVICIOS
# ============================================
Write-Host "1. Iniciando microservicios..." -ForegroundColor Yellow

Set-Location $PSScriptRoot
.\start_all.sh *> $null

# Esperar a que los servicios estén listos
Write-Host "   Esperando a que los servicios inicialicen..." -ForegroundColor Gray
Start-Sleep -Seconds 15

# Verificar que Gateway esté disponible
$attempts = 0
while ($attempts -lt 30) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -Method GET -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            break
        }
    } catch {}
    Start-Sleep -Seconds 2
    $attempts++
}

Write-Host "   Servicios iniciados y saludables" -ForegroundColor Green
Write-Host ""

# ============================================
# 2. EJECUTAR TESTS
# ============================================
Write-Host "2. Ejecutando tests de integración..." -ForegroundColor Yellow
Write-Host ""

$passed = 0
$failed = 0

function Test-HttpEndpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [string]$Body = $null,
        [string]$ExpectedCode = "200",
        [string]$Headers = @{}
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        if ($Headers.Count -gt 0) {
            foreach ($key in $Headers.Keys) {
                $params.Headers = @{ $key = $Headers[$key] }
            }
        }
        
        $response = Invoke-WebRequest @params -ErrorAction SilentlyContinue
        $actualCode = $response.StatusCode
        
        if ($actualCode -eq $ExpectedCode) {
            Write-Host "   [PASS] $Name" -ForegroundColor Green
            $script:passed++
            return $true
        } else {
            Write-Host "   [FAIL] $Name (Expected: $ExpectedCode, Got: $actualCode)" -ForegroundColor Red
            $script:failed++
            return $false
        }
    } catch {
        Write-Host "   [FAIL] $Name - Error: $_" -ForegroundColor Red
        $script:failed++
        return $false
    }
}

# Test 1: Health Check
Write-Host "--- Test 1: Health Check Global ---" -ForegroundColor Cyan
Test-HttpEndpoint -Name "Gateway health check" -Url "http://localhost:8080/health" -ExpectedCode "200"

# Test 2: Sign Up
Write-Host "--- Test 2: Registro de Usuario ---" -ForegroundColor Cyan
$signupBody = '{"email":"testuser@iobuild.com","password":"Test123!","role":"PropertyManager"}'
Test-HttpEndpoint -Name "Sign Up" -Url "http://localhost:8080/api/v1/authentication/sign-up" -Method "POST" -Body $signupBody -ExpectedCode "200"

# Test 3: Sign In
Write-Host "--- Test 3: Login (Sign In) ---" -ForegroundColor Cyan
$signinBody = '{"email":"testuser@iobuild.com","password":"Test123!"}'
$signinResponse = Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authentication/sign-in" -Method POST -Body $signinBody -ContentType "application/json"
$token = $signinResponse.token

if ($token) {
    Write-Host "   Token JWT extraído correctamente" -ForegroundColor Gray
    Test-HttpEndpoint -Name "Sign In" -Url "http://localhost:8080/api/v1/authentication/sign-in" -Method "POST" -Body $signinBody -ExpectedCode "200"
} else {
    Write-Host "   [FAIL] No se pudo obtener token" -ForegroundColor Red
    $script:failed++
}

# Test 4: Login incorrecto
Write-Host "--- Test 4: Login fallido ---" -ForegroundColor Cyan
$wrongBody = '{"email":"testuser@iobuild.com","password":"WrongPassword!"}'
Test-HttpEndpoint -Name "Sign In - contraseña incorrecta" -Url "http://localhost:8080/api/v1/authentication/sign-in" -Method "POST" -Body $wrongBody -ExpectedCode "401"

# Test 5: Crear proyecto (autenticado)
if ($token) {
    Write-Host "--- Test 5: Crear Proyecto ---" -ForegroundColor Cyan
    $projectBody = '{"name":"Edificio Test","description":"Proyecto de prueba","location":"Lima","totalUnits":10}'
    Test-HttpEndpoint -Name "Crear proyecto" -Url "http://localhost:8080/api/v1/projects" -Method "POST" -Body $projectBody -ExpectedCode "201" -Headers @{ Authorization = "Bearer $token" }
}

# Test 6: Listar proyectos (autenticado)
if ($token) {
    Write-Host "--- Test 6: Listar Proyectos ---" -ForegroundColor Cyan
    Test-HttpEndpoint -Name "Listar proyectos" -Url "http://localhost:8080/api/v1/projects" -ExpectedCode "200" -Headers @{ Authorization = "Bearer $token" }
}

# Test 7: Sin auth - proyectos
Write-Host "--- Test 7: Sin auth - proyectos ---" -ForegroundColor Cyan
Test-HttpEndpoint -Name "Listar proyectos sin token" -Url "http://localhost:8080/api/v1/projects" -ExpectedCode "401"

# Test 8: Sin auth - dispositivos
Write-Host "--- Test 8: Sin auth - dispositivos ---" -ForegroundColor Cyan
Test-HttpEndpoint -Name "Listar dispositivos sin token" -Url "http://localhost:8080/api/v1/devices" -ExpectedCode "401"

# Test 9: Planes (público)
Write-Host "--- Test 9: Listar Planes (público) ---" -ForegroundColor Cyan
Test-HttpEndpoint -Name "Listar planes" -Url "http://localhost:8080/api/v1/plans" -ExpectedCode "200"

# Test 10: Logout
if ($token) {
    Write-Host "--- Test 10: Logout ---" -ForegroundColor Cyan
    Test-HttpEndpoint -Name "Logout" -Url "http://localhost:8080/api/v1/authentication/sign-in" -Method "POST" -Body $signinBody -ExpectedCode "200" -Headers @{ Authorization = "Bearer $token" }
}

# Test 11: Health checks de todos los servicios
Write-Host "--- Test 11: Health Checks de todos los servicios ---" -ForegroundColor Cyan
$servicesOk = 0
foreach ($port in @(5001, 5002, 5003, 5004, 5005)) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$port/health" -Method GET -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "   Puerto $port : OK" -ForegroundColor Green
            $servicesOk++
        } else {
            Write-Host "   Puerto $port : FAIL" -ForegroundColor Red
        }
    } catch {
        Write-Host "   Puerto $port : FAIL" -ForegroundColor Red
    }
}

if ($servicesOk -eq 5) {
    Write-Host "   [PASS] Todos los 5 servicios saludables" -ForegroundColor Green
    $passed++
} else {
    Write-Host "   [FAIL] Solo $servicesOk/5 servicios" -ForegroundColor Red
    $failed++
}

# ============================================
# 3. RESUMEN
# ============================================
Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE TESTS" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Pasados: $passed" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host ""

if ($failed -eq 0) {
    Write-Host "TODOS LOS TESTS PASARON!" -ForegroundColor Green
    $RESULT = 0
} else {
    Write-Host "ALGUNOS TESTS FALLARON" -ForegroundColor Red
    $RESULT = 1
}

# ============================================
# 4. LIMPIEZA
# ============================================
Write-Host ""
Write-Host "Deteniendo servicios..." -ForegroundColor Yellow
.\kill_all.sh *> $null
Start-Sleep -Seconds 3
Write-Host "Servicios detenidos" -ForegroundColor Green

exit $RESULT