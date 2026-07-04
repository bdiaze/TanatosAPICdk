using TanatosAPI.Entities.Others.Hermes;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IHermesHelper {
		public Task<SalHermesEnviar> EnviarCorreo(EntHermesCorreoEnviar correo);
		public Task<SalHermesEnviar> EnviarWhatsapp(EntHermesWhatsappEnviar whatsapp);
		public Task<SalHermesWhatsappMedia> ObtenerMedia(string whatsappMessageId);
		public Task<List<SalHermesWhatsappConversacion>> ObtenerConversaciones(string tenantId, DateTime? desde, DateTime? hasta);
		public Task<List<SalHermesWhatsappMensaje>> ObtenerMensajes(string tenantId, string numeroTelefono, DateTime? desde, DateTime? hasta);
	}
}
