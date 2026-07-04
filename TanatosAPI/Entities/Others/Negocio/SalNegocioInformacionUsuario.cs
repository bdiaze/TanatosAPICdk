using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Negocio {
    [ExcludeFromCodeCoverage]
    public class SalNegocioInformacionUsuario {
		public string? Nombre { get; set; }
		public string? Apellido { get; set; }
		public string? Email { get; set; }
		public required bool TienePlanEmpresa { get; set; }
	}
}
