@echo off
echo ========================================
echo   MiniERP - Configuracion del Backend
echo ========================================
echo.

cd /d "%~dp0"

REM Buscar dotnet en ubicaciones comunes
set "DOTNET_PATH="
if exist "C:\Program Files\dotnet\dotnet.exe" set "DOTNET_PATH=C:\Program Files\dotnet\dotnet.exe"
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET_PATH=%ProgramFiles%\dotnet\dotnet.exe"
if exist "%ProgramFiles(x86)%\dotnet\dotnet.exe" set "DOTNET_PATH=%ProgramFiles(x86)%\dotnet\dotnet.exe"

if "%DOTNET_PATH%"=="" (
    echo ERROR: No se encontro .NET SDK
    echo.
    echo Por favor instala .NET 8.0 SDK desde:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo Usando: %DOTNET_PATH%
echo.

echo [1/4] Restaurando paquetes NuGet...
"%DOTNET_PATH%" restore src\MiniERP.API\MiniERP.API.csproj
if errorlevel 1 (
    echo ERROR: No se pudo restaurar los paquetes
    pause
    exit /b 1
)

echo.
echo [2/4] Compilando el proyecto...
"%DOTNET_PATH%" build src\MiniERP.API\MiniERP.API.csproj
if errorlevel 1 (
    echo ERROR: No se pudo compilar el proyecto
    pause
    exit /b 1
)

echo.
echo [3/4] Instalando herramienta EF Core...
"%DOTNET_PATH%" tool install --global dotnet-ef 2>nul
if errorlevel 1 (
    echo Herramienta EF Core ya instalada o error al instalar
)

echo.
echo [4/4] Creando base de datos y ejecutando migraciones...
cd src\MiniERP.API
"%DOTNET_PATH%" ef database update
if errorlevel 1 (
    echo ERROR: No se pudieron ejecutar las migraciones
    echo.
    echo Asegurate de tener SQL Server instalado y ejecutandose
    cd ..\..
    pause
    exit /b 1
)
cd ..\..

echo.
echo ========================================
echo   Configuracion completada exitosamente!
echo ========================================
echo.
echo Ahora puedes ejecutar: start.bat
echo.
pause
