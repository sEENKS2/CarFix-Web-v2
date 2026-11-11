using CarFix.API.Security;
using Entidades.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Modelo;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace CarFix.Security.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly string database = Guid.NewGuid().ToString();
    public readonly string SigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:CarFix"] = "Server=localhost;Database=NotUsed;Integrated Security=True;TrustServerCertificate=True",
            ["Jwt:Key"] = SigningKey
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<Context>();
            services.AddScoped(_ => new Context(new DbContextOptionsBuilder<Context>().UseInMemoryDatabase(database).Options));
        });
    }

    public string Token(string role, bool expired = false, string? issuer = null, string? key = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();
        int id = role == "Administradores" ? 101 : role == "Operadores" ? 102 : role == "Tecnicos" ? 103 : 104;
        var user = db.Usuarios.Include(u => u.Grupos).SingleOrDefault(u => u.Id == id);
        if (user == null)
        {
            user = new Usuario { Id = id, NombreUsuario = "test-" + id, Email = $"test{id}@example.invalid", Activo = true,
                Grupos = new List<Grupo> { new Grupo { Id = id, Nombre = role, Descripcion = "Prueba" } } };
            user.EstablecerPassword(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)));
            db.Usuarios.Add(user); db.SaveChanges();
        }
        var settings = Services.GetRequiredService<JwtSettings>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role),
            new Claim("credential_version", settings.CredentialVersion(user)) };
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer ?? settings.Issuer, settings.Audience, claims,
            expires: expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? SigningKey)), SecurityAlgorithms.HmacSha256)));
    }
}

public class ApiSecurityTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture app;
    public ApiSecurityTests(ApiFixture app) => this.app = app;
    private HttpClient Client(string? token = null)
    {
        var client = app.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        if (token != null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Theory]
    [InlineData("/api/Tickets")]
    [InlineData("/api/Turnos")]
    [InlineData("/api/Inventario")]
    [InlineData("/api/Facturas")]
    [InlineData("/api/Seguridad/usuarios")]
    [InlineData("/api/Usuarios/me")]
    public async Task ApiRejectsAnonymousRequests(string path)
        => Assert.Equal(HttpStatusCode.Unauthorized, (await Client().GetAsync(path)).StatusCode);

    [Theory]
    [InlineData("Operadores")]
    [InlineData("Tecnicos")]
    public async Task NonAdminCannotReadUsersOrResetPasswords(string role)
    {
        using var client = Client(app.Token(role));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/Seguridad/usuarios")).StatusCode);
        var body = new StringContent("{}", Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsync("/api/Seguridad/usuarios/101/password", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/Usuarios")).StatusCode);
    }

    [Fact]
    public async Task AdministratorCanReadUsersWithoutHashes()
    {
        var response = await Client(app.Token("Administradores")).GetAsync("/api/Seguridad/usuarios");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("passwordHash", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExpiredWrongIssuerAndWrongSignatureAreRejected()
    {
        var randomKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        foreach (string token in new[] { app.Token("Operadores", expired: true), app.Token("Operadores", issuer: "other"), app.Token("Operadores", key: randomKey) })
            Assert.Equal(HttpStatusCode.Unauthorized, (await Client(token).GetAsync("/api/Usuarios/me")).StatusCode);
    }

    [Fact]
    public async Task PasswordResetInvalidatesAlreadyIssuedToken()
    {
        var token = app.Token("Tecnicos");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            db.Usuarios.Find(103)!.EstablecerPassword(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)));
            db.SaveChanges();
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await Client(token).GetAsync("/api/Usuarios/me")).StatusCode);
    }

    [Fact]
    public async Task TechnicianCannotAccessPurchasing()
        => Assert.Equal(HttpStatusCode.Forbidden, (await Client(app.Token("Tecnicos")).GetAsync("/api/OrdenesCompra")).StatusCode);
    [Fact]
    public async Task LoginIssuesTokenOnlyForValidActiveCredentials()
    {
        using var isolated = new ApiFixture();
        using var client = isolated.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var password = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        using (var scope = isolated.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var user = new Usuario { Id = 201, NombreUsuario = "login-prueba", Email = "login@example.invalid", Activo = true, Grupos = new List<Grupo>() };
            user.EstablecerPassword(password); db.Usuarios.Add(user); db.SaveChanges();
        }
        var wrong = await client.PostAsJsonAsync("/api/Usuarios/login", new { nombreUsuario = "login-prueba", password = "" + Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        var success = await client.PostAsJsonAsync("/api/Usuarios/login", new { nombreUsuario = "login-prueba", password });
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        var json = await success.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/Usuarios/me")).StatusCode);
        using (var scope = isolated.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            db.Usuarios.Find(201)!.Activo = false; db.SaveChanges();
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Usuarios/me")).StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/Usuarios/login", new { nombreUsuario = "login-prueba", password })).StatusCode);
    }

    [Fact]
    public async Task AdministratorCreatesAccountAndRejectsWeakPasswordAndMissingGroup()
    {
        var client = Client(app.Token("Administradores"));
        var name = "nuevo-" + Guid.NewGuid().ToString("N");
        var password = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/Seguridad/usuarios",
            new { nombreUsuario = name, email = "nuevo@example.invalid", password = "" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/Seguridad/usuarios",
            new { nombreUsuario = name, email = "nuevo@example.invalid", password, grupoId = 99999 })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/Seguridad/usuarios",
            new { nombreUsuario = name, email = "nuevo@example.invalid", password })).StatusCode);
        using var scope = app.Services.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<Context>().Usuarios.Single(u => u.NombreUsuario == name);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.True(user.VerificarPassword(password));
    }

    [Fact]
    public async Task LoginLimitsRepeatedAttempts()
    {
        using var isolated = new ApiFixture();
        using var client = isolated.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var statuses = new List<HttpStatusCode>();
        for (int i = 0; i < 6; i++) statuses.Add((await client.PostAsJsonAsync("/api/Usuarios/login", new { nombreUsuario = "no-existe", password = "" + Guid.NewGuid() })).StatusCode);
        Assert.All(statuses.Take(5), status => Assert.Equal(HttpStatusCode.Unauthorized, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses.Last());
    }

    [Fact]
    public async Task IaRequiresAuthenticationAndSupportedRole()
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/Ia/diagnostico", new { })).StatusCode);
        var token = app.Token("OtroRol");
        Assert.Equal(HttpStatusCode.Forbidden, (await Client(token).PostAsJsonAsync("/api/Ia/diagnostico", new { })).StatusCode);
    }

}
