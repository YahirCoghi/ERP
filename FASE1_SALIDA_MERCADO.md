# Fase 1 - Salida a Mercado (Checklist Operativo)

## Objetivo
Tener un producto demostrable y vendible para pilotos, con flujos críticos funcionando sin errores bloqueantes:
- Ventas: orden -> factura -> nota de crédito.
- Compras: orden -> recepción -> factura de compra -> pago.
- Inventario: entradas/salidas consistentes con operaciones.

## Alcance técnico cerrado en esta fase
- UI de Ventas y Compras con acciones funcionales (`View`, `Edit`, `Invoice`, `Receive`, `Bill`, `Pay`).
- Validaciones backend para evitar errores 500 en creación/actualización.
- Mensajes de error útiles en frontend para diagnóstico rápido comercial.
- Códigos automáticos para entidades operativas.

## Criterios de aceptación funcional
### 1) Venta completa
1. Crear cliente.
2. Crear orden de venta con al menos 1 línea.
3. Facturar orden desde `Sales Orders`.
4. Ver factura en `Invoices`.
5. Crear nota de crédito desde factura.

Resultado esperado:
- Orden queda en estado `Invoiced`.
- Factura creada con número `INV-*`.
- Nota crédito creada con `NC-*`.
- Inventario baja al facturar y opcionalmente sube con nota crédito (`Restock`).

### 2) Compra completa
1. Crear proveedor.
2. Crear orden de compra con líneas.
3. Recibir mercadería (`Receive`) con cantidades válidas.
4. Crear factura de compra (`Bill`) desde la orden.
5. Registrar pago parcial y luego total en `Purchase Bills`.

Resultado esperado:
- Estado de orden cambia (`PartiallyReceived`/`Received`/`Billed`).
- Factura de compra creada con `PINV-*`.
- `PaidAmount`, `Balance` y `LastPaymentDate` se actualizan.
- Estado de factura compra cambia a `Partial` y luego `Paid`.

## Script de pruebas manuales (smoke)
1. Login con usuario owner/admin.
2. Crear 1 producto, 1 cliente, 1 proveedor.
3. Ejecutar flujo completo de venta.
4. Ejecutar flujo completo de compra.
5. Verificar que no haya alerts genéricos sin detalle.
6. Verificar movimientos de inventario en módulo de inventario.

## Riesgos conocidos de Fase 1
- Sin automatización e2e completa todavía (manual QA requerido por release).
- Falta histórico detallado de pagos por factura (solo saldo acumulado).
- Integración fiscal CR 4.4 no está en modo productivo aún.

## Operación recomendada de demo comercial
1. Preparar tenant demo con datos de ejemplo.
2. Mostrar flujo de ventas de inicio a fin.
3. Mostrar flujo de compras y cuentas por pagar.
4. Mostrar inventario impactado en tiempo real.
5. Cerrar con valor SaaS multi-tenant + roadmap fiscal.

## Definición de “listo para piloto”
- Sin errores 500 en flujos críticos.
- Roles y permisos básicos funcionando.
- Datos persisten y estados evolucionan correctamente.
- Se puede ejecutar demo completa en menos de 20 minutos.
