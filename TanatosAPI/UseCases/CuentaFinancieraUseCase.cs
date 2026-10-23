using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class CuentaFinancieraUseCase(ICuentaFinancieraBcp cuentaFinancieraBcp, ITipoCuentaFinancieraBcp tipoCuentaFinancieraBcp, INegocioBcp negocioBcp) {
		public async Task<List<CuentaFinanciera>> ObtenerVigentes(string sub, long idNegocio) {
			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub))!;

			List<CuentaFinanciera> items = await cuentaFinancieraBcp.ObtenerPorSub(sub, idNegocio, filtrarVigentes: true);
			if (items.Count > 0) {
				Dictionary<long, TipoCuentaFinanciera> tipos = (await tipoCuentaFinancieraBcp.ObtenerTodos(filtrarVigentes: true)).ToDictionary(t => t.Id, t => t);
				items.ForEach(i => {
					i.TipoCuentaFinanciera = tipos.GetValueOrDefault(i.IdTipoCuentaFinanciera);
				});
			}

			return items;
		}

		public async Task<CuentaFinanciera> Crear(string sub, long idNegocio, string nombre, long idTipoCuentaFinanciera, decimal saldoInicial, DateTime fechaSaldoInicialUtc) {
			nombre = nombre.Trim();

			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub))!;

			// Se valida que el tipo de cuenta este vigente...
			TipoCuentaFinanciera tipo = (await tipoCuentaFinancieraBcp.Obtener(idTipoCuentaFinanciera, validarVigencia: true))!;

			// Se valida que el usuario no tenga otra cuenta con el mismo nombre...
			List<CuentaFinanciera> cuentas = await cuentaFinancieraBcp.ObtenerPorSub(sub, idNegocio, filtrarVigentes: true);
			if (cuentas.Any(c => c.Nombre.Equals(nombre, StringComparison.InvariantCultureIgnoreCase)))
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya tienes una cuenta con ese nombre.");

			CuentaFinanciera nuevo = await cuentaFinancieraBcp.Crear(sub, idNegocio, nombre, idTipoCuentaFinanciera, saldoInicial, fechaSaldoInicialUtc);
			nuevo.TipoCuentaFinanciera = tipo;
			return nuevo;
		}

		public async Task<CuentaFinanciera> Actualizar(string sub, long idCuentaFinanciera, string nombre, long idTipoCuentaFinanciera) {
			nombre = nombre.Trim();

			// Se valida que la cuenta esté vigente y pertenezca al usuario...
			CuentaFinanciera existente = (await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarVigencia: true, validarSub: sub))!;

			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = (await negocioBcp.Obtener(existente.IdNegocio, validarVigencia: true, validarSub: sub))!;

			// Se valida que el tipo de cuenta este vigente...
			TipoCuentaFinanciera tipo = (await tipoCuentaFinancieraBcp.Obtener(idTipoCuentaFinanciera, validarVigencia: true))!;

			// Se valida que el usuario no tenga otra cuenta con el mismo nombre...
			List<CuentaFinanciera> cuentas = await cuentaFinancieraBcp.ObtenerPorSub(existente.Sub, existente.IdNegocio, filtrarVigentes: true);
			if (cuentas.Any(c => c.Id != idCuentaFinanciera && c.Nombre.Equals(nombre, StringComparison.InvariantCultureIgnoreCase)))
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya tienes una cuenta con ese nombre.");

			if (existente.Nombre != nombre || existente.IdTipoCuentaFinanciera != idTipoCuentaFinanciera) {
				existente.Nombre = nombre;
				existente.IdTipoCuentaFinanciera = idTipoCuentaFinanciera;

				await cuentaFinancieraBcp.Actualizar(existente);
			}

			existente.TipoCuentaFinanciera = tipo;

			return existente;
		} 

		public async Task Eliminar(string sub, long idCuentaFinanciera) {
			// Se valida que la cuenta pertenezca al usuario...
			CuentaFinanciera? existente = await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarSub: sub);

			if (existente != null) {
				// Se valida que el negocio este vigente y pertenezca al usuario...
				_ = (await negocioBcp.Obtener(existente.IdNegocio, validarVigencia: true, validarSub: sub))!;

				await cuentaFinancieraBcp.Eliminar(existente);
			}
		}
	}
}
