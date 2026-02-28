@echo off
echo ========================================
echo MiniERP - Script de Instalacion
echo ========================================
echo.

echo [1/6] Verificando requisitos...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8 SDK no esta instalado
    echo Descargalo de: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js no esta instalado
    echo Descargalo de: https://nodejs.org/
    pause
    exit /b 1
)

echo OK: Requisitos verificados
echo.

echo [2/6] Restaurando paquetes del backend...
cd src\MiniERP.API
dotnet restore
if errorlevel 1 (
    echo ERROR: Fallo al restaurar paquetes
    pause
    exit /b 1
)
echo.

echo [3/6] Instalando herramienta Entity Framework...
dotnet tool install --global dotnet-ef
echo.

echo [4/6] Creando base de datos...
dotnet ef database update --project ..\MiniERP.Infrastructure --startup-project .
if errorlevel 1 (
    echo ERROR: Fallo al crear la base de datos
    echo Verifica que SQL Server este ejecutandose
    pause
    exit /b 1
)
echo.

echo [5/6] Instalando dependencias del frontend...
cd ..\MiniERP.Web
call npm install
if errorlevel 1 (
    echo ERROR: Fallo al instalar dependencias de npm
    pause
    exit /b 1
)
echo.

echo [6/6] Instalacion completada!
echo.
echo ========================================
echo Para ejecutar el sistema:
echo.
echo Backend:  cd src\MiniERP.API ^&^& dotnet run
echo Frontend: cd src\MiniERP.Web ^&^& npm run dev
echo.
echo Usuario: admin@minierp.com
echo Password: Admin123!
echo ========================================
pause
