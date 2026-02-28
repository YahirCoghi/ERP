# MiniERP - Sistema de Gestión Empresarial

Sistema ERP modular para pequeñas empresas desarrollado en .NET 8 y React.

## Módulos Incluidos
- 📊 **Contabilidad**: Plan de cuentas, asientos contables, balance general, estados financieros
- 📦 **Inventario**: Control de stock, almacenes, movimientos, valoración
- 💰 **Ventas**: Cotizaciones, pedidos, facturas, clientes
- 🛒 **Compras**: Órdenes de compra, recepción, proveedores
- 👥 **CRM**: Gestión de clientes, oportunidades, seguimiento
- 👔 **Recursos Humanos**: Empleados, nómina, asistencia
- 🏭 **Producción**: Órdenes de producción, BOM, control de procesos

## Tecnologías
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: React 18 + TypeScript + Material-UI
- **Base de Datos**: SQL Server
- **ORM**: Entity Framework Core 8
- **Autenticación**: JWT + ASP.NET Identity

## Características
- ✅ Multi-moneda y multi-idioma
- ✅ Sistema de roles y permisos
- ✅ Auditoría completa de cambios
- ✅ API REST documentada
- ✅ Backup automático
- ✅ Dashboard personalizable
- ✅ Reportes avanzados

## Requisitos
- .NET 8 SDK
- SQL Server 2019+ o SQL Server Express
- Node.js 18+
- Visual Studio 2022 o VS Code

## Instalación

### Backend
```bash
cd src/MiniERP.API
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd src/MiniERP.Web
npm install
npm start
```

## Configuración Inicial
1. Configurar cadena de conexión en `appsettings.json`
2. Ejecutar migraciones de base de datos
3. Usuario admin por defecto: `admin@minierp.com` / `Admin123!`

## Estructura del Proyecto
```
MiniERP/
├── src/
│   ├── MiniERP.API/          # Web API
│   ├── MiniERP.Domain/       # Entidades y lógica de negocio
│   ├── MiniERP.Application/  # Casos de uso y DTOs
│   ├── MiniERP.Infrastructure/ # Acceso a datos y servicios externos
│   └── MiniERP.Web/          # Frontend React
```

## Licencia
Propietario - Todos los derechos reservados
