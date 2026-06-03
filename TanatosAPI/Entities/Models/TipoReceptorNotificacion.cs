using Dapper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    public class TipoReceptorNotificacion {
		[UseColumnAttribute]
		[Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("regex_validacion")]
		public string? RegexValidacion { get; set; }

        [UseColumnAttribute]
        [Required]
		[DefaultValue(false)]
		[Column("requiere_plan_empresa")]
        public required bool RequierePlanEmpresa { get; set; } = false;

		[UseColumnAttribute]
		[Required]
        [Column("vigencia")]
        public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }
    }
}
