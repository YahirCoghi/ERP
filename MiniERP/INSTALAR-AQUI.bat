@echo off
echo ========================================
echo Instalando dependencias de MiniERP
echo ========================================
echo.

cd /d "C:\Users\yacog\AppData\Roaming\AbacusAI\Agent Workspaces\MiniERP\MiniERP\src\MiniERP.Web"

if not exist "package.json" (
    echo ERROR: No se encontro el archivo package.json
    echo Verifica que la ruta sea correcta
    pause
    exit /b 1
)

echo Directorio actual: %CD%
echo.

echo Verificando Node.js...
"C:\Program Files\nodejs\node.exe" --version
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Node.js no esta instalado
    echo Descargalo desde: https://nodejs.org/
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
echo Instalando paquetes npm...
echo Esto puede tomar 5-10 minutos, por favor espera...
echo.

"C:\Program Files\nodejs\npm.cmd" install

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo INSTALACION COMPLETADA EXITOSAMENTE!
    echo ========================================
    echo.
    echo Los errores de TypeScript deberian desaparecer ahora.
    echo.
    echo Para iniciar el frontend, ejecuta:
    echo   npm run dev
) else (
    echo.
    echo ========================================
    echo ERROR DURANTE LA INSTALACION
    echo Codigo de error: %ERRORLEVEL%
    echo ========================================
)

echo.
pause
