using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablaModeloCanvas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "modelo_canvas",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del modelo Canvas.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el modelo Canvas."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio del usuario."),
                    socios_clave = table.Column<string>(type: "text", nullable: true, comment: "Corresponde a los aliados estratégicos, proveedores o socios comerciales que te ayudan a optimizar recursos o mitigar riesgos."),
                    actividades_clave = table.Column<string>(type: "text", nullable: true, comment: "Corresponde a las acciones más importantes que debes ejecutar para operar, como producción, desarrollo de software, resolución de problemas, etc."),
                    recursos_clave = table.Column<string>(type: "text", nullable: true, comment: "Corresponde a los activos indispensables para que el negocio funcione, como humanos, tecnológicos, físicos o financieros."),
                    propuesta_valor = table.Column<string>(type: "text", nullable: true, comment: "Identifica el problema o necesidad que resuelve tu producto o servicio, qué lo hace diferente de la competencia."),
                    relaciones_clientes = table.Column<string>(type: "text", nullable: true, comment: "Identifica el tipo de interacción que tendrás con los clientes, como asistencia personalizada, autoservicio automatizado, o a través de comunidades."),
                    canales = table.Column<string>(type: "text", nullable: true, comment: "Identifica cómo vas a entregar tu propuesta de valor a los clientess, mediante tiendas físicas, plataformas digitales o aplicaciones móviles."),
                    segmentos_clientes = table.Column<string>(type: "text", nullable: true, comment: "Identifica quiénes son tus clientes objetivos, definiendo características demográficas, sociales y comportamientos específicos."),
                    estructura_costos = table.Column<string>(type: "text", nullable: true, comment: "Corresponde a todos los gastos e inversiones necesarios para mantener el negocio en marcha."),
                    fuentes_ingresos = table.Column<string>(type: "text", nullable: true, comment: "Identifica cómo va a ganar dinero el negocio, define estrategias de precios y los métodos de pago disponibles."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el modelo Canvas."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el modelo Canvas."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del modelo Canvas.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modelo_canvas", x => x.id);
                    table.ForeignKey(
                        name: "FK_modelo_canvas_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los modelos Canvas asociados al negocio del usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_modelo_canvas_id_negocio",
                schema: "tanatos",
                table: "modelo_canvas",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_modelo_canvas_sub_id_negocio",
                schema: "tanatos",
                table: "modelo_canvas",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modelo_canvas",
                schema: "tanatos");
        }
    }
}
