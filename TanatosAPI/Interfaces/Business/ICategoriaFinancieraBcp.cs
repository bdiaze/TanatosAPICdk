using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICategoriaFinancieraBcp {
		public bool EstaVigente(CategoriaFinanciera? item);
		public bool Pertenece(CategoriaFinanciera item, string sub);
		public bool PerteneceNegocio(CategoriaFinanciera item, long idNegocio);
		public List<CategoriaFinanciera> FiltrarVigentes(List<CategoriaFinanciera> items);
		public Task<CategoriaFinanciera?> Obtener(long id, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<List<CategoriaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<CategoriaFinanciera> Crear(string sub, long idNegocio, string nombre, short tipoMovimiento, NpgsqlTransaction? transaction = null);
		public Task Actualizar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null);
	}
}
