using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Controladora;
using Entidades.Tickets;
using CarFix.API.DTOs;
using static Entidades.Tickets.Tecnico;

namespace CarFix.API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores,Tecnicos")]
    [ApiController]
    [Route("api/[controller]")]
    public class TecnicosController : ControllerBase
    {
        private readonly ControladoraTecnicos _gestorTecnicos = ControladoraTecnicos.Instancia;

        [HttpGet]
        public IActionResult Listar()
        {
            try
            {
                var tecnicos = _gestorTecnicos.RecuperarTecnicos();
                return Ok(tecnicos);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPost]
        public IActionResult Crear([FromBody] TecnicoAltaDTO dto)
        {
            try
            {
                _gestorTecnicos.AgregarTecnico(
                    dto.Nombre,
                    dto.Apellido,
                    dto.Dni,
                    dto.Correo,
                    (EnumEspecialidad)dto.Especialidad
                );
                return Ok(new { mensaje = "Técnico agregado exitosamente" });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPut("{id}")]
        public IActionResult Modificar(int id, [FromBody] TecnicoAltaDTO dto)
        {
            try
            {
                _gestorTecnicos.ModificarTecnico(
                    id,
                    dto.Nombre,
                    dto.Apellido,
                    dto.Dni,
                    dto.Correo,
                    (EnumEspecialidad)dto.Especialidad
                );
                return Ok(new { mensaje = "Técnico modificado exitosamente" });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var resultado = _gestorTecnicos.EliminarTecnico(id);
                return Ok(new { mensaje = resultado });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }
    }
}