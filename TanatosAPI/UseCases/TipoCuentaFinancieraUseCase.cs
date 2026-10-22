using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class TipoCuentaFinancieraUseCase(ITipoCuentaFinancieraBcp tipoCuentaFinancieraBcp) {
		public async Task<List<TipoCuentaFinanciera>> ObtenerVigentes() {
			return await tipoCuentaFinancieraBcp.ObtenerTodos(filtrarVigentes: true);
		}

		public async Task<List<TipoCuentaFinanciera>> ObtenerTodos() {
			return await tipoCuentaFinancieraBcp.ObtenerTodos();
		}

		public async Task<TipoCuentaFinanciera> Crear(long id, string nombre, bool vigencia) {
			TipoCuentaFinanciera? existente = await tipoCuentaFinancieraBcp.Obtener(id);
			if (existente != null) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe un tipo de cuenta financiera con ese ID.");

			return await tipoCuentaFinancieraBcp.Crear(id, nombre, vigencia);
		}

		public async Task<TipoCuentaFinanciera> Actualizar(long id, string nombre, bool vigencia) {
			TipoCuentaFinanciera existente = await tipoCuentaFinancieraBcp.Obtener(id) ?? throw new ErrorValidacion(TipoErrorValidacion.NoExiste, "No existe un tipo de cuenta financiera con ese ID.");
			if (existente.Nombre != nombre || existente.Vigencia != vigencia) {
				existente.Nombre = nombre;
				existente.Vigencia = vigencia;
				await tipoCuentaFinancieraBcp.Actualizar(existente);
			}
			return existente;
		}

		public async Task Eliminar(long id) {
			TipoCuentaFinanciera? existente = await tipoCuentaFinancieraBcp.Obtener(id);
			if (existente != null) {
				await tipoCuentaFinancieraBcp.Eliminar(existente.Id);
			}
		}
	}
}
