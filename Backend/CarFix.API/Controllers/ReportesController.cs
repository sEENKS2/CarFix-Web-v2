using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Controladora;

namespace CarFix.API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ControladoraReportes _reportesTaller = ControladoraReportes.Instancia;
        private readonly ControladoraReportesCompras _reportesCompras = ControladoraReportesCompras.Instancia;

        [HttpGet("dashboard")]
        public IActionResult ObtenerResumenDashboard()
        {
            try
            {
                var statsTaller = _reportesTaller.ObtenerEstadisticasGenerales();
                var ticketsPorEstado = _reportesTaller.ObtenerTicketsPorEstado();
                var productividadTecnicos = _reportesTaller.GenerarReporteProductividadTecnicos();

                var fechaDesde = DateTime.Now.AddMonths(-6);
                var fechaHasta = DateTime.Now;

                var comprasMes = _reportesCompras.GenerarReporteComprasPorMes(fechaDesde, fechaHasta);
                var topProveedores = _reportesCompras.GenerarReporteProveedores(fechaDesde, fechaHasta);
                var bajoStock = _reportesCompras.RecuperarProductosBajoStock();

                return Ok(new
                {
                    estadisticas = statsTaller,
                    ticketsPorEstado,
                    productividadTecnicos,
                    comprasPorMes = comprasMes,
                    topProveedores,
                    productosBajoStock = bajoStock
                });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [HttpGet("productividad-tecnicos")]
        public IActionResult ProductividadTecnicos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            try
            {
                var reporte = _reportesTaller.GenerarReporteProductividadTecnicos(desde, hasta);
                return Ok(reporte);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [HttpGet("compras-proveedores")]
        public IActionResult ReporteProveedores([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            try
            {
                var fDesde = desde ?? DateTime.Now.AddMonths(-6);
                var fHasta = hasta ?? DateTime.Now;
                var reporte = _reportesCompras.GenerarReporteProveedores(fDesde, fHasta);
                return Ok(reporte);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }
    }
}