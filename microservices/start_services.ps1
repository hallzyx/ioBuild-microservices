# IoBuild Microservices — Startup Script
# Puerto XAMPP MySQL: 33065
#
# Uso: Abrir PowerShell en la carpeta microservices/ y ejecutar:
#   .\start_services.ps1
#
# Para servicios individuales, usar en terminales separadas:
#   $Env:DB_NAME="iobuild_iam"; dotnet run --project src/IoBuild.IAM

$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# ── Cargar variables desde .env ──
if (-not (Test-Path ".env")) {
    Write-Host "[ERROR] No se encontro el archivo .env" -ForegroundColor Red
    Write-Host "Copia .env.example a .env y configura tus credenciales:" -ForegroundColor Yellow
    Write-Host "  copy .env.example .env" -ForegroundColor Yellow
    Write-Host "  notepad .env" -ForegroundColor Yellow
    exit 1
}

Get-Content ".env" | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith("#")) {
        $parts = $line -split "=", 2
        if ($parts.Count -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim()
            [Environment]::SetEnvironmentVariable($key, $value, "Process")
        }
    }
}

Write-Host "Variables cargadas:" -ForegroundColor Gray
Write-Host "  DB_HOST=$env:DB_HOST  DB_PORT=$env:DB_PORT  DB_USER=$env:DB_USER" -ForegroundColor Gray

# ── Servicios a iniciar ──
$services = @(
    @{ Name = "Gateway";       Project = "src/IoBuild.Gateway";       Port = 8080;  NoDB = $true },
    @{ Name = "IAM";           Project = "src/IoBuild.IAM";           Port = 5001;  DB = "iobuild_iam" },
    @{ Name = "Devices";       Project = "src/IoBuild.Devices";       Port = 5002;  DB = "iobuild_devices" },
    @{ Name = "Projects";      Project = "src/IoBuild.Projects";      Port = 5003;  DB = "iobuild_projects" },
    @{ Name = "Subscriptions"; Project = "src/IoBuild.Subscriptions"; Port = 5004;  DB = "iobuild_subscriptions" },
    @{ Name = "Analytics";     Project = "src/IoBuild.Analytics";     Port = 5005;  DB = "iobuild_analytics" }
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IoBuild Microservices - Startup"        -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

foreach ($svc in $services) {
    $portStatus = netstat -ano | findstr ":$($svc.Port) " | findstr LISTENING
    if ($portStatus) {
        Write-Host "⚠️  $($svc.Name) - puerto $($svc.Port) ya esta en uso, omitiendo..." -ForegroundColor Yellow
        continue
    }

    if (-not $svc.NoDB) {
        $Env:DB_NAME = $svc.DB
    }

    Write-Host "▶️  Iniciando $($svc.Name) en puerto $($svc.Port)..." -ForegroundColor Green

    # Inicia en ventana separada (no bloquea esta terminal)
    Start-Process -WindowStyle Hidden -FilePath "dotnet" -ArgumentList "run --project $($svc.Project)"

    Start-Sleep -Seconds 3
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Todos los servicios fueron lanzados!"   -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Gateway: http://localhost:8080"
Write-Host "Health:  curl http://localhost:8080/health"
Write-Host ""
Write-Host "Para verificar cada servicio:"
Write-Host "  curl http://localhost:5001/health   (IAM)"
Write-Host "  curl http://localhost:5002/health   (Devices)"
Write-Host "  curl http://localhost:5003/health   (Projects)"
Write-Host "  curl http://localhost:5004/health   (Subscriptions)"
Write-Host "  curl http://localhost:5005/health   (Analytics)"
Write-Host ""
Write-Host "Para detener todo: .\kill_services.ps1"
