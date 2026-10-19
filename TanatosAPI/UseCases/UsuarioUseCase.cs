using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.UseCases;

namespace TanatosAPI.UseCases {
	public class UsuarioUseCase(IWhatsappUsuarioUseCase whatsappUsuarioUseCase, ISuscripcionBcp suscripcionBcp, IUsuarioBcp usuarioBcp) {
		public async Task<(Usuario, WhatsappUsuario?, bool tienePlanEmpresa)> ObtenerConWhatsappYPlanEmpresa(string sub, NpgsqlTransaction? transaction = null) {
			Usuario usuario = await usuarioBcp.Obtener(sub, transaction) ?? throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "No se encuentra registro del usuario.");
			WhatsappUsuario? whatsappUsuario = await whatsappUsuarioUseCase.ObtenerUltimoVigente(sub, transaction: transaction);
			bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction);
			return (usuario, whatsappUsuario, tienePlanEmpresa);
		}
	}
}
