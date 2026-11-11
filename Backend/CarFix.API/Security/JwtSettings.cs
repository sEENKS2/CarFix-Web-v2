using System.Security.Cryptography;
using System.Text;
using Entidades.Core;

namespace CarFix.API.Security;

public sealed class JwtSettings
{
    public string Key { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpirationMinutes { get; }

    public JwtSettings(IConfiguration configuration)
    {
        Key = configuration["Jwt:Key"] ?? "";
        if (string.IsNullOrWhiteSpace(Key) || Encoding.UTF8.GetByteCount(Key) < 32
            || Key.Contains("REPLACE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure Jwt:Key con al menos 32 bytes aleatorios fuera de Git.");
        Issuer = configuration["Jwt:Issuer"] ?? "CarFixAPI";
        Audience = configuration["Jwt:Audience"] ?? "CarFixReactApp";
        ExpirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;
        if (ExpirationMinutes < 5 || ExpirationMinutes > 120)
            throw new InvalidOperationException("Jwt:ExpirationMinutes debe estar entre 5 y 120.");
    }

    // Vincula el token a la contraseña actual sin publicar su hash en el JWT.
    public string CredentialVersion(Usuario usuario) => Convert.ToBase64String(
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(usuario.PasswordHash + "|" + string.Join(",", usuario.Grupos.OrderBy(g => g.Id).Select(g => $"{g.Id}:{g.Nombre}")))));
}
