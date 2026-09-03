using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Negocio {
    [ExcludeFromCodeCoverage]
    public class EntNegocioCrear {
		public required string Nombre { get; set; }
		public string? Direccion { get; set; }
		public long? IdTipoActividad { get; set; }
	}
}
