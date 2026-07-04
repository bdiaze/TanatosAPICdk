using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IVideoTutorialDao {
		public Task<List<VideoTutorial>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(VideoTutorial item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(VideoTutorial item, NpgsqlTransaction? transaction = null);
	}
}
