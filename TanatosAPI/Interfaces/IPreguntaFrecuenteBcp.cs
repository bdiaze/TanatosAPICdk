using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
	public interface IPreguntaFrecuenteBcp {
		public bool EstaVigente(PreguntaFrecuente? preguntaFrecuente);
		public Task<List<PreguntaFrecuente>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<PreguntaFrecuente> Insertar(string pregunta, string respuesta, bool habilitado, int orden, NpgsqlTransaction? transaction = null);
		public Task Modificar(PreguntaFrecuente preguntaFrecuente, NpgsqlTransaction? transaction = null);
		public Task Eliminar(PreguntaFrecuente preguntaFrecuente, NpgsqlTransaction? transaction = null);
	}
}
