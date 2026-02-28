# Solución de Problemas - Instalación de Dependencias del Frontend

## Problema Actual

Los errores en `Dashboard.tsx` se deben a que las dependencias de npm no se han instalado correctamente.

## Solución Manual

### Opción 1: Usar el script automático (Recomendado)

1. Abre el Explorador de Archivos
2. Navega a la carpeta `MiniERP`
3. Haz doble clic en `install-frontend.bat`
4. Espera a que termine la instalación (puede tomar 3-5 minutos)

### Opción 2: Instalación manual desde CMD

1. Abre **CMD** (Command Prompt) como Administrador:
   - Presiona `Win + X`
   - Selecciona "Símbolo del sistema (Administrador)" o "Windows PowerShell (Administrador)"

2. Navega al directorio del frontend:
   ```cmd
   cd "C:\Users\yacog\AppData\Roaming\AbacusAI\Agent Workspaces\MiniERP\MiniERP\src\MiniERP.Web"
   ```

3. Instala las dependencias:
   ```cmd
   "C:\Program Files\nodejs\npm.cmd" install
   ```

4. Espera a que termine (verás un spinner animado y mensajes de progreso)

### Opción 3: Habilitar ejecución de scripts en PowerShell

Si prefieres usar PowerShell, necesitas habilitar la ejecución de scripts:

1. Abre PowerShell como Administrador
2. Ejecuta:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```
3. Confirma con `Y`
4. Luego ejecuta:
   ```powershell
   cd "C:\Users\yacog\AppData\Roaming\AbacusAI\Agent Workspaces\MiniERP\MiniERP\src\MiniERP.Web"
   npm install
   ```

## Verificación

Después de la instalación, verifica que se crearon las dependencias:

1. Abre el Explorador de Archivos
2. Navega a: `MiniERP\src\MiniERP.Web\node_modules`
3. Deberías ver cientos de carpetas (React, Material-UI, etc.)

## Problemas Comunes

### "npm no se reconoce como comando"

**Causa:** Node.js no está instalado o no está en el PATH.

**Solución:**
1. Descarga Node.js desde: https://nodejs.org/
2. Instala la versión LTS (recomendada)
3. Reinicia tu terminal/CMD/PowerShell
4. Intenta nuevamente

### "No se puede cargar el archivo npm.ps1"

**Causa:** PowerShell tiene la ejecución de scripts deshabilitada.

**Solución:**
- Usa CMD en lugar de PowerShell, O
- Usa `npm.cmd` en lugar de `npm`, O
- Habilita la ejecución de scripts (ver Opción 3 arriba)

### La instalación se queda "colgada"

**Causa:** npm está descargando muchos paquetes (puede tomar 5-10 minutos).

**Solución:**
- Ten paciencia y espera
- Verifica tu conexión a internet
- Si después de 10 minutos no avanza, presiona `Ctrl+C` y vuelve a intentar

## Próximos Pasos

Una vez instaladas las dependencias:

1. Los errores en `Dashboard.tsx` deberían desaparecer
2. Podrás iniciar el frontend con:
   ```cmd
   npm run dev
   ```
3. El frontend estará disponible en: http://localhost:5173

## Contacto

Si sigues teniendo problemas, por favor proporciona:
- El mensaje de error completo
- La versión de Node.js (`node --version`)
- La versión de npm (`npm --version`)
