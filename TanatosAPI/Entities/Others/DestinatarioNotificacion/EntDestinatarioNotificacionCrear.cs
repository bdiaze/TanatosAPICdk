using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.DestinatarioNotificacion {
    [ExcludeFromCodeCoverage]
    public class EntDestinatarioNotificacionCrear {
		public required long IdNegocio { get; set; }
		public required long IdTipoReceptor { get; set; }
		public string? Alias { get; set; }
		public required string Destino { get; set; }
	}
}
