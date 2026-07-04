using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DocumentoAdjunto {
    [ExcludeFromCodeCoverage]
    public class SalDocumentoAdjunto {
		public required long Id { get; set; }
		public required string NombreArchivo { get; set; }
		public DateTime? FechaSubida { get; set; }
	}
}
