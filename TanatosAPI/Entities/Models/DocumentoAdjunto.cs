using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("documento_adjunto", Schema = "tanatos")]
	public class DocumentoAdjunto {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_historial_norma_suscrita")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[Required]
		[Column("bucket_name")]
		public required string BucketName { get; set; }

		[Required]
		[Column("bucket_key")]
		public required string BucketKey { get; set; }

		[Required]
		[Column("nombre_archivo")]
		public required string NombreArchivo { get; set; }

		[Required]
		[Column("mime_esperado")]
		public required string MimeEsperado { get; set; }

		[Required]
		[Column("tamanno_esperado")]
		public required long TamannoEsperado { get; set; }

		[Column("mime_real")]
		public string? MimeReal { get; set; }

		[Column("tamanno_real")]
		public long? TamannoReal { get; set; }

        // Estado de subida del documento adjunto. 0: Generada URL prefirmada para PUT - 1: Documento recepcionado.
        [Required]
		[Column("estado_subida")]
        public required short EstadoSubida { get; set; }

		[Required]
		[Column("fecha_emision_url_prefirmada_put", TypeName = "timestamp with time zone")]
		public required DateTime FechaEmisionUrlPrefirmadaPut { get; set; }

		[Column("fecha_confirmacion_subida", TypeName = "timestamp with time zone")]
		public DateTime? FechaConfirmacionSubida { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdHistorialNormaSuscrita))]
		public HistorialNormaSuscrita? HistorialNormaSuscrita { get; set; }
	}
}
