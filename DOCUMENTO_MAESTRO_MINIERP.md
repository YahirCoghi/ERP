# MiniERP - Documento Maestro del Proyecto

## 1) Resumen Ejecutivo
MiniERP es una plataforma ERP modular orientada a pymes, con enfoque en:
- Operación diaria (ventas, compras, inventario, facturación).
- Control financiero básico (cuentas por cobrar y cuentas por pagar operativas).
- Escalabilidad SaaS multi-tenant para crecer a múltiples empresas clientes.

El sistema está diseñado para correr en arquitectura web moderna:
- Backend en `.NET 8` (API REST).
- Frontend en `React + Vite`.
- Base de datos principal en `SQL Server`.

El objetivo de negocio es vender MiniERP como software de suscripción mensual (SaaS), con capacidad de administrar múltiples empresas (tenants), cada una con sus datos separados.

---

## 2) Objetivos del Producto
Objetivos estratégicos:
- Digitalizar el ciclo comercial completo de una pyme.
- Reducir errores operativos (códigos automáticos, flujos guiados).
- Preparar una base tecnológica lista para escalar a SaaS empresarial.
- Integrar cumplimiento fiscal de Costa Rica (factura electrónica 4.4) por fases.

Objetivos operativos actuales:
- Gestionar órdenes de venta y su facturación.
- Gestionar órdenes de compra, recepción de mercadería y facturas de proveedor.
- Controlar movimientos de inventario de entrada/salida.
- Controlar estado de cobro y pago en documentos clave.

---

## 3) Alcance Funcional Actual
### 3.1 Ventas
- Módulo de clientes.
- Creación de órdenes de venta.
- Facturación desde orden de venta.
- Notas de crédito desde factura de venta.
- Estados de orden y factura para seguimiento.

### 3.2 Compras
- Módulo de proveedores.
- Creación de órdenes de compra.
- Recepción de compra (impacta inventario).
- Factura de compra (Purchase Invoice / Bill) desde orden.
- Registro de pagos a factura de compra, con saldo y estado (`Partial`/`Paid`).

### 3.3 Inventario
- Catálogo de productos.
- Movimientos automáticos:
  - Salida por facturación de venta.
  - Entrada por recepción de compra.

### 3.4 Catálogos y Configuración
- Impuestos.
- Condiciones de venta.
- Métodos de pago.
- Términos de pago.
- Configuración de ventas y compras.

### 3.5 Multi-tenant / Licenciamiento (base)
- Tenant master database.
- Resolución de tenant por header `X-Tenant-ID`.
- Estructura base para licencias/suscripción por cliente.

---

## 4) Arquitectura Técnica
### 4.1 Capas del backend
- `MiniERP.Domain`: entidades y reglas base.
- `MiniERP.Application`: contratos/servicios de aplicación.
- `MiniERP.Infrastructure`: EF Core, repositorios, servicios de infraestructura.
- `MiniERP.API`: controladores REST, autenticación, middleware.

### 4.2 Frontend
- Aplicación SPA en React.
- Routing por módulos funcionales.
- Consumo HTTP con Axios.
- Interceptor para JWT y `X-Tenant-ID`.

### 4.3 Datos
- SQL Server como motor principal.
- `MiniERP_Master`: datos de tenants/licencias.
- `MiniERP` (u otras): datos transaccionales por tenant.

---

## 5) Flujos de Negocio Clave
### 5.1 Ventas (Order to Cash)
1. Crear cliente.
2. Crear orden de venta.
3. Facturar la orden.
4. Reducir inventario automáticamente.
5. Gestionar cobro/estado.
6. Si aplica, emitir nota de crédito (opcional reingreso a stock).

### 5.2 Compras (Procure to Pay)
1. Crear proveedor.
2. Crear orden de compra.
3. Recibir mercadería (`Receive`) para aumentar inventario.
4. Crear factura de compra (`Bill`) desde la orden.
5. Registrar pagos de factura de compra.
6. Controlar saldo pendiente y estado (`Issued`, `Partial`, `Paid`).

---

## 6) Diseño de Datos y Reglas Importantes
### 6.1 Códigos automáticos
Se implementó un generador central de secuencias por entidad (`EntitySequences`) para evitar choques manuales y estandarizar identificadores:
- Productos: `PROD-...`
- Clientes: `CUST-...`
- Proveedores: `SUP-...`
- Orden venta: `SO-...`
- Orden compra: `PO-...`
- Factura venta: `INV-...`
- Nota crédito: `NC-...`
- Factura compra: `PINV-...`

### 6.2 Integridad operativa
- Orden de compra no aumenta inventario por sí sola.
- Solo `Receive` aumenta inventario.
- Factura de venta reduce inventario.
- Pago de factura de compra actualiza `PaidAmount`, `Balance`, `LastPaymentDate`.

---

## 7) Estado de SaaS y Multi-empresa
Base implementada:
- Tenants y configuración por empresa.
- Provisionamiento de DB por tenant (estructura inicial).
- Resolución en runtime de conexión por tenant.
- Fundamento para control de licencia/estado de suscripción.

Pendiente para nivel comercial SaaS completo:
- Portal público de signup robusto.
- Billing automático (pasarela de pago).
- Automatización completa de creación y lifecycle de tenant.
- Operación de soporte, monitoreo y observabilidad multi-cliente.

---

