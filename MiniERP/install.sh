#!/bin/bash

echo "========================================"
echo "MiniERP - Script de Instalación"
echo "========================================"
echo ""

echo "[1/6] Verificando requisitos..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET 8 SDK no está instalado"
    echo "Descárgalo de: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js no está instalado"
    echo "Descárgalo de: https://nodejs.org/"
    exit 1
fi

echo "OK: Requisitos verificados"
echo ""

echo "[2/6] Restaurando paquetes del backend..."
cd src/MiniERP.API
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Falló al restaurar paquetes"
    exit 1
fi
echo ""

echo "[3/6] Instalando herramienta Entity Framework..."
dotnet tool install --global dotnet-ef
echo ""

echo "[4/6] Creando base de datos..."
dotnet ef database update --project ../MiniERP.Infrastructure --startup-project .
if [ $? -ne 0 ]; then
    echo "ERROR: Falló al crear la base de datos"
    echo "Verifica que SQL Server esté ejecutándose"
    exit 1
fi
echo ""

echo "[5/6] Instalando dependencias del frontend..."
cd ../MiniERP.Web
npm install
if [ $? -ne 0 ]; then
    echo "ERROR: Falló al instalar dependencias de npm"
    exit 1
fi
echo ""

echo "[6/6] Instalación completada!"
echo ""
echo "========================================"
echo "Para ejecutar el sistema:"
echo ""
echo "Backend:  cd src/MiniERP.API && dotnet run"
echo "Frontend: cd src/MiniERP.Web && npm run dev"
echo ""
echo "Usuario: admin@minierp.com"
echo "Password: Admin123!"
echo "========================================"
