using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Controladora;
using CarFix.API.DTOs;

namespace CarFix.API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores,Tecnicos")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ControladoraProductos _gestorProductos = ControladoraProductos.Instancia;

        [HttpGet]
        public IActionResult Listar()
        {
            try
            {
                var lista = _gestorProductos.RecuperarProductos();
                return Ok(lista);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPost]
        public IActionResult Crear([FromBody] ProductoAltaDTO dto)
        {
            try
            {
                _gestorProductos.AgregarProducto(
                    dto.Codigo,
                    dto.Nombre,
                    dto.Descripcion,
                    dto.Categoria,
                    dto.PrecioUnitario,
                    dto.StockMinimo,
                    dto.StockActual
                );
                return Ok(new { mensaje = "Producto agregado correctamente" });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administradores,Operadores")]
        [HttpPut("{id}")]
        public IActionResult Modificar(int id, [FromBody] ProductoAltaDTO dto)
        {
            try
            {
                _gestorProductos.ModificarProducto(
                    id,
                    dto.Codigo,
                    dto.Nombre,
                    dto.Descripcion,
                    dto.Categoria,
                    dto.PrecioUnitario,
                    dto.StockMinimo,
                    dto.StockActual
                );
                return Ok(new { mensaje = "Producto actualizado correctamente" });
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
                _gestorProductos.EliminarProducto(id);
                return Ok(new { mensaje = "Producto dado de baja correctamente" });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo completar la operación. Revise los datos ingresados.");
            }
        }
    }
}