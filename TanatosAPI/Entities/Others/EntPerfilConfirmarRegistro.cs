using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
	[ExcludeFromCodeCoverage]
	public class EntPerfilConfirmarRegistro {
        public required string Username { get; set; }
        public required string Codigo { get; set; }
    }
}
