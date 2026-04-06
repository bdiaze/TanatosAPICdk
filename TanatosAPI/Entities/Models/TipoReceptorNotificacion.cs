using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    [Comment("Tabla que contiene los tipos de receptores de notificación.")]
    public class TipoReceptorNotificacion {
		[UseColumnAttribute]
		[Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Comment("Identificador del tipo de receptor de notificación.")]
        public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("nombre")]
        [Comment("Nombre del tipo de receptor de notificación.")]
        public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("regex_validacion")]
		[Comment("Regex para validar el tipo de receptor.")]
		public string? RegexValidacion { get; set; }

        [UseColumnAttribute]
        [Required]
		[DefaultValue(false)]
		[Column("requiere_plan_empresa")]
        [Comment("Indicador de si el tipo de receptor requiere de que el usuario tenga plan Empresa.")]
        public required bool RequierePlanEmpresa { get; set; } = false;

		[UseColumnAttribute]
		[Required]
        [Column("vigencia")]
        [Comment("Vigencia del tipo de receptor de notificación.")]
        public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }
    }
}
