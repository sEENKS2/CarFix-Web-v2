using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Entidades.Core;
using CarFix.API.DTOs;
using Modelo;
using System.Security.Claims;

namespace CarFix.API.Controllers;

[Authorize(Roles = "Administradores")]
[ApiController]
[Route("api/[controller]")]
public class SeguridadController(Context db) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios() => Ok(await db.Usuarios.AsNoTracking().Select(u => new
    {
        u.Id, u.NombreUsuario, u.Email, u.Activo, u.FechaCreacion, u.UltimoAcceso,
        Grupos = u.Grupos.Select(g => g.Nombre).ToList()
    }).ToListAsync());

    [HttpGet("grupos")]
    public async Task<IActionResult> ListarGrupos() => Ok(await db.Grupos.AsNoTracking().Select(g => new
    {
        g.Id, g.Nombre, g.Descripcion, Permisos = g.Permisos.Select(p => p.Nombre).ToList()
    }).ToListAsync());

    [HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDTO dto)
    {
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            return Conflict("El nombre de usuario ya existe.");
        Grupo? grupo = dto.GrupoId.HasValue ? await db.Grupos.FindAsync(dto.GrupoId.Value) : null;
        if (dto.GrupoId.HasValue && grupo == null) return BadRequest("El grupo no existe.");
        var usuario = new Usuario { NombreUsuario = dto.NombreUsuario, Email = dto.Email,
            FechaCreacion = DateTime.UtcNow, Activo = true, Grupos = new List<Grupo>() };
        usuario.EstablecerPassword(dto.Password);
        if (grupo != null) usuario.Grupos.Add(grupo);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Ok(new { mensaje = "Usuario creado exitosamente" });
    }

    [HttpPut("usuarios/{id}/password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] CambiarPasswordDTO dto)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound("Usuario no encontrado.");
        usuario.EstablecerPassword(dto.PasswordNuevo);
        // La recuperación explícita reactiva una cuenta bloqueada por la migración.
        usuario.Activo = true;
        await db.SaveChangesAsync();
        return Ok(new { mensaje = "Contraseña restablecida. Las sesiones anteriores quedaron invalidadas." });
    }

    [HttpDelete("usuarios/{id}")]
    public async Task<IActionResult> DesactivarUsuario(int id)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
            return BadRequest("No puede desactivar su propia cuenta.");
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound("Usuario no encontrado.");
        usuario.Activo = false;
        await db.SaveChangesAsync();
        return Ok(new { mensaje = "Usuario desactivado del sistema" });
    }
}
