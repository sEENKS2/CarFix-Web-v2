using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Controladora;

namespace CarFix.API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores,Tecnicos")]
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialDescripcionesController : ControllerBase
    {
        private readonly ControladoraHistorialDescripciones _gestor = ControladoraHistorialDescripciones.Instancia;

        [HttpGet("ticket/{ticketId}")]
        public IActionResult ObtenerPorTicket(int ticketId)
        {
            try
            {
                var historial = _gestor.ObtenerHistorialPorTicket(ticketId);
                return Ok(historial);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }
    }
}