# Manual de Usuario Final - NEX MiniERP

## 1. Acceso al sistema
1. Abrir `http://localhost:3000`.
2. Ingresar usuario y contraseña.
3. Confirmar tenant (empresa) correcto.
4. Verificar nombre y rol en la parte superior derecha.

## 2. Estructura general del sistema
- `Dashboard`: resumen operativo.
- `Ventas`: clientes, ordenes de venta, facturacion.
- `Compras`: proveedores, ordenes de compra, facturas de compra.
- `Inventario`: productos y movimientos.
- `Finanzas`: facturas, tipos de cambio, reportes.
- `Administracion`: usuarios, tenants, suscripcion, licencias y configuraciones avanzadas (segun rol).

## 3. Flujo operativo recomendado

### 3.1 Ventas (diario)
1. Crear o actualizar cliente.
2. Crear orden de venta.
3. Agregar lineas (producto, cantidad, precio, descuento).
4. Guardar orden.
5. Generar factura de venta.

Resultado:
- Se registra la venta.
- Se descuenta inventario al facturar.

### 3.2 Compras (diario)
1. Crear o actualizar proveedor.
2. Crear orden de compra.
3. Agregar lineas de productos.
4. Registrar recepcion de mercaderia.
5. Registrar factura de compra.
6. Registrar pagos parciales o totales.

Resultado:
- La recepcion aumenta inventario.
- Se actualiza saldo por pagar.

### 3.3 Inventario (diario)
1. Revisar movimientos.
2. Buscar por producto o referencia.
3. Validar entradas/salidas del dia.

## 4. Uso por modulo

### 4.1 Clientes
- `+ Add Customer` para crear.
- Completar datos y guardar.
- `Edit` para modificar.

### 4.2 Proveedores
- Igual a clientes, enfocado en compras.
- Mantener datos de contacto/fiscales actualizados.

### 4.3 Productos
- Registrar codigo, nombre, costo, precio y stock.
- Se usa en ventas, compras e inventario.

### 4.4 Ordenes de venta
- Seleccionar cliente.
- Agregar lineas.
- Verificar impuesto y total.
- Guardar y facturar.

### 4.5 Ordenes de compra
- Seleccionar proveedor.
- Agregar lineas.
- Guardar.
- Recibir para impactar inventario.

### 4.6 Facturas de venta
- Consultar facturas emitidas.
- Revisar estado.
- Aplicar nota de credito cuando corresponda.

### 4.7 Facturas de compra
- Registrar factura del proveedor.
- Revisar `PaidAmount`, `Balance`, `Status`.

## 5. Finanzas

### 5.1 Tipos de cambio
- Registrar fecha, moneda origen/destino y tasa.

### 5.2 Dashboard financiero
- Indicadores de debitos/creditos, ventas, compras, alertas y aprobaciones.

### 5.3 Plantillas recurrentes
- Ejecutar asientos periodicos predefinidos.

## 6. Funciones avanzadas (segun plan y rol)
- `Activos fijos`: alta y depreciacion.
- `Intrastat`: declaraciones y exportacion.
- `Campanas`: segmentacion y ejecucion.
- `Pick Pack`: tareas logisticas.
- `Knowledge Base`: articulos de soporte.
- `Add-ons`, `Workflows`, `Licencias`: administracion avanzada.

## 7. Buenas practicas
- Revisar impuestos y totales antes de guardar documentos.
- Confirmar tenant correcto antes de operar.
- No compartir credenciales.
- Usar perfiles por rol (no usar Owner para tareas operativas diarias).
- Ejecutar cierres periodicos y validar saldos.

## 8. Mensajes comunes y solucion
- `401`: sesion/token vencido. Reingresar.
- `402`: suscripcion inactiva del tenant.
- `403`: falta permiso de rol o plan.
- `Error saving ...`: validar campos requeridos y conectividad API.

## 9. Lista rapida de verificacion (si algo falla)
1. Backend activo en `http://localhost:5000`.
2. Frontend activo en `http://localhost:3000`.
3. Tenant correcto (`tenantId`).
4. Usuario con rol y plan adecuados.
5. Base de datos del tenant con migraciones al dia.
