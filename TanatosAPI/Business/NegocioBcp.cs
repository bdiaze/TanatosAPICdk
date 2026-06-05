using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NegocioBcp(IDateTimeProvider dateTimeProvider, NormaSuscritaBcp normaSuscritaBcp, NegocioDao negocioDao, NormaSuscritaDao normaSuscritaDao) {
		public async Task<Negocio?> ObtenerVigentePorSubYNegocio(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			return (await negocioDao.ObtenerPorSub(sub, true, transaction)).FirstOrDefault(n => n.Id == idNegocio);
        }
		
		public async Task EliminarNegocio(Negocio negocio, NpgsqlTransaction? transaction = null) {
			if (negocio.Vigencia) {
				negocio.FechaEliminacion = dateTimeProvider.UtcNow;
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
