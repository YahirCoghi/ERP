@echo off
echo Iniciando MiniERP Backend...
cd src\MiniERP.API
start "MiniERP API" dotnet run

timeout /t 3

echo Iniciando MiniERP Frontend...
cd ..\MiniERP.Web
start "MiniERP Web" npm run dev

echo.
echo ========================================
echo MiniERP iniciado correctamente
echo.
echo Backend:  http://localhost:5000
echo Frontend: http://localhost:3000
echo Swagger:  http://localhost:5000/swagger
echo.
echo Usuario: admin@minierp.com
echo Password: Admin123!
echo ========================================
