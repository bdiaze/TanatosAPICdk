using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.NormaSuscrita {
    [ExcludeFromCodeCoverage]
    public class SalNormaSuscritaObtenerConVencimiento {
		public required DateTime? FechaVencimiento { get; set; }
		public required DateTime? FechaCompletitud { get; set; }
		public long? IdTemplate { get; set; }
		public long? IdNorma { get; set; }
		public required long IdNormaSuscrita { get; set; }
		public required long? IdHistorialNormaSuscrita { get; set; }
		public string? NombreTemplate { get; set; }
		public string? NombreNorma { get; set; }
		public string? DescripcionNorma { get; set; }
		public string? MultaNorma { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
		public long? IdCargo { get; set; }
		public string? NombreCargo { get; set; }
		public required bool Activado { get; set; }
	}
}
