$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '../Backend/CarFix.API/CarFix.API.csproj'
$server = Read-Host 'Instancia SQL Server local (ejemplo: .\SQLEXPRESS)'
$database = Read-Host 'Nombre de la base de datos local'
if ([string]::IsNullOrWhiteSpace($server) -or [string]::IsNullOrWhiteSpace($database) -or $server.Contains(';') -or $database.Contains(';')) {
    throw 'Indique servidor y base sin punto y coma.'
}
# Windows: autenticación integrada, sin contraseña SQL. Certificado local de desarrollo.
$connection = "Server=$server;Database=$database;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
$settings = @{
    'ConnectionStrings:CarFix' = $connection
    'Jwt:Key' = [Convert]::ToBase64String($bytes)
} | ConvertTo-Json -Compress
# stdin evita poner el secreto en argumentos o en el historial de comandos.
$settings | dotnet user-secrets set --project $project
if ($LASTEXITCODE -ne 0) { throw 'No se pudo guardar la configuración local.' }
$settings = $null
$connection = $null
[Array]::Clear($bytes, 0, $bytes.Length)
Write-Host 'Configuración local guardada fuera del repositorio. No copie secrets.json al proyecto.'
