using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Perfil {
	[ExcludeFromCodeCoverage]
	public class SalPerfil {
		public string? Nombre { get; set; }
		public string? Apellido { get; set; }
		public string? Email { get; set; }
		public string? Whatsapp { get; set; }
		public bool? WhatsappValidado { get; set; }
		public required bool TienePlanEmpresa { get; set; }
	}
}
