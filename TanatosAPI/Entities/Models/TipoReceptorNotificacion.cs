using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    public class TipoReceptorNotificacion {
		[Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required long Id { get; set; }

		[Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

		[Column("regex_validacion")]
		public string? RegexValidacion { get; set; }

        [Required]
		[DefaultValue(false)]
		[Column("requiere_plan_empresa")]
        public required bool RequierePlanEmpresa { get; set; } = false;

		[Required]
        [Column("vigencia")]
        public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }
    }
}
