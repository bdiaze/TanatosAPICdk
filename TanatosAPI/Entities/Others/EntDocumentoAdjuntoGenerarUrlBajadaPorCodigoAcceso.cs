using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntDocumentoAdjuntoGenerarUrlBajadaPorCodigoAcceso {
		public required string CodigoAcceso { get; set; }
		public required long IdDocumentoAdjunto { get; set; }
	}
}
