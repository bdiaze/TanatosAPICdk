using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("cargo", Schema = "tanatos")]
    public class Cargo {
        [Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("sub")]
        public required string Sub { get; set; }

        [Required]
        [Column("id_negocio")]
        public required long IdNegocio { get; set; }

        [Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("fecha_creacion", TypeName = "timestamp with time zone")]
        public required DateTime FechaCreacion { get; set; }

        [Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
        public DateTime? FechaEliminacion { get; set; }

        [Required]
        [Column("vigencia")]
        public required bool Vigencia { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(IdNegocio))]
        public Negocio? Negocio { get; set; }

		[JsonIgnore]
		public List<Empleado>? Empleados { get; set; }

        [JsonIgnore]
        public List<NormaSuscrita>? NormasSuscritas { get; set; }
    }
}
