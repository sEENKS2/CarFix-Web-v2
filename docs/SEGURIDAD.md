# Corrección de información sensible

Revisión del código público de `sEENKS2/CarFix-Web`, base `fb195f7becb8035d90f04ac897086ae0abcd4a05`. El autor indicó que las credenciales anteriores se usaron exclusivamente de forma local. Este cambio prepara una versión educativa reproducible sin credenciales publicadas; no certifica seguridad para producción.

## Cambios aplicados

- Se retiraron contraseñas de cuentas precargadas, hashes antiguos, salt fijo y clave JWT de respaldo del árbol actual. Las migraciones usan el marcador inválido `RESET_REQUIRED` y cuentas de ejemplo inactivas.
- Contraseñas: PBKDF2-HMAC-SHA256 con 600.000 iteraciones, salt criptográfico aleatorio de 16 bytes, derivación de 32 bytes y comparación en tiempo constante. El formato incluye algoritmo, iteraciones, salt y derivación. No se admite el formato SHA-256 antiguo. Longitud de nuevas contraseñas: 12 a 128 caracteres. El hash se excluye de la serialización JSON.
- JWT: clave externa, mínimo 32 bytes, HS256, emisor/audiencia validados y vencimiento de 60 minutos por defecto. La ausencia de configuración impide arrancar. La versión de credenciales se calcula con HMAC sobre el hash de contraseña y los grupos; no se incluye el hash de contraseña en el token. Cada petición comprueba el usuario activo y esa versión.
- Autenticación exigida por defecto en la API; sólo el login es anónimo. Administración y auditoría requieren Administradores. Se aplican roles a los demás controladores y a las escrituras de datos compartidos. El login permite cinco solicitudes por minuto por dirección IP; no hay cola.
- Identidad de auditoría aislada por contexto asíncrono, en lugar de compartir el último usuario autenticado entre todas las solicitudes.
- Se eliminó la sesión ficticia y el acceso administrativo por llamarse `admin` en el frontend. El navegador restaura la sesión consultando la API y elimina la sesión al recibir 401. Las rutas tienen controles de rol adicionales a los menús.
- Conexión SQL externa, sin registro de parámetros sensibles. La configuración de ejemplo tiene los valores privados vacíos. Se incluye un asistente PowerShell que genera la clave JWT y la guarda por stdin con User Secrets, fuera de Git.
- Los datos de ejemplo del código y las migraciones son sintéticos: nombres con Demo/Ejemplo, correos `example.invalid`, DNI de prueba y dominios de vehículos DEMO. No se consideran datos personales reales; no utilizarlos como datos de un cliente.
- Gemini, que sigue deshabilitado como proveedor predeterminado, envía su clave en el encabezado `x-goog-api-key` y no en la URL. El modelo se configura externamente. Los errores HTTP no devuelven cuerpos del proveedor, excepciones SQL ni detalles internos.
- Se incorporó una comprobación ligera de patrones sensibles y un flujo de CI para pruebas/compilación. La comprobación analiza archivos actuales; no hace análisis de entropía ni sustituye una revisión del historial.

## Instalaciones nuevas y existentes

Una instalación nueva aplica todas las migraciones saneadas y crea el administrador con `--bootstrap-admin`. Una base existente aplica la migración `SeguridadYDatosDemostracion`, que bloquea todos los hashes antiguos y requiere recuperar el administrador mediante `--reset-admin`; luego el administrador restablece las demás cuentas. Ese bloqueo no se deshace al revertir la migración: nunca se restauran las contraseñas anteriores.

Los registros de negocio de una base existente no se anonimizan automáticamente. Si se va a presentar una base, usar una base nueva con datos ficticios o realizar una revisión específica de su contenido. Los respaldos y archivos privados deben quedar fuera de Git y del ZIP entregado.

La migración también incorpora al esquema los cambios de Turnos, Kilometraje y Factura/Cliente que ya existían en las clases. Las relaciones de Turnos usan restricción de borrado para evitar rutas múltiples de cascada. Revisar bases con modificaciones manuales antes de aplicar esta migración.

## Validación realizada

- Compilación .NET 8 correcta.
- 24 pruebas automatizadas de seguridad correctas: hash/salt, contraseñas inválidas, no serialización del hash, aislamiento de identidad, acceso anónimo, roles, autenticación válida/inválida, expiración/firma/emisor, revocación por cambio de contraseña o desactivación, alta de usuarios y limitación de intentos.
- Dos pruebas del frontend correctas: ausencia de privilegios por nombre de usuario y rechazo de sesiones ficticias/vencidas.
- Compilación de producción del frontend correcta.
- Modelo EF sin cambios pendientes respecto de las migraciones y generación del script SQL idempotente correcta.
- Comprobación de patrones del árbol actual sin hallazgos.

Las pruebas HTTP usan EF InMemory. No se aplicaron migraciones sobre un SQL Server real, no se verificó la instalación en la computadora del autor, no se ejecutó un ensayo de carga y no se invocaron servicios reales de IA. Permanecen advertencias previas de nulabilidad/precisión decimal y de tamaño del bundle del frontend. Los demás defectos funcionales del taller quedan fuera de esta corrección.

## Historial publicado y presentación

El commit correctivo de una rama basada en el repositorio original conserva los commits viejos. Por ese motivo, el ZIP de presentación excluye `.git`, configuraciones privadas, dependencias instaladas, salidas de compilación, bases y respaldos. Contiene únicamente las fuentes corregidas, documentación y pruebas.

La copia Git de historial limpio preparada para revisión contiene un único commit con estas fuentes corregidas; no contiene los commits anteriores. No se publicó ni se reemplazó `main`. Actualizar GitHub requiere la conexión de la cuenta; sustituir el historial remoto requiere además una decisión expresa del propietario, porque cambia los identificadores de commits. Reescribir una rama no elimina copias que terceros ya hayan descargado.

## Pendientes antes de una implementación real

La arquitectura todavía conserva varias controladoras de negocio singleton con contextos EF de larga duración; corresponde migrarlas a servicios por solicitud antes de asegurar concurrencia de producción. La sesión usa sessionStorage, accesible a JavaScript; se requiere prevención de XSS y evaluar cookies HttpOnly si se cambia el esquema de autenticación. Falta una estrategia de recuperación de cuentas por canal verificado, auditoría exhaustiva de accesos fallidos, gestión centralizada de secretos y una evaluación de seguridad completa del despliegue. El proyecto sigue siendo educativo y local.
