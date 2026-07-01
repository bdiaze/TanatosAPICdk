using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
	public interface IVideoTutorialBcp {
		public bool EstaVigente(VideoTutorial? videoTutorial);
		public Task<List<VideoTutorial>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<VideoTutorial> Insertar(string titulo, string? descripcion, string url, bool habilitado, int orden, NpgsqlTransaction? transaction = null);
		public Task Modificar(VideoTutorial videoTutorial, NpgsqlTransaction? transaction = null);
		public Task Eliminar(VideoTutorial videoTutorial, NpgsqlTransaction? transaction = null);
	}
}
