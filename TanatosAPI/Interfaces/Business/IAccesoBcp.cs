using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IAccesoBcp {
		public bool EstaVigente(Acceso? item);
		public bool EstaExpirado(Acceso item);
		public Task<Acceso?> ObtenerPorCodigo(string codigo, bool validarVigencia = false, bool validarExpiracion = false, NpgsqlTransaction? transaction = null);
		public Task<(Acceso, string Codigo)> Crear(TimeSpan? duracion = null, NpgsqlTransaction? transaction = null);
		public Task Eliminar(Acceso item, NpgsqlTransaction? transaction = null);
	}
}
