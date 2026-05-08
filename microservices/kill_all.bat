@echo off
echo ========================================
echo   IoBuild Microservices - Shutdown
echo ========================================
echo.

for %%p in (8080 5001 5002 5003 5004 5005) do (
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":%%p " ^| findstr LISTENING') do (
        echo Deteniendo proceso %%a en puerto %%p ...
        taskkill /F /PID %%a 2>nul
    )
)

echo.
echo ✅ Todos los servicios detenidos
echo Para verificar:
echo   netstat -ano ^| findstr ":8080 :5001 :5002 :5003 :5004 :5005"
pause
