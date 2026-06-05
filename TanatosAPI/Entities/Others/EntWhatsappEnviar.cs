using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntWhatsappEnviar {
		public required string Para { get; set; }

		public required string Cuerpo { get; set; }
	}
}
