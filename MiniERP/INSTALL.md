# Guía de Instalación - MiniERP

## Requisitos Previos

### Software Necesario
1. **.NET 8 SDK**
   - Descargar de: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verificar instalación: `dotnet --version`

2. **SQL Server 2019+ o SQL Server Express (GRATIS)**
   - SQL Server Express: https://www.microsoft.com/es-es/sql-server/sql-server-downloads
   - O usar SQL Server LocalDB (incluido con Visual Studio)

3. **Node.js 18+**
   - Descargar de: https://nodejs.org/
   - Verificar instalación: `node --version` y `npm --version`

4. **Visual Studio 2022 o VS Code** (Opcional pero recomendado)
   - Visual Studio Community (GRATIS): https://visualstudio.microsoft.com/
   - VS Code: https://code.visualstudio.com/

## Instalación Paso a Paso

### 1. Clonar o Descargar el Proyecto
```bash
cd MiniERP
```

### 2. Configurar Base de Datos

#### Opción A: SQL Server Express (Recomendado para iniciar)
1. Instalar SQL Server Express
2. La cadena de conexión por defecto ya está configurada en `appsettings.json`

#### Opción B: SQL Server LocalDB
Modificar `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniERP;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Instalar Dependencias del Backend

```bash
cd src/MiniERP.API
dotnet restore
```

### 4. Crear Base de Datos y Ejecutar Migraciones

```bash
# Desde la carpeta src/MiniERP.API
dotnet ef migrations add InitialCreate --project ../MiniERP.Infrastructure --startup-project .
dotnet ef database update --project ../MiniERP.Infrastructure --startup-project .
```

Si no tienes `dotnet ef` instalado:
```bash
dotnet tool install --global dotnet-ef
```

### 5. Ejecutar el Backend

```bash
# Desde src/MiniERP.API
dotnet run
```

El API estará disponible en: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### 6. Instalar Dependencias del Frontend

```bash
cd ../MiniERP.Web
npm install
```

### 7. Ejecutar el Frontend

```bash
npm run dev
```

El frontend estará disponible en: `http://localhost:3000`

## Credenciales por Defecto

- **Usuario:** admin@minierp.com
- **Contraseña:** Admin123!

## Verificación de Instalación

1. Abrir navegador en `http://localhost:3000`
2. Iniciar sesión con las credenciales por defecto
3. Deberías ver el dashboard principal

## Solución de Problemas Comunes

### Error: "Cannot connect to SQL Server"
- Verificar que SQL Server esté ejecutándose
- Verificar la cadena de conexión en `appsettings.json`
- Intentar con SQL Server LocalDB

### Error: "dotnet ef not found"
```bash
dotnet tool install --global dotnet-ef
```

### Error: "npm install fails"
```bash
# Limpiar caché de npm
npm cache clean --force
npm install
```

### Puerto 5000 o 3000 ya en uso
Modificar los puertos en:
- Backend: `Properties/launchSettings.json`
- Frontend: `vite.config.ts`

## Próximos Pasos

1. Cambiar la contraseña del usuario admin
2. Configurar datos de tu empresa en el módulo de configuración
3. Crear usuarios adicionales con diferentes roles
4. Configurar monedas y tipos de cambio
5. Comenzar a usar los módulos según tus necesidades

## Soporte

Para problemas o preguntas, revisar la documentación completa en `/docs`
