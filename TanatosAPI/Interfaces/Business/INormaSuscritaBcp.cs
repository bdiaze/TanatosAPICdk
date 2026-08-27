using Npgsql;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;

namespace TanatosAPI.Interfaces.Business {
	public interface INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita);
		public bool Pertenece(NormaSuscrita normaSuscrita, string sub);
		public bool PerteneceNegocio(NormaSuscrita normaSuscrita, long idNegocio);
		public bool EstaActiva(NormaSuscrita normaSuscrita);
		public bool EsEditable(NormaSuscrita normaSuscrita);
		public List<NormaSuscrita> FiltrarVigentes(List<NormaSuscrita> normasSuscritas);
		public Task<NormaSuscrita?> Obtener(long idNormaSuscrita, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, bool validarEditable = false, NpgsqlTransaction? transaction = null);
		public Task<List<NormaSuscrita>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<NormaSuscrita> CrearObligacionUsuario(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, NpgsqlTransaction? transaction = null);
		public Task Actualizar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Activar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Desactivar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Eliminar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
    }
}
