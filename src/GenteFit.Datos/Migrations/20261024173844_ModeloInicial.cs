using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenteFit.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ModeloInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actividad",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    intensidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    duracion_minutos = table.Column<int>(type: "int", nullable: false),
                    activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actividad", x => x.id);
                    table.CheckConstraint("ck_actividad_duracion", "[duracion_minutos] > 0");
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dni = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    fecha_nacimiento = table.Column<DateTime>(type: "date", nullable: true),
                    fecha_alta = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_baja = table.Column<DateTime>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monitor",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monitor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sala",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    aforo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sala", x => x.id);
                    table.CheckConstraint("ck_sala_aforo", "[aforo] > 0");
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    hash_contrasena = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    id_cliente = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "monitor_actividad",
                columns: table => new
                {
                    id_actividad = table.Column<int>(type: "int", nullable: false),
                    id_monitor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monitor_actividad", x => new { x.id_actividad, x.id_monitor });
                    table.ForeignKey(
                        name: "fk_monitor_actividad_actividad_id_actividad",
                        column: x => x.id_actividad,
                        principalTable: "actividad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_monitor_actividad_monitor_id_monitor",
                        column: x => x.id_monitor,
                        principalTable: "monitor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sesion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_hora_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    id_actividad = table.Column<int>(type: "int", nullable: false),
                    id_sala = table.Column<int>(type: "int", nullable: false),
                    id_monitor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sesion", x => x.id);
                    table.ForeignKey(
                        name: "fk_sesion_actividad_id_actividad",
                        column: x => x.id_actividad,
                        principalTable: "actividad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sesion_monitor_id_monitor",
                        column: x => x.id_monitor,
                        principalTable: "monitor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sesion_sala_id_sala",
                        column: x => x.id_sala,
                        principalTable: "sala",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lote_intercambio",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    fecha_desde = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_hasta = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_generacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ruta_fichero = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    num_registros = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lote_intercambio", x => x.id);
                    table.ForeignKey(
                        name: "fk_lote_intercambio_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reserva",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    fecha_cancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_sesion = table.Column<int>(type: "int", nullable: false),
                    id_cliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reserva", x => x.id);
                    table.ForeignKey(
                        name: "fk_reserva_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reserva_sesiones_id_sesion",
                        column: x => x.id_sesion,
                        principalTable: "sesion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "actividad",
                columns: new[] { "id", "activa", "descripcion", "duracion_minutos", "intensidad", "nombre", "tipo" },
                values: new object[,]
                {
                    { 1, true, null, 55, "ALTA", "Body Pump", "BODY_PUMP" },
                    { 2, true, null, 55, "ALTA", "Body Combat", "BODY_COMBAT" },
                    { 3, true, null, 30, "MEDIA", "Core", "CORE" },
                    { 4, true, "Estiramientos y respiración para todos los niveles", 60, "BAJA", "Yoga", "YOGA" },
                    { 5, true, null, 45, "ALTA", "Spinning", "SPINNING" },
                    { 6, true, null, 50, "MEDIA", "Zumba", "ZUMBA" }
                });

            migrationBuilder.InsertData(
                table: "monitor",
                columns: new[] { "id", "activo", "apellidos", "email", "nombre" },
                values: new object[,]
                {
                    { 1, true, "Núñez O'Connor", "mariajose@gentefit.test", "María José" },
                    { 2, true, "Puig i Font", "jordi@gentefit.test", "Jordi" }
                });

            migrationBuilder.InsertData(
                table: "sala",
                columns: new[] { "id", "aforo", "nombre" },
                values: new object[,]
                {
                    { 1, 16, "Sala Montjuïc" },
                    { 2, 16, "Sala d'Spinning" }
                });

            migrationBuilder.InsertData(
                table: "sesion",
                columns: new[] { "id", "estado", "fecha_hora_inicio", "id_actividad", "id_monitor", "id_sala" },
                values: new object[,]
                {
                    { 1, "PROGRAMADA", new DateTime(2026, 11, 2, 18, 30, 0, 0, DateTimeKind.Unspecified), 4, 1, 1 },
                    { 2, "PROGRAMADA", new DateTime(2026, 11, 3, 19, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 1 },
                    { 3, "PROGRAMADA", new DateTime(2026, 11, 4, 20, 0, 0, 0, DateTimeKind.Unspecified), 5, 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_dni",
                table: "cliente",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_email",
                table: "cliente",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lote_intercambio_codigo",
                table: "lote_intercambio",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lote_intercambio_id_usuario",
                table: "lote_intercambio",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_monitor_email",
                table: "monitor",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_monitor_actividad_id_monitor",
                table: "monitor_actividad",
                column: "id_monitor");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_id_cliente",
                table: "reserva",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_id_sesion_id_cliente",
                table: "reserva",
                columns: new[] { "id_sesion", "id_cliente" },
                unique: true,
                filter: "[estado] <> 'CANCELADA'");

            migrationBuilder.CreateIndex(
                name: "ix_sala_nombre",
                table: "sala",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sesion_id_actividad",
                table: "sesion",
                column: "id_actividad");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_id_monitor",
                table: "sesion",
                column: "id_monitor");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_id_sala_fecha_hora_inicio",
                table: "sesion",
                columns: new[] { "id_sala", "fecha_hora_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_id_cliente",
                table: "usuario",
                column: "id_cliente",
                unique: true,
                filter: "[id_cliente] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_nombre_usuario",
                table: "usuario",
                column: "nombre_usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lote_intercambio");

            migrationBuilder.DropTable(
                name: "monitor_actividad");

            migrationBuilder.DropTable(
                name: "reserva");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "sesion");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "actividad");

            migrationBuilder.DropTable(
                name: "monitor");

            migrationBuilder.DropTable(
                name: "sala");
        }
    }
}
