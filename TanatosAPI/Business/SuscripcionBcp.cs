using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class SuscripcionBcp(SuscripcionDao suscripcionDao, NegocioDao negocioDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) {
		public async Task<bool> TienePlanEmpresa(string sub, NpgsqlTransaction? transaction = null) {
			// Se obtienen las suscripciones del usuario...
			List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true, transaction);

			if (suscripciones.Any(s => s.FechaExpiracion != null && s.FechaExpiracion > DateTime.UtcNow)) {
				return true;
			}

			return false;
		}

		public async Task<bool> NegocioAccesible(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			// Se valida que el negocio sea del usuario...
			List<Negocio> negocios = await negocioDao.ObtenerPorSub(sub, true, transaction);
			Negocio? negocioSeleccionado = negocios.FirstOrDefault(n => n.Id == idNegocio);
			if (negocioSeleccionado == null) return false;

			// Se valida si el usuario tiene plan Empresa...
			bool tienePlanEmpresa = await TienePlanEmpresa(sub, transaction);
			if (tienePlanEmpresa) return true;

			// Dado que no tiene plan Empresa, se valida si el negocio corresponde al primer negocio creado por el usuario...
			Negocio primerNegocio = negocios.OrderBy(n => n.FechaCreacion).First();
			if (primerNegocio.Id != negocioSeleccionado.Id) return false;
			else return true;
		}

		public async Task<bool> DestinatarioHabilitado(string sub, long idNegocio, long idDestinatario, NpgsqlTransaction? transaction = null) {
			// Se valida si el negocio es accesible...
			bool negocioAccesible = await NegocioAccesible(sub, idNegocio, transaction);
			if (!negocioAccesible) return false;

			// Se valida que el destinatario sea del negocio y este validado...
			List<DestinatarioNotificacion> destinatarios = await destinatarioNotificacionDao.ObtenerPorSub(sub, idNegocio, true, transaction);
			DestinatarioNotificacion? destinatarioSeleccionado = destinatarios.FirstOrDefault(d => d.Id == idDestinatario);
			if (destinatarioSeleccionado == null || !destinatarioSeleccionado.Validado) return false;

			// Se valida si el usuario tiene plan Empresa...
			bool tienePlanEmpresa = await TienePlanEmpresa(sub, transaction);
			if (tienePlanEmpresa) return true;

			// Dado que no tiene plan Empresa, se valida si el tipo de receptor requiere plan Empresa...
			TipoReceptorNotificacion? tipoReceptorDestinatario = await tipoReceptorNotificacionDao.ObtenerPorId(destinatarioSeleccionado.IdTipoReceptor, transaction);
			if (tipoReceptorDestinatario == null || !tipoReceptorDestinatario.Vigencia || tipoReceptorDestinatario.RequierePlanEmpresa) {
				return false;
			}

			// Dado que no tiene plan Empresa, se valida si el destinatario corresponde al primer destinatario creado por el usuario...
			DestinatarioNotificacion primerDestinatario = destinatarios.OrderBy(d => d.FechaCreacion).First();
			if (primerDestinatario.Id != destinatarioSeleccionado.Id) return false;
			else return true;
		}
	}
}
