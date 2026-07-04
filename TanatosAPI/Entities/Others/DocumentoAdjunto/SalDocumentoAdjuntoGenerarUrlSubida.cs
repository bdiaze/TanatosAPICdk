using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DocumentoAdjunto {
    [ExcludeFromCodeCoverage]
    public class SalDocumentoAdjuntoGenerarUrlSubida {
		public required long IdDocumentoAdjunto { get; set; }
		public required string PreSignedUrl { get; set; }
		public Dictionary<string, string>? PreSignedFields { get; set; }
	}
}
