# MiniERP - Sistema de Gestion Empresarial

Sistema ERP modular para pequenas empresas desarrollado en .NET 8 y React.

## Modulos incluidos
- Contabilidad: plan de cuentas y asientos contables.
- Inventario: control de stock y movimientos.
- Ventas: pedidos y facturacion.
- Compras: ordenes de compra y proveedores.
- Multi-tenant: tenants, licencias y onboarding.

## Tecnologias
- Backend: ASP.NET Core 8 Web API
- Frontend: React 18 + TypeScript + Vite
- Base de datos: SQL Server
- ORM: Entity Framework Core 8
- Autenticacion: JWT + BCrypt

## Instalacion rapida

### Backend
```bash
cd src/MiniERP.API
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Estructura
```text
MiniERP/
  src/
    MiniERP.API/
    MiniERP.Domain/
    MiniERP.Application/
    MiniERP.Infrastructure/
  frontend/
```
