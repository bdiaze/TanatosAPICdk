using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Negocio {
    [ExcludeFromCodeCoverage]
    public class SalNegocio {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public string? Direccion { get; set; }
		public long? IdTipoActividad { get; set; }
		public string? NombreTipoActividad { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
