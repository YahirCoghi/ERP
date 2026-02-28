@echo off
echo ========================================
echo Instalando dependencias del Frontend
echo ========================================
echo.

cd /d "%~dp0src\MiniERP.Web"

echo Directorio actual: %CD%
echo.

echo Verificando Node.js...
"C:\Program Files\nodejs\node.exe" --version
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Node.js no esta instalado o no se encuentra en C:\Program Files\nodejs\
    echo Por favor, instala Node.js desde https://nodejs.org/
    pause
    exit /b 1
)

echo.
echo Verificando npm...
"C:\Program Files\nodejs\npm.cmd" --version
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: npm no esta disponible
    pause
    exit /b 1
)

echo.
echo Instalando paquetes npm (esto puede tomar varios minutos)...
echo.
"C:\Program Files\nodejs\npm.cmd" install

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Instalacion completada exitosamente!
    echo ========================================
    echo.
    echo Puedes iniciar el frontend con: npm run dev
) else (
    echo.
    echo ========================================
    echo Error durante la instalacion
    echo Codigo de error: %ERRORLEVEL%
    echo ========================================
)

echo.
pause
