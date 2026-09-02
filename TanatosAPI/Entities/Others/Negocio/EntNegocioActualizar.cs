using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Negocio {
    [ExcludeFromCodeCoverage]
    public class EntNegocioActualizar {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public string? Direccion { get; set; }
		public long? IdTipoActividad { get; set; }
		public string? Mision { get; set; }
		public string? Vision { get; set; }
		public string? Valores { get; set; }
	}
}
