namespace TanatosAPI.Entities.Others {
	public class EntDocumentoAdjuntoGenerarUrlSubidaPorCodigoAcceso {
		public required string CodigoAcceso { get; set; }
		public required string NombreArchivo { get; set; }
		public required string Mime { get; set; }
		public required long Tamanno { get; set; }
	}
}
