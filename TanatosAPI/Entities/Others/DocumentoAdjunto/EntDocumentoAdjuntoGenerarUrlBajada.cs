using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DocumentoAdjunto {
    [ExcludeFromCodeCoverage]
    public class EntDocumentoAdjuntoGenerarUrlBajada {
		public required long IdDocumentoAdjunto { get; set; }
		public bool ParaVisualizacion { get; set; } = false;
	}
}
