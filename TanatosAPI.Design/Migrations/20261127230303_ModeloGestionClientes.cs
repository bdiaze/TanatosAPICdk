using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ModeloGestionClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cliente",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del cliente.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Identificador del usuario al que pertenece el cliente."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio al que pertenece el cliente."),
                    tipo_persona = table.Column<short>(type: "smallint", nullable: false, comment: "Tipo de persona a la que corresponde el cliente. 1: Persona natural - 2: Persona jurídica."),
                    tipo_cliente = table.Column<short>(type: "smallint", nullable: false, comment: "Tipo de cliente. 1: Cliente - 2: Subcliente."),
                    rut = table.Column<string>(type: "text", nullable: true, comment: "RUT del cliente."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del cliente."),
                    id_cliente_padre = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del cliente padre."),
                    observacion = table.Column<string>(type: "text", nullable: true, comment: "Observación relacionada con el cliente."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el cliente."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el cliente."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del cliente.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id);
                    table.ForeignKey(
                        name: "FK_cliente_cliente_id_cliente_padre",
                        column: x => x.id_cliente_padre,
                        principalSchema: "tanatos",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cliente_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los clientes pertenecientes a un usuario.");

            migrationBuilder.CreateTable(
                name: "tipo_contacto",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de contacto."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de contacto."),
                    regex_validacion = table.Column<string>(type: "text", nullable: true, comment: "Regex para validar el tipo de contacto."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de contacto.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_contacto", x => x.id);
                },
                comment: "Tabla que contiene los tipos de contactos.");

            migrationBuilder.CreateTable(
                name: "tipo_interaccion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de interacción."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de interacción."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de interacción.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_interaccion", x => x.id);
                },
                comment: "Tabla que contiene los tipos de interacciones.");

            migrationBuilder.CreateTable(
                name: "tarea",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la tarea.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del cliente relacionado con la tarea."),
                    descripcion = table.Column<string>(type: "text", nullable: false, comment: "Descripción de la tarea."),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que vence la tarea"),
                    estado = table.Column<short>(type: "smallint", nullable: false, comment: "Estado de la tarea. 0: Pendiente - 1: Completada - 2: Cancelada."),
                    fecha_completitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se completó la tarea."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la tarea."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la tarea."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la tarea.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarea", x => x.id);
                    table.ForeignKey(
                        name: "FK_tarea_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "tanatos",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las tareas relacionadas con un cliente.");

            migrationBuilder.CreateTable(
                name: "contacto",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del contacto.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del cliente al que pertenece el contacto."),
                    id_tipo_contacto = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de contacto."),
                    valor = table.Column<string>(type: "text", nullable: false, comment: "Valor del dato de contacto."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el contacto."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el contacto."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del contacto.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacto", x => x.id);
                    table.ForeignKey(
                        name: "FK_contacto_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "tanatos",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contacto_tipo_contacto_id_tipo_contacto",
                        column: x => x.id_tipo_contacto,
                        principalSchema: "tanatos",
                        principalTable: "tipo_contacto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los datos de contacto pertenecientes a un cliente.");

            migrationBuilder.CreateTable(
                name: "interaccion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la interacción.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del cliente al que pertenece la interacción."),
                    id_cliente_relacionado = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del subcliente relacionado con la interacción."),
                    id_contacto_relacionado = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del contacto relacionado con la interacción."),
                    id_tipo_interaccion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de interacción."),
                    fecha_interaccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se llevó a cabo la interacción."),
                    detalle = table.Column<string>(type: "text", nullable: false, comment: "Detalle de la interacción."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la interacción."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la interacción."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la interacción.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interaccion", x => x.id);
                    table.ForeignKey(
                        name: "FK_interaccion_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "tanatos",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_interaccion_cliente_id_cliente_relacionado",
                        column: x => x.id_cliente_relacionado,
                        principalSchema: "tanatos",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_interaccion_contacto_id_contacto_relacionado",
                        column: x => x.id_contacto_relacionado,
                        principalSchema: "tanatos",
                        principalTable: "contacto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_interaccion_tipo_interaccion_id_tipo_interaccion",
                        column: x => x.id_tipo_interaccion,
                        principalSchema: "tanatos",
                        principalTable: "tipo_interaccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las interacciones que se han tenido con un cliente.");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_id_cliente_padre",
                schema: "tanatos",
                table: "cliente",
                column: "id_cliente_padre");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_id_negocio",
                schema: "tanatos",
                table: "cliente",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_sub_id_negocio_tipo_cliente",
                schema: "tanatos",
                table: "cliente",
                columns: new[] { "sub", "id_negocio", "tipo_cliente" });

            migrationBuilder.CreateIndex(
                name: "IX_contacto_id_cliente",
                schema: "tanatos",
                table: "contacto",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_contacto_id_tipo_contacto",
                schema: "tanatos",
                table: "contacto",
                column: "id_tipo_contacto");

            migrationBuilder.CreateIndex(
                name: "IX_interaccion_id_cliente",
                schema: "tanatos",
                table: "interaccion",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_interaccion_id_cliente_relacionado",
                schema: "tanatos",
                table: "interaccion",
                column: "id_cliente_relacionado");

            migrationBuilder.CreateIndex(
                name: "IX_interaccion_id_contacto_relacionado",
                schema: "tanatos",
                table: "interaccion",
                column: "id_contacto_relacionado");

            migrationBuilder.CreateIndex(
                name: "IX_interaccion_id_tipo_interaccion",
                schema: "tanatos",
                table: "interaccion",
                column: "id_tipo_interaccion");

            migrationBuilder.CreateIndex(
                name: "IX_tarea_id_cliente",
                schema: "tanatos",
                table: "tarea",
                column: "id_cliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interaccion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tarea",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "contacto",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_interaccion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "cliente",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_contacto",
                schema: "tanatos");
        }
    }
}
