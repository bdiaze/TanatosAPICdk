namespace TanatosAPI.Entities.Others {
	public class EntInscripcionTemplateActivar {
		public required long IdNegocio { get; set; }

		public required long IdTemplate { get; set; }

		public required bool ActivarPadres { get; set; } = false;
	}
}
