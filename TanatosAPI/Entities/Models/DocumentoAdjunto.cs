using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("documento_adjunto", Schema = "tanatos")]
	public class DocumentoAdjunto {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_historial_norma_suscrita")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("bucket_name")]
		public required string BucketName { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("bucket_key")]
		public required string BucketKey { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre_archivo")]
		public required string NombreArchivo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("mime_esperado")]
		public required string MimeEsperado { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("tamanno_esperado")]
		public required long TamannoEsperado { get; set; }

		[UseColumnAttribute]
		[Column("mime_real")]
		public string? MimeReal { get; set; }

		[UseColumnAttribute]
		[Column("tamanno_real")]
		public long? TamannoReal { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("estado_subida")]
		public required short EstadoSubida { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_emision_url_prefirmada_put", TypeName = "timestamp with time zone")]
		public required DateTime FechaEmisionUrlPrefirmadaPut { get; set; }

		[UseColumnAttribute]
		[Column("fecha_confirmacion_subida", TypeName = "timestamp with time zone")]
		public DateTime? FechaConfirmacionSubida { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdHistorialNormaSuscrita))]
		public HistorialNormaSuscrita? HistorialNormaSuscrita { get; set; }
	}
}
