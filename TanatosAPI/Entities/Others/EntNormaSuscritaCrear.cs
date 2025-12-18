namespace TanatosAPI.Entities.Others {
	public class EntNormaSuscritaCrear {
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public required long IdTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public required long IdCategoriaNorma { get; set; }
		public List<EntFiscalizadorNormaSuscritaCrear>? Fiscalizadores { get; set; }
	}

	public class EntFiscalizadorNormaSuscritaCrear {
		public required long IdTipoFiscalizador { get; set; }
	}
}
