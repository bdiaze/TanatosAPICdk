using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NegocioBcp(NormaSuscritaBcp normaSuscritaBcp, NegocioDao negocioDao, NormaSuscritaDao normaSuscritaDao) {
		public async Task EliminarNegocio(Negocio negocio, NpgsqlTransaction? transaction = null) {
			if (negocio.Vigencia) {
				negocio.FechaEliminacion = DateTime.UtcNow;
				negocio.Vigencia = false;
				await negocioDao.Actualizar(negocio, transaction);

				List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(negocio.Sub, negocio.Id, true, transaction);
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await normaSuscritaBcp.EliminarNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}
	}
}
