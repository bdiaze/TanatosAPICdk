namespace TanatosAPI.Entities.Others {
	public class SalEmpleado {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public long? IdCargo { get; set; }
		public string? NombreCargo { get; set; }
		public List<SalEmpleadoDestinatario> Destinatarios { get; set; } = [];
	}

	public class  SalEmpleadoDestinatario {
		public required long Id { get; set; }
		public required long IdTipoReceptor { get; set; }
		public required string NombreTipoReceptor { get; set; }
		public required bool TipoReceptorRequierePlanEmpresa { get; set; }
		public required string Destino { get; set; }
		public required bool Validado { get; set; }
	}
}
