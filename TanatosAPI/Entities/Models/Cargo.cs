using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("cargo", Schema = "tanatos")]
    [Comment("Tabla que contiene los cargos asociados a un negocio.")]
    [Index(nameof(Sub), nameof(IdNegocio))]
    public class Cargo {
        [UseColumnAttribute]
        [Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Identificador del cargo.")]
        public long Id { get; set; }

        [UseColumnAttribute]
        [Required]
        [Column("sub")]
        [Comment("Usuario al que pertenece el cargo.")]
        public required string Sub { get; set; }

        [UseColumnAttribute]
        [Required]
        [Column("id_negocio")]
        [Comment("Identificador del negocio del usuario.")]
        public required long IdNegocio { get; set; }

        [UseColumnAttribute]
        [Required]
        [Column("nombre")]
        [Comment("Nombre del cargo.")]
        public required string Nombre { get; set; }

        [UseColumnAttribute]
        [Required]
        [Column("fecha_creacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se creó el cargo.")]
        public required DateTime FechaCreacion { get; set; }

        [UseColumnAttribute]
        [Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se eliminó el cargo.")]
        public DateTime? FechaEliminacion { get; set; }

        [UseColumnAttribute]
        [Required]
        [Column("vigencia")]
        [Comment("Vigencia del cargo.")]
        public required bool Vigencia { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(IdNegocio))]
        public Negocio? Negocio { get; set; }
    }
}
