using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class TipoProcesoAutomaticoUseCase(ITipoProcesoAutomaticoBcp tipoProcesoAutomaticoBcp) {
		public async Task<List<TipoProcesoAutomatico>> ObtenerHabilitados() {
			return await tipoProcesoAutomaticoBcp.ObtenerTodos(filtrarVigentes: true, filtrarHabilitados: true);
		}

		public async Task<List<TipoProcesoAutomatico>> ObtenerVigentes() {
			return await tipoProcesoAutomaticoBcp.ObtenerTodos(filtrarVigentes: true);
		}

		public async Task<TipoProcesoAutomatico> Registrar(long id, string nombre, string? descripcion, bool habilitado, int orden) {
			nombre = nombre.Trim();
			descripcion = descripcion?.Trim();
			if (descripcion != null && descripcion.Length == 0) descripcion = null;

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El tipo de proceso automático no puede tener un orden menor o igual a 0.");
			}

			TipoProcesoAutomatico? mismoId = await tipoProcesoAutomaticoBcp.Obtener(id);
			if (mismoId != null) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicho tipo de proceso automático.");
			}

			List<TipoProcesoAutomatico> vigentes = await ObtenerVigentes();
			if (vigentes.Any(v => v.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicho tipo de proceso automático.");
			}

			if (vigentes.Any(p => p.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un tipo de proceso automático con dicho orden.");
			}

			return await tipoProcesoAutomaticoBcp.Insertar(id, nombre, descripcion, habilitado, orden);
		}

		public async Task<TipoProcesoAutomatico> Actualizar(long id, string nombre, string? descripcion, bool habilitado, int orden) {
			nombre = nombre.Trim();
			descripcion = descripcion?.Trim();
			if (descripcion != null && descripcion.Length == 0) descripcion = null;

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El tipo de proceso automático no puede tener un orden menor o igual a 0.");
			}

			TipoProcesoAutomatico? existente = await tipoProcesoAutomaticoBcp.Obtener(id);
			if (existente == null) {
				throw new ErrorValidacion(TipoErrorValidacion.NoExiste, "No existe dicho tipo de proceso automático.");
			}

			List<TipoProcesoAutomatico> vigentes = await ObtenerVigentes();
			if (vigentes.Any(v => v.Id != id && v.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicho tipo de proceso automático.");
			}

			if (vigentes.Any(v => v.Id != id && v.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un tipo de proceso automático con dicho orden.");
			}

			if (existente.Nombre != nombre || existente.Descripcion != descripcion || existente.Habilitado != habilitado || existente.Orden != orden) {
				existente.Nombre = nombre;
				existente.Descripcion = descripcion;
				existente.Habilitado = habilitado;
				existente.Orden = orden;

				await tipoProcesoAutomaticoBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task Eliminar(long idTipoProcesoAutomatico) {
			TipoProcesoAutomatico? existente = await tipoProcesoAutomaticoBcp.Obtener(idTipoProcesoAutomatico);

			if (!tipoProcesoAutomaticoBcp.EstaVigente(existente)) {
				return;
			}

			await tipoProcesoAutomaticoBcp.Eliminar(existente!);
		}
	}
}
