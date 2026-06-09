using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NegocioBcp(IDateTimeProvider dateTimeProvider, NormaSuscritaBcp normaSuscritaBcp, NegocioDao negocioDao, NormaSuscritaDao normaSuscritaDao) : INegocioBcp {
		public bool EstaVigente(Negocio? negocio) {
			return negocio != null && negocio.Vigencia;
		}

		public bool PerteneceAlUsuario(Negocio negocio, string sub) {
			return negocio.Sub == sub;
		}

		public async Task<Negocio?> ObtenerPorId(long idNegocio, NpgsqlTransaction? transaction = null) {
			return await negocioDao.Obtener(idNegocio, transaction);
		}

		public async Task<Negocio> ObtenerPorIdValidandoVigenciaYPertenencia(long idNegocio, string sub, NpgsqlTransaction? transaction = null) {
			Negocio? negocio = await ObtenerPorId(idNegocio, transaction);
			if (!EstaVigente(negocio)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El negocio es inválido.");
			}

			if (!PerteneceAlUsuario(negocio!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El negocio es inválido.");
			}
			return negocio!;
		}

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
