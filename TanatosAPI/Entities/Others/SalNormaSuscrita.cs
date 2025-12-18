namespace TanatosAPI.Entities.Others {
	public class SalNormaSuscrita {
		public long Id { get; set; }
		public string? Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
		public long? OrdenVisual { get; set; }
		public bool Editable { get; set; }
		public required bool Activado { get; set; }
		public SalTemplateNorma? TemplateNorma { get; set; }
		public List<SalFiscalizadorNormaSuscrita>? Fiscalizadores { get; set; }
		public List<SalNotificacionNormaSuscrita>? Notificaciones { get; set; }
	}

	public class SalTemplateNorma {
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public required long IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
	}

	public class SalFiscalizadorNormaSuscrita {
		public long Id { get; set; }
		public long IdTipoFiscalizador { get; set; }
		public string? NombreTipoFiscalizador { get; set; }
	}

	public class SalNotificacionNormaSuscrita {
		public long Id { get; set; }
		public long IdTipoUnidadTiempoAntelacion { get; set; }
		public string? NombreTipoUnidadTiempoAntelacion { get; set; }
		public int CantAntelacion { get; set; }
	}
}
