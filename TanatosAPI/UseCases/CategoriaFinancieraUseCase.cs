using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class CategoriaFinancieraUseCase(ICategoriaFinancieraBcp categoriaFinancieraBcp, INegocioBcp negocioBcp) {
		public async Task<List<CategoriaFinanciera>> ObtenerVigentes(string sub, long idNegocio) {
			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub))!;

			return await categoriaFinancieraBcp.ObtenerPorSub(sub, idNegocio, filtrarVigentes: true);
		}

		public async Task<CategoriaFinanciera> Crear(string sub, long idNegocio, string nombre, short tipoMovimiento) {
			nombre = nombre.Trim();

			if (!new[] { 1, 2, 3 }.Contains(tipoMovimiento))
				throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "El tipo de movimiento es inválido.");

			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub))!;

			// Se valida que el usuario no tenga otra categoría con el mismo nombre...
			List<CategoriaFinanciera> items = await categoriaFinancieraBcp.ObtenerPorSub(sub, idNegocio, filtrarVigentes: true);
			if (items.Any(c => c.Nombre.Equals(nombre, StringComparison.InvariantCultureIgnoreCase)))
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya tienes una categoría con ese nombre.");

			return await categoriaFinancieraBcp.Crear(sub, idNegocio, nombre, tipoMovimiento);
		}

		public async Task<CategoriaFinanciera> Actualizar(string sub, long idCategoriaFinanciera, string nombre, short tipoMovimiento) {
			nombre = nombre.Trim();

			if (!new[] { 1, 2, 3 }.Contains(tipoMovimiento))
				throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "El tipo de movimiento es inválido.");

			// Se valida que la cuenta esté vigente y pertenezca al usuario...
			CategoriaFinanciera existente = (await categoriaFinancieraBcp.Obtener(idCategoriaFinanciera, validarVigencia: true, validarSub: sub))!;

			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(existente.IdNegocio, validarVigencia: true, validarSub: sub))!;

			// Se valida que el usuario no tenga otra cuenta con el mismo nombre...
			List<CategoriaFinanciera> items = await categoriaFinancieraBcp.ObtenerPorSub(existente.Sub, existente.IdNegocio, filtrarVigentes: true);
			if (items.Any(c => c.Id != idCategoriaFinanciera && c.Nombre.Equals(nombre, StringComparison.InvariantCultureIgnoreCase)))
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya tienes una categoría con ese nombre.");

			if (existente.Nombre != nombre || existente.TipoMovimiento != tipoMovimiento) {
				existente.Nombre = nombre;
				existente.TipoMovimiento = tipoMovimiento;

				await categoriaFinancieraBcp.Actualizar(existente);
			}

			return existente;
		}

		public async Task Eliminar(string sub, long idCategoriaFinanciera) {
			// Se valida que la categoría pertenezca al usuario...
			CategoriaFinanciera? existente = await categoriaFinancieraBcp.Obtener(idCategoriaFinanciera, validarSub: sub);

			if (existente != null) {
				// Se valida que el negocio este vigente y pertenezca al usuario...
				_ = (await negocioBcp.Obtener(existente.IdNegocio, validarVigencia: true, validarSub: sub))!;

				await categoriaFinancieraBcp.Eliminar(existente);
			}
		}
	}
}
