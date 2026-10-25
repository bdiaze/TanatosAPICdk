using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DocumentoAdjunto {
    [ExcludeFromCodeCoverage]
    public class EntDocumentoAdjuntoGenerarUrlSubida {
		public required string NombreArchivo { get; set; }
		public required string Mime { get; set; }
		public required long Tamanno { get; set; }
		public long? IdHistorialNormaSuscrita { get; set; }
		public long? IdMovimientoFinanciero { get; set; }
	}
}
