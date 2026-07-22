using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NegocioBcp(INegocioDao negocioDao) : INegocioBcp {
		public bool EstaVigente(Negocio? negocio) {
			return negocio != null && negocio.Vigencia;
		}

		public bool PerteneceAlUsuario(Negocio negocio, string sub) {
			return negocio.Sub == sub;
		}

		public async Task<Negocio?> Obtener(long idNegocio, NpgsqlTransaction? transaction = null) {
			return await negocioDao.Obtener(idNegocio, transaction);
		}

        public async Task<Negocio> ObtenerValidandoVigencia(long idNegocio, NpgsqlTransaction? transaction = null) {
            Negocio? negocio = await Obtener(idNegocio, transaction);
            if (!EstaVigente(negocio)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El negocio es inválido.");
			return negocio!;
        }

        public async Task<Negocio> ObtenerValidandoVigenciaYPertenencia(long idNegocio, string sub, NpgsqlTransaction? transaction = null) {
			Negocio? negocio = await ObtenerValidandoVigencia(idNegocio, transaction);
            if (!PerteneceAlUsuario(negocio!, sub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El negocio es inválido.");
			return negocio!;
		}

		public async Task<Negocio?> ObtenerVigentePorSubYNegocio(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			return (await negocioDao.ObtenerPorSub(sub, true, transaction)).FirstOrDefault(n => n.Id == idNegocio);
        }

		public async Task<List<Negocio>>ObtenerVigentesPorSub(string sub, NpgsqlTransaction? transaction = null) {
			return await negocioDao.ObtenerPorSub(sub, true, transaction);
		}
	}
}
