using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(INormaSuscritaDao normaSuscritaDao) {
		public bool EstaVigente(NormaSuscrita? normaSuscrita) {
			return normaSuscrita != null && normaSuscrita.Vigencia;
		}

		public bool Pertenece(NormaSuscrita normaSuscrita, string sub) {
			return normaSuscrita.Sub == sub;
		}

		public async Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita) {
            return await normaSuscritaDao.ObtenerPorId(idNormaSuscrita);
        }
	}
}
