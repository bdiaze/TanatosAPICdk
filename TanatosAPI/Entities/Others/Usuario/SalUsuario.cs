using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Usuario {
	[ExcludeFromCodeCoverage]
	public class SalUsuario {
		public string? Nombre { get; set; }
		public string? Apellido { get; set; }
		public string? Email { get; set; }
		public string? Whatsapp { get; set; }
		public bool? WhatsappValidado { get; set; }
		public required bool TienePlanEmpresa { get; set; }
	}
}
