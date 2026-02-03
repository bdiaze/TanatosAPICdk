namespace TanatosAPI.Entities.Others {
	public class SalDocumentoAdjuntoGenerarUrlSubida {
		public required long IdDocumentoAdjunto { get; set; }
		public required string PreSignedUrl { get; set; }
	}
}
