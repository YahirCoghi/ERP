# Guía de Desarrollo - MiniERP

## Arquitectura del Sistema

### Backend (.NET 8)
```
MiniERP/
├── MiniERP.API/          # Capa de presentación (Controllers, Middleware)
├── MiniERP.Application/  # Lógica de aplicación (DTOs, Interfaces, Validators)
├── MiniERP.Domain/       # Entidades de dominio y lógica de negocio
└── MiniERP.Infrastructure/ # Acceso a datos, servicios externos
```

### Frontend (React + TypeScript)
```
MiniERP.Web/
├── src/
│   ├── components/    # Componentes reutilizables
│   ├── pages/         # Páginas de la aplicación
│   ├── services/      # Servicios de API
│   ├── contexts/      # Contextos de React
│   └── App.tsx        # Componente principal
```

## Módulos Implementados

### 1. Contabilidad
- **Entidades:** Account, JournalEntry, JournalEntryLine
- **Funcionalidades:**
  - Plan de cuentas jerárquico
  - Asientos contables
  - Balance general
  - Estados financieros

### 2. Inventario
- **Entidades:** Item, ItemCategory, Warehouse, ItemWarehouse, StockMovement
- **Funcionalidades:**
  - Gestión de artículos
  - Control de stock por almacén
  - Movimientos de inventario
  - Valoración de inventario

### 3. Ventas
- **Entidades:** Customer, SalesOrder, Invoice
- **Funcionalidades:**
  - Gestión de clientes
  - Cotizaciones y pedidos
  - Facturación
  - Integración con factura electrónica (preparado)

### 4. Compras
- **Entidades:** Supplier, PurchaseOrder
- **Funcionalidades:**
  - Gestión de proveedores
  - Órdenes de compra
  - Recepción de mercancía

### 5. CRM
- **Entidades:** Lead, Opportunity, Activity
- **Funcionalidades:**
  - Gestión de prospectos
  - Oportunidades de negocio
  - Seguimiento de actividades

### 6. Recursos Humanos
- **Entidades:** Employee, Department, Position, Payroll, Attendance
- **Funcionalidades:**
  - Gestión de empleados
  - Nómina
  - Control de asistencia

### 7. Producción
- **Entidades:** BillOfMaterials, ProductionOrder
- **Funcionalidades:**
  - Lista de materiales (BOM)
  - Órdenes de producción
  - Control de procesos

## Características Técnicas

### Seguridad
- Autenticación JWT
- ASP.NET Identity para gestión de usuarios
- Roles y permisos
- CORS configurado

### Auditoría
- Registro automático de cambios
- Tracking de usuario y fecha
- Historial completo de modificaciones

### Multi-moneda
- Soporte para múltiples monedas
- Tipos de cambio configurables
- Conversión automática

### API REST
- Documentación Swagger
- Endpoints RESTful
- Validación de datos
- Manejo de errores

## Agregar Nuevos Módulos

### 1. Crear Entidades de Dominio
```csharp
// En MiniERP.Domain/Entities/NuevoModulo/
public class MiEntidad : BaseEntity
{
    public string Nombre { get; set; }
    // ... propiedades
}
```

### 2. Agregar al DbContext
```csharp
// En ApplicationDbContext.cs
public DbSet<MiEntidad> MisEntidades { get; set; }
```

### 3. Crear Migración
```bash
dotnet ef migrations add AgregarMiEntidad
dotnet ef database update
```

### 4. Crear Controller
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MisEntidadesController : ControllerBase
{
    // ... implementación
}
```

### 5. Crear Servicio Frontend
```typescript
// En src/services/miEntidadService.ts
export const miEntidadService = {
    getAll: () => api.get('/mientidades'),
    // ... métodos
};
```

## Personalización

### Cambiar Tema
Modificar `src/MiniERP.Web/src/App.tsx`:
```typescript
const theme = createTheme({
  palette: {
    primary: { main: '#tu-color' },
  },
});
```

### Agregar Idiomas
1. Instalar i18next
2. Crear archivos de traducción
3. Configurar en App.tsx

### Configurar Factura Electrónica (Costa Rica)
1. Implementar servicio de integración con Hacienda
2. Agregar certificados digitales
3. Configurar endpoints de Hacienda

## Testing

### Backend
```bash
# Crear proyecto de pruebas
dotnet new xunit -n MiniERP.Tests
dotnet add reference ../MiniERP.API
```

### Frontend
```bash
npm install --save-dev @testing-library/react
npm test
```

## Deployment

### Producción
1. Configurar `appsettings.Production.json`
2. Compilar: `dotnet publish -c Release`
3. Frontend: `npm run build`
4. Configurar IIS o servidor web

### Docker (Opcional)
```dockerfile
# Crear Dockerfile para backend y frontend
# Usar docker-compose para orquestar
```

## Mejores Prácticas

1. **Siempre usar migraciones** para cambios en BD
2. **Validar datos** en frontend y backend
3. **Manejar errores** apropiadamente
4. **Documentar** cambios importantes
5. **Hacer commits** frecuentes y descriptivos

## Recursos Adicionales

- [Documentación .NET](https://docs.microsoft.com/dotnet/)
- [React Docs](https://react.dev/)
- [Material-UI](https://mui.com/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
