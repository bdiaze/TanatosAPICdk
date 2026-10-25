using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class MovimientoFinancieroUseCase(IDatabaseConnectionHelper connectionHelper, IMovimientoFinancieroBcp movimientoFinancieroBcp, ICuentaFinancieraBcp cuentaFinancieraBcp, INegocioBcp negocioBcp, ICategoriaFinancieraBcp categoriaFinancieraBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp) {
		public async Task<(List<MovimientoFinanciero> items, (DateTime siguienteFecha, long siguienteId)?)> ObtenerConPaginacion(string sub, long idNegocio, long idCuentaFinanciera, DateTime fechaDesde, DateTime fechaHasta, DateTime? primeraFecha = null, long? primerId = null) {
			// Se valida que el negocio este vigente y pertenezca al usuario...
			_ = await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub);

			// Se valida que la cuenta este vigente y pertenezca al usuario y negocio...
			_ = await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio);

			int cantRegistros = 50;

			if (cantRegistros <= 0 || cantRegistros > 100) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "La cantidad de registros debe estar entre 1 y 100.");
			}

			List<MovimientoFinanciero> items = await movimientoFinancieroBcp.ObtenerConPaginacion(idCuentaFinanciera, fechaDesde, fechaHasta, vigencia: true, primeraFecha, primerId, cantRegistros + 1);
			bool hayMas = items.Count > cantRegistros;

			(DateTime, long)? siguiente = null;
			if (hayMas) {
				siguiente = (items[^1].FechaMovimiento, items[^1].Id);
				items.RemoveAt(items.Count - 1);
			}

			if (items.Count > 0) {
				Dictionary<long, CategoriaFinanciera> categorias = (await categoriaFinancieraBcp.ObtenerPorSub(sub, idNegocio, filtrarVigentes: true)).ToDictionary(c => c.Id, c => c);
				items.ForEach(i => {
					i.CategoriaFinanciera = i.IdCategoriaFinanciera != null ? categorias.GetValueOrDefault(i.IdCategoriaFinanciera.Value) : null;
				});
			}

			return (items, siguiente);
		}

		public async Task<MovimientoFinanciero> Crear(string sub, long idCuentaFinanciera, long? idCategoriaFinanciera, short tipoMovimiento, decimal monto, DateTime fechaMovimientoUtc, string? descripcion, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				descripcion = descripcion?.Trim();
				if (string.IsNullOrWhiteSpace(descripcion)) descripcion = null;

				if (!new[] { 1, 2 }.Contains(tipoMovimiento)) {
					throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "El tipo de movimiento es inválido.");
				}

				if (monto < 0) {
					throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El monto del movimiento es inválido.");
				}

				// Se valida que cuenta financiera perteneza al usuario y este vigente...
				CuentaFinanciera cuenta = (await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarVigencia: true, validarSub: sub, forUpdate: true, transaction: transaction!.NpgsqlTransaction()))!;
				
				// Se valida que el negocio pertenezca al usuario y este vigente...
				Negocio negocio = (await negocioBcp.Obtener(cuenta.IdNegocio, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction()))!;

				// Se valida que la categoría pertenezca al usuario y este vigente...
				CategoriaFinanciera? categoria = null;
				if (idCategoriaFinanciera != null) {
					categoria = await categoriaFinancieraBcp.Obtener(idCategoriaFinanciera.Value, validarVigencia: true, validarSub: sub, validarIdNegocio: negocio.Id, transaction: transaction!.NpgsqlTransaction());
				}

				if (fechaMovimientoUtc <= cuenta.FechaSaldoInicial)
					throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La fecha del movimiento debe ser posterior a la fecha del saldo inicial de la cuenta: {DateTimeHelper.TransformarFechaUTCATimezone(cuenta.FechaSaldoInicial):dd-MM-yyyy HH:mm}.");

				// Se obtiene movimiento anterior...
				MovimientoFinanciero? anterior = (await movimientoFinancieroBcp.ObtenerPrevios(
					idCuentaFinanciera, 
					fechaMovimientoUtc, 
					idReferencia: null,
					vigencia: true,
					cantidad: 1,
					transaction: transaction!.NpgsqlTransaction()
				)).FirstOrDefault();

				decimal saldoAnterior;
				if (anterior != null) {
					saldoAnterior = anterior.Saldo;
				} else {
					saldoAnterior = cuenta.SaldoInicial;
				}

				MovimientoFinanciero nuevo = await movimientoFinancieroBcp.Crear(
					idCuentaFinanciera,
					idCategoriaFinanciera, 
					tipoMovimiento, 
					monto,
					saldo: saldoAnterior + (tipoMovimiento == 2 ? -1 : 1) * monto, 
					fechaMovimientoUtc,
					descripcion,
					transaction!.NpgsqlTransaction()
				);
				nuevo.CategoriaFinanciera = categoria;

				List<MovimientoFinanciero> posteriores = await movimientoFinancieroBcp.ObtenerPosteriores(
					idCuentaFinanciera, 
					nuevo.FechaMovimiento,
					nuevo.Id,
					vigencia: true,
					transaction: transaction!.NpgsqlTransaction()
				);

				decimal saldoParcial = nuevo.Saldo;
				foreach (MovimientoFinanciero posterior in posteriores) {
					posterior.Saldo = saldoParcial + (posterior.TipoMovimiento == 2 ? -1 : 1) * posterior.Monto;
					await movimientoFinancieroBcp.Actualizar(posterior, transaction!.NpgsqlTransaction());
					saldoParcial = posterior.Saldo;
				}

				cuenta.SaldoActual = cuenta.SaldoActual + (tipoMovimiento == 2 ? -1 : 1) * monto;
				await cuentaFinancieraBcp.Actualizar(cuenta, transaction!.NpgsqlTransaction());

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return nuevo;
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task Eliminar(string sub, long idNegocio, long idCuentaFinanciera, long idMovimientoFinanciero, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				// Se valida que la cuenta financiera perteneza al usuario y este vigente...
				CuentaFinanciera cuenta = (await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio, forUpdate: true, transaction: transaction!.NpgsqlTransaction()))!;

				// Se valida que el negocio pertenezca al usuario y este vigente...
				_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction()))!;

				// Se ejecuta lógica de eliminación solo si el movimiento esta vigente...
				MovimientoFinanciero? movimiento = await movimientoFinancieroBcp.Obtener(idMovimientoFinanciero, filtrarVigente: true, validarIdCuentaFinanciera: idCuentaFinanciera, transaction: transaction!.NpgsqlTransaction());
				if (movimiento != null) {
					// Se obtiene movimiento anterior...
					MovimientoFinanciero? anterior = (await movimientoFinancieroBcp.ObtenerPrevios(
						movimiento.IdCuentaFinanciera,
						movimiento.FechaMovimiento,
						movimiento.Id,
						vigencia: true,
						cantidad: 1,
						transaction: transaction!.NpgsqlTransaction()
					)).FirstOrDefault();

					// Se calcula el saldo previo al movimiento a eliminar...
					decimal saldoAnterior;
					if (anterior != null) {
						saldoAnterior = anterior.Saldo;
					} else {
						saldoAnterior = cuenta.SaldoInicial;
					}

					// Se obtienen los movimientos posteriores al movimiento a eliminar...
					List<MovimientoFinanciero> posteriores = await movimientoFinancieroBcp.ObtenerPosteriores(
						movimiento.IdCuentaFinanciera,
						movimiento.FechaMovimiento,
						movimiento.Id,
						vigencia: true,
						transaction: transaction!.NpgsqlTransaction()
					);

					// Se actualizan los saldos posteriores...
					decimal saldoParcial = saldoAnterior;
					foreach (MovimientoFinanciero posterior in posteriores) {
						posterior.Saldo = saldoParcial + (posterior.TipoMovimiento == 2 ? -1 : 1) * posterior.Monto;
						await movimientoFinancieroBcp.Actualizar(posterior, transaction!.NpgsqlTransaction());
						saldoParcial = posterior.Saldo;
					}

					// Se actualiza el saldo actual de la cuenta...
					cuenta.SaldoActual = cuenta.SaldoActual + (movimiento.TipoMovimiento == 2 ? 1 : -1) * movimiento.Monto;
					await cuentaFinancieraBcp.Actualizar(cuenta, transaction!.NpgsqlTransaction());

					// Se elimina el movimiento en cuestión...
					await movimientoFinancieroBcp.Eliminar(movimiento, transaction!.NpgsqlTransaction());
					await documentoAdjuntoBcp.EliminarPorMovimientoFinanciero(movimiento.Id, transaction!.NpgsqlTransaction());
				}


				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task<MovimientoFinanciero> Actualizar(string sub, long idNegocio, long idCuentaFinanciera, long idMovimientoFinanciero, 
														   long? idCategoriaFinanciera, short tipoMovimiento, decimal monto, 
														   DateTime fechaMovimientoUtc, string? descripcion, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				descripcion = descripcion?.Trim();
				if (string.IsNullOrWhiteSpace(descripcion)) descripcion = null;

				if (!new[] { 1, 2 }.Contains(tipoMovimiento)) {
					throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "El tipo de movimiento es inválido.");
				}

				if (monto < 0) {
					throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El monto del movimiento es inválido.");
				}

				// Se valida que cuenta financiera perteneza al usuario y este vigente...
				CuentaFinanciera cuenta = (await cuentaFinancieraBcp.Obtener(idCuentaFinanciera, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio, forUpdate: true, transaction: transaction!.NpgsqlTransaction()))!;

				// Se valida que el negocio pertenezca al usuario y este vigente...
				_ = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction()))!;

				// Se valida que el movimiento este vigente...
				MovimientoFinanciero movimiento = (await movimientoFinancieroBcp.Obtener(idMovimientoFinanciero, validarVigencia: true, validarIdCuentaFinanciera: idCuentaFinanciera, transaction: transaction!.NpgsqlTransaction()))!;
								
				// Se valida que la categoría pertenezca al usuario y este vigente...
				CategoriaFinanciera? categoria = null;
				if (idCategoriaFinanciera != null) {
					categoria = await categoriaFinancieraBcp.Obtener(idCategoriaFinanciera.Value, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio, transaction: transaction!.NpgsqlTransaction());
				}

				if (fechaMovimientoUtc <= cuenta.FechaSaldoInicial)
					throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La fecha del movimiento debe ser posterior a la fecha del saldo inicial de la cuenta: {DateTimeHelper.TransformarFechaUTCATimezone(cuenta.FechaSaldoInicial):dd-MM-yyyy HH:mm}.");

				// Solo se ejecuta la edición si alguno de los valores cambia...
				if (movimiento.IdCategoriaFinanciera != idCategoriaFinanciera || movimiento.TipoMovimiento != tipoMovimiento || movimiento.Monto != monto ||
					movimiento.FechaMovimiento != fechaMovimientoUtc || movimiento.Descripcion != descripcion) {

					movimiento.IdCategoriaFinanciera = idCategoriaFinanciera;
					movimiento.Descripcion = descripcion;

					// Solo se recalculan los saldos si se modifica un valor que afecta a los saldos...
					if (movimiento.TipoMovimiento != tipoMovimiento || movimiento.Monto != monto || movimiento.FechaMovimiento != fechaMovimientoUtc) {
						DateTime fechaReferencia = movimiento.FechaMovimiento > fechaMovimientoUtc ? fechaMovimientoUtc : movimiento.FechaMovimiento;

						// Se obtiene movimiento anterior según nueva fecha...
						MovimientoFinanciero? anterior = (await movimientoFinancieroBcp.ObtenerPrevios(
							idCuentaFinanciera,
							fechaReferencia,
							movimiento.Id,
							vigencia: true,
							cantidad: 1,
							transaction: transaction!.NpgsqlTransaction()
						)).FirstOrDefault();

						decimal saldoAnterior;
						if (anterior != null) {
							saldoAnterior = anterior.Saldo;
						} else {
							saldoAnterior = cuenta.SaldoInicial;
						}

						cuenta.SaldoActual += (movimiento.TipoMovimiento == 2 ? 1 : -1) * movimiento.Monto;
						cuenta.SaldoActual += (tipoMovimiento == 2 ? -1 : 1) * monto;
						await cuentaFinancieraBcp.Actualizar(cuenta, transaction!.NpgsqlTransaction());

						movimiento.TipoMovimiento = tipoMovimiento;
						movimiento.Monto = monto;
						movimiento.FechaMovimiento = fechaMovimientoUtc;
						await movimientoFinancieroBcp.Actualizar(movimiento, transaction: transaction!.NpgsqlTransaction());

						// Se obtienen los movimientos posteriores al movimiento a eliminar...
						List<MovimientoFinanciero> posteriores = await movimientoFinancieroBcp.ObtenerPosteriores(
							idCuentaFinanciera,
							fechaReferencia,
							movimiento.Id,
							vigencia: true,
							incluirReferencia: true,
							transaction: transaction!.NpgsqlTransaction()
						);

						// Se actualizan los saldos posteriores...
						decimal saldoParcial = saldoAnterior;
						foreach (MovimientoFinanciero posterior in posteriores) {
							posterior.Saldo = saldoParcial + (posterior.TipoMovimiento == 2 ? -1 : 1) * posterior.Monto;
							await movimientoFinancieroBcp.Actualizar(posterior, transaction!.NpgsqlTransaction());
							if (posterior.Id == movimiento.Id) movimiento.Saldo = posterior.Saldo;

							saldoParcial = posterior.Saldo;
						}
					} else {
						await movimientoFinancieroBcp.Actualizar(movimiento, transaction: transaction!.NpgsqlTransaction());
					}
				}

				movimiento.CategoriaFinanciera = categoria;

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return movimiento;
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}
	}
}
