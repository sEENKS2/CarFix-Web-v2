using System.Security.Cryptography;

namespace Entidades.Core;

public static class PasswordSecurity
{
    public const string Prefix = "PBKDF2-SHA256";
    public const int Iterations = 600_000;
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumLength || password.Length > MaximumLength)
            throw new ArgumentException("La contraseña debe tener entre 12 y 128 caracteres.", nameof(password));
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string? storedHash, string? password)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password) || password.Length > MaximumLength)
            return false;
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out int iterations)
            || iterations < Iterations || iterations > 1_000_000) return false;
        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            if (salt.Length != 16 || expected.Length != 32) return false;
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
