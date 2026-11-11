using Microsoft.EntityFrameworkCore;
using Modelo;
using CarFix.API.Security;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Controladora;
using CarFix.API.DTOs;

namespace CarFix.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly Context _db;
        private readonly JwtSettings _settings;

        public UsuariosController(JwtSettings settings, Context db)
        {
            _settings = settings;
            _db = db;
        }

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                var usuario = await _db.Usuarios.Include(u => u.Grupos)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == login.NombreUsuario && u.Activo);

                if (usuario == null || !usuario.VerificarPassword(login.Password))
                    return Unauthorized("Credenciales inválidas");


                usuario.UltimoAcceso = DateTime.UtcNow;
                _db.AuditoriasLogin.Add(new Entidades.Validaciones.AuditoriaLogin
                {
                    UsuarioId = usuario.Id, NombreUsuario = usuario.NombreUsuario,
                    FechaHora = DateTime.UtcNow, TipoEvento = "LOGIN",
                    DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida"
                });
                await _db.SaveChangesAsync();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                    new Claim("credential_version", _settings.CredentialVersion(usuario))
                };

                if (usuario.Grupos != null)
                {
                    foreach (var grupo in usuario.Grupos)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, grupo.Nombre));
                    }
                }

                var jwtKey = _settings.Key;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                    signingCredentials: creds
                );

                var jwtToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

                return Ok(new
                {
                    mensaje = "Acceso concedido",
                    id = usuario.Id,
                    nombreUsuario = usuario.NombreUsuario,
                    email = usuario.Email,
                    roles = usuario.Grupos?.Select(g => g.Nombre).ToList() ?? new List<string>(),
                    token = jwtToken
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "No se pudo completar la operación.");
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var usuario = HttpContext.Items["CarFix.Usuario"] as Entidades.Core.Usuario;
            if (usuario == null) return Unauthorized();
            return Ok(new { id = usuario.Id, nombreUsuario = usuario.NombreUsuario, email = usuario.Email,
                roles = usuario.Grupos.Select(g => g.Nombre).ToList() });
        }

        [Authorize(Roles = "Administradores")]
        [HttpGet]
        public IActionResult ListarUsuarios()
        {
            try
            {
                var usuarios = _db.Usuarios.AsNoTracking().Select(u => new
                {
                    u.Id,
                    u.NombreUsuario,
                    u.Email,
                    u.Activo,
                    u.FechaCreacion,
                    u.UltimoAcceso,
                    Grupos = u.Grupos.Select(g => g.Nombre).ToList()
                });
                return Ok(usuarios);
            }
            catch (Exception)
            {
                return StatusCode(500, "No se pudo completar la operación.");
            }
        }
    }
}