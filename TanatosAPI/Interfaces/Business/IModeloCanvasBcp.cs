using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IModeloCanvasBcp {
		public bool EstaVigente(ModeloCanvas? item);
		public bool Pertenece(ModeloCanvas item, string sub);
		public bool PerteneceNegocio(ModeloCanvas item, long idNegocio);
		public List<ModeloCanvas> FiltrarVigentes(List<ModeloCanvas> items);
		public Task<ModeloCanvas?> Obtener(long idModeloCanvas, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<List<ModeloCanvas>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<ModeloCanvas> Insertar(string sub, long idNegocio, string? sociosClave, string? actividadesClave, string? recursosClave, string? propuestaValor, string? relacionesClientes, string? canales, string? segmentosClientes, string? estructuraCostos, string? fuentesIngresos, NpgsqlTransaction? transaction = null);
		public Task Modificar(ModeloCanvas modeloCanvas, NpgsqlTransaction? transaction = null);
		public Task Eliminar(ModeloCanvas modeloCanvas, NpgsqlTransaction? transaction = null);
	}
}
