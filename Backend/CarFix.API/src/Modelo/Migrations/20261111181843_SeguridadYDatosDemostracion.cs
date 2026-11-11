using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modelo.Migrations
{
    /// <inheritdoc />
    public partial class SeguridadYDatosDemostracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bloquea todas las credenciales antiguas, incluidas cuentas no precargadas.
            // Restablecer mediante CLI o administrador; nunca conservar hashes SHA-256.
            migrationBuilder.Sql("UPDATE [Usuarios] SET [Activo] = 0, [PasswordHash] = 'RESET_REQUIRED' WHERE [PasswordHash] NOT LIKE 'PBKDF2-SHA256$%';");
            migrationBuilder.AddColumn<int>(
                name: "Kilometraje",
                table: "Vehiculos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turnos_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Vehiculos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Kilometraje",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Vehiculos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Kilometraje",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Vehiculos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Kilometraje",
                value: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ClienteId",
                table: "Facturas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_ClienteId",
                table: "Turnos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_VehiculoId",
                table: "Turnos",
                column: "VehiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Clientes_ClienteId",
                table: "Facturas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Clientes_ClienteId",
                table: "Facturas");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_ClienteId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Kilometraje",
                table: "Vehiculos");
        }
    }
}
