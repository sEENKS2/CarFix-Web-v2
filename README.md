# CarFix Web

Sistema de gestión para talleres mecánicos que centraliza la atención de clientes, el seguimiento de trabajos, las compras y el control de inventario. Integra la agenda de turnos con los tickets de taller, la emisión de comprobantes y el registro de cobros.

## Funcionalidades

- **Clientes y vehículos:** registro de titulares, unidades y kilometraje, con consulta del historial de trabajos.
- **Agenda de turnos:** programación de citas, cancelación y recepción del vehículo con generación de un ticket.
- **Tickets de taller:** descripción de fallas, asignación de técnicos y seguimiento del estado del trabajo.
- **Catálogo de repuestos:** administración de productos, categorías, precios y niveles de stock.
- **Inventario:** consulta de existencias, movimientos de entrada y salida, ajustes de depósito y alertas de reposición.
- **Compras:** emisión de órdenes a proveedores y recepción de mercadería con actualización del stock.
- **Facturación y cobranzas:** comprobantes asociados a tickets, detalle de mano de obra y repuestos, pagos y saldos pendientes.
- **Reportes:** panel de indicadores y exportaciones para consulta y seguimiento de la actividad.
- **Asistencia con IA:** prediagnóstico de síntomas y sugerencias de mantenimiento para apoyar la evaluación del técnico.
- **Usuarios y seguridad:** acceso por roles, administración de cuentas y consulta de auditoría.

Las vistas de catálogo, existencias, movimientos, compras y facturación incorporan búsqueda o paginación según el módulo. Los selectores permiten buscar productos por código y nombre.

## Arquitectura y tecnologías

| Componente | Tecnologías | Responsabilidad |
|---|---|---|
| Frontend | React 19, Vite, Axios | Interfaz de usuario y comunicación con la API |
| API | ASP.NET Core 8, JWT | Endpoints HTTP, autenticación y autorización |
| Lógica de negocio | C# / .NET 8 | Operaciones del taller y reglas de gestión |
| Persistencia | Entity Framework Core 8, SQL Server | Acceso a datos y migraciones del esquema |
| IA local | Ollama | Asistencia de diagnóstico y mantenimiento |

```text
CarFix-Web/
├── Backend/CarFix.API/
│   ├── Controllers/        Endpoints de la API
│   ├── DTOs/               Contratos de entrada y salida
│   ├── Security/           Autenticación y administración inicial
│   ├── Services/           Servicios de la API e integración con IA
│   └── src/
│       ├── Controladora/   Lógica de negocio
│       ├── Entidades/      Modelo de dominio
│       └── Modelo/         Contexto de datos y migraciones
├── Frontend/               Aplicación React
├── tests/                  Pruebas del backend
├── scripts/                Configuración local y verificaciones
└── docs/                   Documentación técnica
```

## Requisitos

- .NET SDK 8.
- Node.js 22.12 o superior y npm.
- SQL Server y una cuenta con permisos para crear la base y aplicar migraciones.
- PowerShell en Windows para el asistente de configuración local.
- Python 3 para ejecutar el control de patrones sensibles.

La asistencia con IA es opcional. Para utilizar el proveedor local, se requiere Ollama con el modelo `llama3:8b`, disponible en `localhost:11434`.

## Instalación local

Los comandos siguientes se ejecutan desde la raíz del proyecto en PowerShell, salvo donde se indique otra carpeta.

### 1. Configurar la conexión y la autenticación

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./scripts/Configurar-Local.ps1
```

El asistente solicita la instancia de SQL Server y el nombre de la base, utiliza autenticación integrada de Windows y genera una clave JWT aleatoria de 64 bytes. La conexión y la clave se guardan en **.NET User Secrets**, fuera del repositorio, sin imprimir sus valores.

Para una instalación independiente, indicar un nombre de base nuevo. Antes de actualizar una base existente, revisar su esquema y guardar un respaldo fuera del repositorio.

También se pueden suministrar `ConnectionStrings__CarFix` y `Jwt__Key` mediante variables de entorno. El backend no carga archivos `.env` automáticamente. User Secrets se utiliza para desarrollo local; la configuración de otros entornos debe gestionarse externamente.

### 2. Restaurar herramientas y aplicar migraciones

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet tool restore
dotnet restore Backend/CarFix.API
dotnet ef database update --project Backend/CarFix.API/src/Modelo --startup-project Backend/CarFix.API
```

Las migraciones incluidas crean y actualizan el esquema. No es necesario generar una migración para instalar el proyecto.

### 3. Crear el administrador

En una terminal interactiva:

