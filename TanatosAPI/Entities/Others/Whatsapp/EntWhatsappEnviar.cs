using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Whatsapp {
    [ExcludeFromCodeCoverage]
    public class EntWhatsappEnviar {
		public required string Para { get; set; }

		public required string Cuerpo { get; set; }
	}
}
