using Entidades.Core;
using Microsoft.EntityFrameworkCore;
using Modelo;

namespace CarFix.API.Security;

public static class BootstrapAdmin
{
    public static async Task<int> RunAsync(bool reset)
    {
        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Ejecute este comando en una terminal interactiva. La contraseña no se admite por argumentos.");
            return 1;
        }
        Console.Write(reset ? "Usuario administrador a recuperar: " : "Nuevo usuario administrador: ");
        var name = Console.ReadLine()?.Trim() ?? "";
        if (name.Length < 3 || name.Length > 50) throw new ArgumentException("Nombre de usuario: entre 3 y 50 caracteres.");
        using var db = new Context();
        var user = await db.Usuarios.Include(u => u.Grupos).SingleOrDefaultAsync(u => u.NombreUsuario == name);
        if (user != null && !reset) throw new InvalidOperationException("El usuario ya existe. Use --reset-admin para recuperarlo explícitamente.");
        if (user == null && reset) throw new InvalidOperationException("No existe ese usuario.");
        if (user == null)
        {
            Console.Write("Correo del administrador: ");
            var email = Console.ReadLine()?.Trim() ?? "";
            if (email.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("Indique un correo válido.");
            user = new Usuario { NombreUsuario = name, Email = email, FechaCreacion = DateTime.UtcNow };
            db.Usuarios.Add(user);
        }
        Console.Write("Nueva contraseña (12–128 caracteres, no se mostrará): ");
        var password = ReadPassword();
        Console.Write("Repetir contraseña: ");
        if (password != ReadPassword()) throw new ArgumentException("Las contraseñas no coinciden.");
        user.EstablecerPassword(password);
        user.Activo = true;
        var group = await db.Grupos.SingleAsync(g => g.Nombre == "Administradores");
        if (!user.Grupos.Any(g => g.Id == group.Id)) user.Grupos.Add(group);
        await db.SaveChangesAsync();
        Console.WriteLine("Administrador configurado. No se guardó su contraseña en archivos del proyecto.");
        return 0;
    }

    private static string ReadPassword()
    {
        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return new string(chars.ToArray()); }
            if (key.Key == ConsoleKey.Backspace) { if (chars.Count > 0) chars.RemoveAt(chars.Count - 1); }
            else if (!char.IsControl(key.KeyChar))
            {
                if (chars.Count >= PasswordSecurity.MaximumLength)
                    throw new ArgumentException("La contraseña no puede superar los 128 caracteres.");
                chars.Add(key.KeyChar);
            }
        }
    }
}
