using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DestinatarioNotificacion {
    [ExcludeFromCodeCoverage]
    public class SalDestinatarioNotificacion {
		public long Id { get; set; }
		public long IdTipoReceptor { get; set; }
		public string? NombreTipoReceptor { get; set; }
		public bool? RequierePlanEmpresa { get; set; }
		public string? Alias { get; set; }
		public string Destino { get; set; } = null!;
		public bool Validado { get; set; }
	}
}
