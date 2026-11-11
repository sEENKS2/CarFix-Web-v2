using CarFix.API.Security;
using Controladora;
using Entidades.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Modelo;
using System.Security.Claims;
using System.Threading.RateLimiting;
using CarFix.API.Services.IA;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables();
// Acción de mantenimiento local; no existe un endpoint público de registro inicial.
if (args.Contains("--bootstrap-admin") || args.Contains("--reset-admin"))
{
    Context.Configure(builder.Configuration.GetConnectionString("CarFix")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:CarFix fuera del repositorio."));
    try { Environment.ExitCode = await BootstrapAdmin.RunAsync(args.Contains("--reset-admin")); }
    catch { Console.Error.WriteLine("No se pudo configurar el administrador. Verifique usuario, contraseña, migraciones y conexión local."); Environment.ExitCode = 1; }
    return;
}
builder.Services.AddSingleton(sp => new JwtSettings(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddScoped(_ => new Context());

// Configurar CORS para permitir peticiones desde React (Vite / CRA)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 1. Controladores y JSON
builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// 2. Swagger con soporte para JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CarFix API", 
        Version = "v1" 
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // El error CS1660 ocurre si pasas una lambda; debe ser la instancia de OpenApiSecurityRequirement:
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 3. Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwtSettings>((options, jwtSettings) =>
    {
        options.IncludeErrorDetails = false;
        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
                { context.Fail("Sesión inválida."); return; }
                try
                {
                    var db = context.HttpContext.RequestServices.GetRequiredService<Context>();
                    var usuario = await db.Usuarios.AsNoTracking().Include(u => u.Grupos)
                        .ThenInclude(g => g.Permisos).SingleOrDefaultAsync(u => u.Id == id && u.Activo);
                    if (usuario == null || !usuario.PasswordHash.StartsWith(PasswordSecurity.Prefix + "$", StringComparison.Ordinal)
                        || context.Principal?.FindFirstValue("credential_version") != jwtSettings.CredentialVersion(usuario))
                    { context.Fail("La sesión debe renovarse."); return; }
                    context.HttpContext.Items["CarFix.Usuario"] = usuario;
                }
                catch { context.Fail("No se pudo validar la sesión."); }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
            AutoReplenishment = true
        }));
});

// Registrar HttpClient y el servicio de IA
//builder.Services.AddHttpClient<IIaService, GeminiIaService>();

builder.Services.AddHttpClient<IIaService, OllamaIaService>();

var app = builder.Build();
Context.Configure(app.Configuration.GetConnectionString("CarFix")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:CarFix fuera del repositorio."));
_ = app.Services.GetRequiredService<JwtSettings>(); // Validar antes de aceptar peticiones.

app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { mensaje = "No se pudo completar la operación." });
}));

// 4. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CarFix API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.Use(async (context, next) =>
{
    using var scope = ControladoraSeguridad.Instancia.IniciarContexto(
        context.Items["CarFix.Usuario"] as Usuario);
    await next(context);
});
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program { }
