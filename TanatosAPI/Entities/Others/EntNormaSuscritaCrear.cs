namespace TanatosAPI.Entities.Others {
	public class EntNormaSuscritaCrear {
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public required bool Activado { get; set; }
		public List<EntFiscalizadorNormaSuscritaCrear>? Fiscalizadores { get; set; }
		public List<EntNotificacionNormaSuscritaCrear>? Notificaciones { get; set; }
		public DateTime? ProximoVencimiento { get; set; }
	}

	public class EntFiscalizadorNormaSuscritaCrear {
		public required long IdTipoFiscalizador { get; set; }
	}

	public class EntNotificacionNormaSuscritaCrear {
		public required long IdTipoUnidadTiempoAntelacion { get; set; }
		public required int CantAntelacion { get; set; }
	}
}
