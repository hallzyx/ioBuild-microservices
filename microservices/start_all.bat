@echo off
title IoBuild - Startup
cd /d "%~dp0"

echo ========================================
echo   IoBuild Microservices - Startup
echo ========================================
echo.

:: Load .env file
if not exist .env (
    echo [ERROR] No se encontro el archivo .env
    echo Copia .env.example a .env y configura tus credenciales:
    echo   copy .env.example .env
    echo   notepad .env
    pause
    exit /b 1
)

echo Cargando variables desde .env ...
for /f "usebackq tokens=1,2 delims==" %%a in (".env") do (
    set "line=%%a"
    if not "!line:~0,1!"=="#" if not "%%a"=="" set "%%a=%%b"
)

:: Verify DB vars
if "%DB_HOST%"=="" ( echo [ERROR] DB_HOST no definido en .env & pause & exit /b 1 )

echo DB_HOST=%DB_HOST% DB_PORT=%DB_PORT% DB_USER=%DB_USER%
echo.

echo ▶️ Iniciando Gateway en puerto 8080...
start "IoBuild-Gateway" dotnet run --project src/IoBuild.Gateway
timeout /t 3 /nobreak >nul

echo ▶️ Iniciando IAM en puerto 5001...
start "IoBuild-IAM" cmd /c "set DB_HOST=%DB_HOST% && set DB_PORT=%DB_PORT% && set DB_USER=%DB_USER% && set DB_PASSWORD=%DB_PASSWORD% && set JWT_SECRET=%JWT_SECRET% && set DB_NAME=iobuild_iam && dotnet run --project src/IoBuild.IAM"
timeout /t 3 /nobreak >nul

echo ▶️ Iniciando Devices en puerto 5002...
start "IoBuild-Devices" cmd /c "set DB_HOST=%DB_HOST% && set DB_PORT=%DB_PORT% && set DB_USER=%DB_USER% && set DB_PASSWORD=%DB_PASSWORD% && set DB_NAME=iobuild_devices && dotnet run --project src/IoBuild.Devices"
timeout /t 3 /nobreak >nul

echo ▶️ Iniciando Projects en puerto 5003...
start "IoBuild-Projects" cmd /c "set DB_HOST=%DB_HOST% && set DB_PORT=%DB_PORT% && set DB_USER=%DB_USER% && set DB_PASSWORD=%DB_PASSWORD% && set DB_NAME=iobuild_projects && dotnet run --project src/IoBuild.Projects"
timeout /t 3 /nobreak >nul

echo ▶️ Iniciando Subscriptions en puerto 5004...
start "IoBuild-Subscriptions" cmd /c "set DB_HOST=%DB_HOST% && set DB_PORT=%DB_PORT% && set DB_USER=%DB_USER% && set DB_PASSWORD=%DB_PASSWORD% && set STRIPE_SECRET_KEY=%STRIPE_SECRET_KEY% && set STRIPE_PUBLISHABLE_KEY=%STRIPE_PUBLISHABLE_KEY% && set DB_NAME=iobuild_subscriptions && dotnet run --project src/IoBuild.Subscriptions"
timeout /t 3 /nobreak >nul

echo ▶️ Iniciando Analytics en puerto 5005...
start "IoBuild-Analytics" cmd /c "set DB_HOST=%DB_HOST% && set DB_PORT=%DB_PORT% && set DB_USER=%DB_USER% && set DB_PASSWORD=%DB_PASSWORD% && set DB_NAME=iobuild_analytics && dotnet run --project src/IoBuild.Analytics"

echo.
echo ✅ Todos los servicios fueron lanzados!
echo.
echo Gateway: http://localhost:8080
echo Health:  curl http://localhost:8080/health
echo.
echo Para detener todo: .\kill_all.bat
echo.
pause
