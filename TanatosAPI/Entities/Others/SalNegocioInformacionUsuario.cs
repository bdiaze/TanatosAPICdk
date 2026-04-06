namespace TanatosAPI.Entities.Others {
	public class SalNegocioInformacionUsuario {
		public string? Nombre { get; set; }
		public string? Apellido { get; set; }
		public string? Email { get; set; }
		public required bool TienePlanEmpresa { get; set; }
	}
}
