using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Hermes {
    [ExcludeFromCodeCoverage]
    public class EntHermesCorreoEnviar {
		public DireccionCorreo? De { get; set; }
		public required List<DireccionCorreo> Para { get; set; }
		public List<DireccionCorreo>? Cc { get; set; }
		public List<DireccionCorreo>? Cco { get; set; }
		public List<DireccionCorreo>? ResponderA { get; set; }
		public required string Asunto { get; set; }
		public required string Cuerpo { get; set; }
		public List<Adjunto>? Adjuntos { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class DireccionCorreo {
		public string? Nombre { get; set; }
		public required string Correo { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class Adjunto {
		public required string NombreArchivo { get; set; }
		public required string TipoMime { get; set; }
		public required string ContenidoBase64 { get; set; }
	}
}