```powershell
dotnet run --project Backend/CarFix.API -- --bootstrap-admin
```

Ingresar un nombre de usuario nuevo, un correo y una contraseña de entre 12 y 128 caracteres. La contraseña se solicita de forma oculta y debe confirmarse. No hay credenciales predeterminadas.

Desde **Usuarios / Seguridad**, el administrador puede crear cuentas para operadores y técnicos o restablecer sus contraseñas.

### 4. Configurar el frontend

Crear `Frontend/.env.local` con este contenido:

```dotenv
VITE_API_URL=https://localhost:7002/api
```

Las variables `VITE_` son públicas y se incorporan al cliente del navegador. No deben contener claves ni contraseñas.

### 5. Iniciar la aplicación

En una terminal, iniciar el backend:

```powershell
dotnet dev-certs https --trust
dotnet run --project Backend/CarFix.API --launch-profile https
```

En otra terminal, instalar las dependencias e iniciar el frontend:

```powershell
cd Frontend
npm.cmd ci
npm.cmd run dev
```

En otros sistemas, utilizar `npm` en lugar de `npm.cmd`.

| Acceso | Dirección |
|---|---|
| Aplicación web | http://localhost:5173 |
| API | https://localhost:7002/api |
| Swagger en Development | https://localhost:7002/swagger |

Mantener ambas terminales abiertas durante el uso. Para detener los servidores, presionar `Ctrl+C` en cada terminal. Si un puerto está ocupado, comprobar si la aplicación ya está ejecutándose antes de iniciar otra instancia.

## Flujo de trabajo

1. Registrar el cliente y su vehículo.
2. Agendar un turno o crear un ticket directamente.
3. Asignar el técnico y actualizar el estado del trabajo.
4. Consultar repuestos y gestionar su reposición cuando corresponda.
5. Emitir el comprobante del ticket con mano de obra y repuestos utilizados.
6. Registrar los cobros y consultar los saldos pendientes.

## Roles y permisos

| Operación | Administradores | Operadores | Técnicos |
|---|---|---|---|
| Usuarios, restablecimientos y auditoría | Sí | No | No |
| Tickets, historial y prediagnóstico | Sí | Sí | Sí |
| Consulta de catálogo y datos de apoyo al ticket | Sí | Sí | Sí |
| Cambios en catálogo, clientes, vehículos y técnicos | Sí | Sí | No |
| Compras, inventario, turnos, facturación y reportes | Sí | Sí | No |

Los permisos se verifican en la API. Las sesiones utilizan JWT con una duración predeterminada de 60 minutos; restablecer la contraseña, desactivar la cuenta o cambiar sus grupos invalida los tokens anteriores.

## Pruebas y compilación

Desde la raíz:

```powershell
python scripts/check_secrets.py
dotnet test tests/CarFix.Security.Tests/CarFix.Security.Tests.csproj -m:1
dotnet build Backend/CarFix.API
```

Para el frontend:

```powershell
cd Frontend
npm.cmd test
npm.cmd run build
```

Las pruebas cubren autenticación, permisos, manejo de contraseñas, conservación de teléfonos, contratos de rutas y servicios del frontend, tratamiento de errores y paginación. Las pruebas del backend utilizan EF InMemory; la integración completa con SQL Server se verifica por separado.

## Mantenimiento y alcance

- **Recuperación del administrador:** `dotnet run --project Backend/CarFix.API -- --reset-admin` permite restablecer una cuenta existente y asignarle el rol Administradores desde una terminal interactiva. Requiere acceso local a la configuración y a la base.
- **Datos iniciales:** las migraciones incluyen registros de demostración y cuentas deshabilitadas. Crear las cuentas de acceso mediante las herramientas de administración.
- **Comprobantes:** el módulo registra facturación y cobranzas internas del taller. La emisión fiscal requiere una integración específica con el organismo correspondiente.
- **Asistencia con IA:** las sugerencias requieren revisión y confirmación del técnico responsable.
- **Volumen de datos:** la paginación actual se realiza en el frontend. Para catálogos de gran volumen, se requiere extender la API con búsqueda y paginación en el servidor.
- **Despliegue:** la configuración incluida utiliza hosts, CORS y certificados de desarrollo local. Antes de desplegar, adaptar la infraestructura y revisar los pendientes técnicos y de seguridad descritos en [docs/SEGURIDAD.md](docs/SEGURIDAD.md).

La configuración privada, las claves, los certificados y los respaldos deben mantenerse fuera del repositorio. La documentación de seguridad detalla el tratamiento de credenciales, las actualizaciones de bases anteriores y el alcance de las verificaciones realizadas.
