using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("documento_adjunto", Schema = "tanatos")]
	[Comment("Tabla que contiene la metadata de los documentos adjuntos asociados al historial de ejecución de una norma suscrita.")]
	[Index(nameof(BucketName), nameof(BucketKey))]
	public class DocumentoAdjunto {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del documento adjunto.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_historial_norma_suscrita")]
		[Comment("Identificador del historial de ejecución de una norma suscrita.")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("bucket_name")]
		[Comment("Nombre del bucket donde está almacenado el documento.")]
		public required string BucketName { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("bucket_key")]
		[Comment("Identificador del objeto dentro del bucket.")]
		public required string BucketKey { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre_archivo")]
		[Comment("Nombre original del archivo.")]
		public required string NombreArchivo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("mime_esperado")]
		[Comment("Mime esperado del archivo.")]
		public required string MimeEsperado { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("tamanno_esperado")]
		[Comment("Tamaño esperado del archivo en bytes.")]
		public required long TamannoEsperado { get; set; }

		[UseColumnAttribute]
		[Column("mime_real")]
		[Comment("Mime real del archivo.")]
		public string? MimeReal { get; set; }

		[UseColumnAttribute]
		[Column("tamanno_real")]
		[Comment("Tamaño real del archivo en bytes.")]
		public long? TamannoReal { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("estado_subida")]
		[Comment("Estado de subida del documento adjunto. 0: Generada URL prefirmada para PUT - 1: Documento recepcionado.")]
		public required short EstadoSubida { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_emision_url_prefirmada_put", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se emitió la URL prefirmada para método PUT.")]
		public required DateTime FechaEmisionUrlPrefirmadaPut { get; set; }

		[UseColumnAttribute]
		[Column("fecha_confirmacion_subida", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se confirmó la subida del archivo.")]
		public DateTime? FechaConfirmacionSubida { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el registro.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el registro.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del registro.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdHistorialNormaSuscrita))]
		public HistorialNormaSuscrita? HistorialNormaSuscrita { get; set; }
	}
}
