namespace TanatosAPI.Entities.Others {
	public class SalDestinatarioNotificacion {
		public long Id { get; set; }
		public long IdTipoReceptor { get; set; }
		public string Destino { get; set; } = null!;
		public bool Validado { get; set; }
	}
}
