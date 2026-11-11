using System.Text.Json;
using CarFix.API.DTOs;
using Entidades.Tickets;
using Microsoft.EntityFrameworkCore;
using Modelo;
using Xunit;

namespace CarFix.Security.Tests;

public class ClienteTelefonoTests
{
    [Theory]
    [InlineData("5491100000000")]
    [InlineData("+54 9 11 0000-0000")]
    [InlineData("001100000000")]
    [InlineData("")]
    public void TelefonoSeRecibeYConservaComoTexto(string telefono)
    {
        var json = JsonSerializer.Serialize(new { telefono });
        var dto = JsonSerializer.Deserialize<ClienteDTO>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(telefono, dto.Telefono);
        var options = new DbContextOptionsBuilder<Context>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var db = new Context(options))
        {
            db.Clientes.Add(new Cliente { Nombre = "Prueba", Apellido = "Telefono", Dni = 90000999,
                Correo = "telefono@example.invalid", Telefono = dto.Telefono });
            db.SaveChanges();
        }
        using var query = new Context(options);
        Assert.Equal(telefono, query.Clientes.Single().Telefono);
    }
}