## 8) Seguridad y Accesos
- Autenticación con JWT.
- Control por roles (`Owner`, `Admin`, `Sales`, `Purchasing`, `Inventory`, `Accounting`).
- Middleware de tenant para aislar contexto de datos.

Recomendaciones de endurecimiento:
- Secretos en Azure Key Vault.
- Rotación de llaves JWT.
- Auditoría de acciones críticas.
- Políticas de backup/restore por tenant.

---

## 9) Roadmap Recomendado
### Fase A - Cierre operativo core
- Historial detallado de pagos (tabla de pagos por factura).
- Reportes operativos (CxC, CxP, inventario valorizado).
- Mejoras UX de edición/cancelación con trazabilidad.

### Fase B - Contabilidad y cumplimiento CR
- Plan de cuentas y asientos automáticos por evento.
- Integración completa Factura Electrónica 4.4 + Hacienda.
- Cierre fiscal y reportes contables formales.

### Fase C - SaaS comercial
- Dominio/subdominios por tenant.
- Portal de registro y onboarding.
- Licenciamiento y cobro recurrente automatizado.
- Panel de administración global (Master).

---

## 10) Propuesta de Valor para Socios/Inversionistas
MiniERP combina:
- Producto funcional hoy (ventas/compras/inventario con flujos reales).
- Base tecnológica preparada para SaaS multi-tenant.
- Camino claro a diferenciación local (cumplimiento fiscal Costa Rica).

Esto permite:
- Arranque con pilotos de clientes reales.
- Monetización temprana por suscripción.
- Escalamiento progresivo sin rediseño completo de plataforma.

---

## 11) Riesgos y Mitigaciones
Riesgos:
- Complejidad de cumplimiento fiscal y cambios normativos.
- Soporte multi-tenant sin procesos de observabilidad maduros.
- Deuda técnica por crecimiento rápido de funcionalidades.

Mitigaciones:
- Roadmap por fases con hitos de calidad.
- Pruebas automáticas y QA funcional por flujo crítico.
- Estándares de release y versionado de migraciones.

---

## 12) Cómo operar localmente (resumen)
Backend:
```powershell
cd C:\Users\yacog\Documents\MiniERP
dotnet run --project src\MiniERP.API
```

Frontend:
```powershell
cd C:\Users\yacog\Documents\MiniERP\frontend
npm run dev
```

URL:
- Frontend: `http://localhost:3000`
- API: `http://localhost:5000`

---

## 13) Conclusión
MiniERP ya es una base sólida de ERP operativo con visión SaaS.  
El siguiente salto de valor está en:
- Profundizar contabilidad y cumplimiento fiscal CR.
- Formalizar operación SaaS (onboarding, billing, soporte, monitoreo).
- Estandarizar producto para escalar comercialmente con seguridad.


---

## 14) Modulos avanzados incorporados (fase de cierre)
Se agregaron funcionalidades adicionales para cubrir el bloque pendiente de administracion avanzada, finanzas especializadas, CRM avanzado, operaciones de produccion, servicio y herramientas generales.

### 14.1 Administracion avanzada
- Plantillas de aprobacion por modulo con etapas (`ApprovalTemplate`, `ApprovalTemplateStage`) y reporte de aprobaciones.
- Gestor de add-ons con definiciones y activaciones por tenant (`AddonDefinition`, `AddonActivation`).
- Configuracion de servicio/licencia movil (`MobileServiceConfig`).
- Workflow Manager con definiciones, pasos e instancias (`WorkflowDefinition`, `WorkflowStep`, `WorkflowInstance`).
- Gestion de licencias operativas por tenant/usuario (`TenantLicenseAssignment`).

### 14.2 Finanzas especializadas
- Edicion de montos 1099 por proveedor y periodo (`Vendor1099Amount`).
- Activos fijos y depreciacion mensual (`FixedAsset`, `FixedAssetDepreciation`).
- Intrastat con declaracion, lineas y exportacion CSV (`IntrastatDeclaration`, `IntrastatDeclarationLine`).

### 14.3 CRM avanzado
- Campanas con wizard de segmentacion, programacion y ejecucion (`Campaign`, `CampaignRecipient`).

### 14.4 Produccion y logistica avanzada
- Wizard de confirmacion de aprovisionamiento (`ProcurementConfirmation`, `ProcurementConfirmationLine`).
- Pick Pack & Production tracking (`PickPackTask`, `PickPackTaskLine`).

### 14.5 Servicio
- Base de conocimiento para incidencias (`KnowledgeBaseArticle`).
- Metricas de SLA y reporte agregado de servicio (`ServiceSlaMetric`).

### 14.6 Herramientas generales
- Query Manager persistente (`QueryDefinition`).
- Plantillas de impresion (`PrintLayoutTemplate`).
- Campos y objetos definidos por usuario (`UserDefinedField`, `UserDefinedObject`).

### 14.7 Licenciamiento por modulo
El sistema de planes se amplio para incluir nuevos modulos:
- `Addons`, `Mobile`, `WorkflowManager`, `LicenseManagement`
- `FixedAssets`, `Intrastat`, `Forms1099`
- `Campaigns`
- `ProcurementWizard`, `PickPack`
- `KnowledgeBase`, `ServiceReports`
- `PrintLayouts`, `UserDefinedObjects`

Estos modulos se exponen via politicas dinamicas (`Modulo:Read|Full`) y el frontend respeta la visibilidad por plan/rol.
