namespace TanatosAPI.Entities.Others {
	public class EntNormaSuscritaActualizar {
		public required long Id { get; set; }
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public required long IdTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public required long IdCategoriaNorma { get; set; }
		public required bool Activado { get; set; }
		public List<EntFiscalizadorNormaSuscritaActualizar>? Fiscalizadores { get; set; }
	}

	public class EntFiscalizadorNormaSuscritaActualizar {
		public required long IdTipoFiscalizador { get; set; }
	}
}
