namespace TanatosAPI.Entities.Others {
	public class SalNormaSuscritaObtenerConVencimiento {
		public required DateTime FechaVencimiento { get; set; }
		public required long IdNormaSuscrita { get; set; }
		public string? NombreNorma { get; set; }
		public string? DescripcionNorma { get; set; }
		public string? MultaNorma { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
	}
}
