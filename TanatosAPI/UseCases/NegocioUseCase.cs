using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NegocioUseCase(IDateTimeProvider dateTimeProvider, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp, INegocioDao negocioDao, INormaSuscritaDao normaSuscritaDao, NormaSuscritaUseCase normaSuscritaUseCase) {
		public async Task<bool> NegocioAccesible(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			// Se valida que el negocio sea del usuario...
			List<Negocio> negocios = await negocioBcp.ObtenerVigentesPorSub(sub, transaction);
			Negocio? negocioSeleccionado = negocios.FirstOrDefault(n => n.Id == idNegocio);
			if (negocioSeleccionado == null) return false;

			// Se valida si el usuario tiene plan Empresa...
			bool tienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(sub, transaction);
			if (tienePlanEmpresa) return true;

			// Dado que no tiene plan Empresa, se valida si el negocio corresponde al primer negocio creado por el usuario...
			Negocio primerNegocio = negocios.OrderBy(n => n.FechaCreacion).First();
			if (primerNegocio.Id != negocioSeleccionado.Id) return false;
			else return true;
		}

		public async Task EliminarNegocio(Negocio negocio, NpgsqlTransaction? transaction = null) {
			if (negocio.Vigencia) {
				negocio.FechaEliminacion = dateTimeProvider.UtcNow;
				negocio.Vigencia = false;
				await negocioDao.Actualizar(negocio, transaction);

				List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(negocio.Sub, negocio.Id, true, transaction);
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}
	}
}
