using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Modelo.Migrations
{
    /// <inheritdoc />
    public partial class test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dni = table.Column<int>(type: "int", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tecnicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Especialidad = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dni = table.Column<int>(type: "int", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Año = table.Column<int>(type: "int", nullable: false),
                    Dominio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Tecnicos_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Tecnicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Apellido", "Correo", "Dni", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "Ejemplo1", "cliente1@example.invalid", 90000001, "ClienteDemo1", 11223 },
                    { 2, "Ejemplo2", "cliente2@example.invalid", 90000002, "TecnicoDemo1", 11667 },
                    { 3, "Ejemplo3", "cliente3@example.invalid", 90000003, "ClienteDemo3", 11990 }
                });

            migrationBuilder.InsertData(
                table: "Tecnicos",
                columns: new[] { "Id", "Apellido", "Correo", "Dni", "Especialidad", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ejemplo1", "tecnico1@example.invalid", 90000101, 1, "TecnicoDemo1" },
                    { 2, "Ejemplo2", "tecnico2@example.invalid", 90000102, 2, "TecnicoDemo2" },
                    { 3, "Ejemplo3", "tecnico3@example.invalid", 90000103, 0, "TecnicoDemo3" }
                });

            migrationBuilder.InsertData(
                table: "Vehiculos",
                columns: new[] { "Id", "Año", "ClienteId", "Dominio", "Marca", "Modelo" },
                values: new object[,]
                {
                    { 1, 2020, 1, "DEMO001", "Ford", "Fiesta" },
                    { 2, 2018, 1, "DEMO002", "Chevrolet", "Cruze" },
                    { 3, 2022, 1, "DEMO003", "Toyota", "Corolla" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "ClienteId", "Descripcion", "Estado", "FechaCreacion", "TecnicoId", "VehiculoId" },
                values: new object[,]
                {
                    { 1, 1, "Cambio de aceite y filtro", 0, new DateTime(2023, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1 },
                    { 2, 1, "Reparación de parabrisas", 0, new DateTime(2023, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2 },
                    { 3, 1, "Falla en instrumental", 0, new DateTime(2023, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClienteId",
                table: "Tickets",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TecnicoId",
                table: "Tickets",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VehiculoId",
                table: "Tickets",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_ClienteId",
                table: "Vehiculos",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Tecnicos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
