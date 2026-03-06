# Fase 2 - Licencias, Cobro y Control de Acceso

## Objetivo
Asegurar que el acceso al sistema dependa del estado comercial del tenant (pago/licencia), con señales visibles para operación y ventas.

## Cambios implementados
- Enforzamiento de suscripción en `TenantResolutionMiddleware`:
  - Detecta vencimiento de trial.
  - Detecta vencimiento de pago (`PaidUntil`) para tenants activos.
  - Sincroniza estado efectivo a `PastDue` cuando corresponde.
  - Bloquea acceso con `402 Payment Required` en estados inactivos (`PastDue`, `Suspended`, `Canceled`).
- Salud del servicio:
  - Excepción para endpoint `/health` en resolución de tenant.
- Límite de usuarios por plan:
  - `AuthController.Register` valida `MaxUsers` del tenant antes de crear usuario.
  - Si excede límite, devuelve `402` con mensaje comercial.
- Endpoint de estado de suscripción:
  - `GET /api/subscription/current`
  - Retorna estado tenant + licencia + consumo de usuarios (`ActiveUsers`, `RemainingUsers`).
- Frontend:
  - Página `Subscription` para owner/admin.
  - Tenant selector enriquecido con plan, fechas y límite usuarios.
  - Interceptor HTTP muestra mensaje de suscripción al recibir `402`.

## Criterios de aceptación
1. Tenant en `PastDue` no puede operar endpoints de negocio.
2. Trial vencido se marca automáticamente como `PastDue`.
3. Tenant activo con `PaidUntil` vencido pasa a `PastDue`.
4. No se pueden registrar usuarios si `ActiveUsers >= MaxUsers`.
5. Owner/Admin puede ver estado comercial actual en `/subscription`.

## Prueba rápida
1. Cambiar `PaidUntil` de tenant a fecha pasada.
2. Invocar cualquier endpoint transaccional con ese tenant.
3. Confirmar respuesta `402` + mensaje de suscripción.
4. Restaurar tenant a `Active` y fecha vigente.
5. Confirmar acceso normal.
