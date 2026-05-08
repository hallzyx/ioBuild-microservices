# IoBuild Microservices — Kill Script
#
# Uso: Abrir PowerShell en la carpeta microservices/ y ejecutar:
#   .\kill_services.ps1
#
# Mata todos los procesos en los puertos de los microservicios

$ports = @(8080, 5001, 5002, 5003, 5004, 5005)

Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host "  IoBuild Microservices - Shutdown"       -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

$found = $false

foreach ($port in $ports) {
    $line = netstat -ano | findstr ":$port " | findstr LISTENING
    if ($line) {
        $pid = ($line -split '\s+')[-1]
        try {
            Stop-Process -Id $pid -Force -ErrorAction Stop
            Write-Host "✅ Puerto $port ($([string]::Join(', ', ($line | ForEach-Object { ($_ -split '\s+')[-1] })))) - Eliminado" -ForegroundColor Green
            $found = $true
        } catch {
            Write-Host "❌ Puerto $port - No se pudo eliminar: $_" -ForegroundColor Red
        }
    }
}

# Tambien matar procesos 'dotnet' que esten corriendo desde src/IoBuild.
# (Esto no afecta otros procesos .NET que no sean de IoBuild)
$dotnetProcs = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcs) {
    foreach ($proc in $dotnetProcs) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -and $cmdLine.Contains("IoBuild")) {
                Stop-Process -Id $proc.Id -Force
                Write-Host "✅ Proceso dotnet PID $($proc.Id) (IoBuild) - Eliminado" -ForegroundColor Green
                $found = $true
            }
        } catch {
            # No hay permiso o no se pudo leer
        }
    }
}

Write-Host ""

if (-not $found) {
    Write-Host "ℹ️  No se encontraron procesos de IoBuild corriendo." -ForegroundColor Yellow
} else {
    Write-Host "✅ Todos los servicios de IoBuild fueron detenidos." -ForegroundColor Green
}

Write-Host ""
Write-Host "Para verificar:"
Write-Host "  netstat -ano | findstr \":8080 :5001 :5002 :5003 :5004 :5005\""
