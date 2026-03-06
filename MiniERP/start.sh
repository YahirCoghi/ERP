#!/bin/bash

echo "Iniciando MiniERP Backend..."
cd src/MiniERP.API
dotnet run &
BACKEND_PID=$!

sleep 3

echo "Iniciando MiniERP Frontend..."
cd ../MiniERP.Web
npm run dev &
FRONTEND_PID=$!

echo ""
echo "========================================"
echo "MiniERP iniciado correctamente"
echo ""
echo "Backend:  http://localhost:5000"
echo "Frontend: http://localhost:3000"
echo "Swagger:  http://localhost:5000/swagger"
echo ""
echo "Usuario: admin@minierp.com"
echo "Password: Admin123!"
echo ""
echo "Presiona Ctrl+C para detener"
echo "========================================"

trap "kill $BACKEND_PID $FRONTEND_PID" EXIT

wait
