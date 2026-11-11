using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Controladora;
using CarFix.API.DTOs;

namespace CarFix.API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores,Tecnicos")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ControladoraClientes _gestor = ControladoraClientes.Instancia;

        [HttpGet]
        public IActionResult Listar()
        {
            try
            {
                var clientes = _gestor.RecuperarClientes();
                return Ok(clientes);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPost]
        public IActionResult Crear([FromBody] ClienteDTO dto)
        {
            try
            {
                _gestor.AgregarCliente(dto.Nombre, dto.Apellido, dto.Dni, dto.Correo, dto.Telefono);
                return Ok(new { mensaje = "Cliente registrado con éxito" });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPut("{id}")]
        public IActionResult Modificar(int id, [FromBody] ClienteDTO dto)
        {
            try
            {
                _gestor.ModificarCliente(id, dto.Nombre, dto.Apellido, dto.Dni, dto.Correo, dto.Telefono);
                return Ok(new { mensaje = "Cliente modificado con éxito" });
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
                var msg = _gestor.EliminarCliente(id);
                return Ok(new { mensaje = msg });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }
    }
}