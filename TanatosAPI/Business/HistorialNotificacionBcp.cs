using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNotificacionBcp(HistorialNotificacionDao historialNotificacionDao, TipoUnidadTiempoDao tipoUnidadTiempoDao) {
		public async Task ActualizarPorHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, HashSet<(long IdDestinatarioNotificacion, long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> historialesNotificaciones, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificacionesExistentes = await historialNotificacionDao.ObtenerPorHistorial(historialNormaSuscrita.Id, null, true, transaction);
			
			// Se eliminan los historiales de notificaciones existentes que no se incluyen en la entrada...
			foreach (HistorialNotificacion historialExistente in historialNotificacionesExistentes) {
				if (!historialesNotificaciones.Any(n => n.IdDestinatarioNotificacion == historialExistente.IdDestinatarioNotificacion && n.IdTipoUnidadTiempoAntelacion == historialExistente.IdTipoUnidadTiempoAntelacion && n.CantAntelacion == historialExistente.CantAntelacion)) {
					await Eliminar(historialExistente, transaction);
				}
			}

			List<TipoUnidadTiempo> tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);

			// Se agregan los nuevos historiales de  notificaciones...
			foreach ((long IdDestinatarioNotificacion, long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion) historialNotificacion in historialesNotificaciones) {
				if (!historialNotificacionesExistentes.Any(ne => ne.IdDestinatarioNotificacion == historialNotificacion.IdDestinatarioNotificacion && ne.IdTipoUnidadTiempoAntelacion == historialNotificacion.IdTipoUnidadTiempoAntelacion && ne.CantAntelacion == historialNotificacion.CantAntelacion)) {
					
					// Si viene la antelación, se calcula la fecha de programación y se registra...
					if (historialNotificacion.IdTipoUnidadTiempoAntelacion != null && historialNotificacion.CantAntelacion != null) {
						TipoUnidadTiempo? tipoUnidadTiempo = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == historialNotificacion.IdTipoUnidadTiempoAntelacion);
						
						if (tipoUnidadTiempo != null) {
							long segundosPrevios = historialNotificacion.CantAntelacion.Value * tipoUnidadTiempo.CantSegundos;
							DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

							await Crear(
								historialNormaSuscrita.Id,
								historialNotificacion.IdDestinatarioNotificacion,
								historialNotificacion.IdTipoUnidadTiempoAntelacion,
								historialNotificacion.CantAntelacion,
								fechaProgramacion,
								transaction
							);
						}
					// Si no viene la antelación, se programa para la fecha de vencimiento...
					} else {
						await Crear(
							historialNormaSuscrita.Id, 
							historialNotificacion.IdDestinatarioNotificacion, 
							null, 
							null, 
							historialNormaSuscrita.FechaVencimiento,
							transaction
						);
					}
				}
			}
		}

		public async Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificacionesEliminar = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificacionesEliminar) {
				await Eliminar(historialNotificacionEliminar, transaction);
			}
		}

		public async Task EliminarPorHistorialNormaSuscritaYAntelacion(long idHistorialNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificaciones = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificaciones.Where(hne => hne.IdTipoUnidadTiempoAntelacion == idTipoUnidadTiempoAntelacion && hne.CantAntelacion == cantAntelacion)) {
				await Eliminar(historialNotificacionEliminar, transaction);
			}
		}

		public async Task<HistorialNotificacion?> Crear(long idHistorialNormaSuscrita, long idDestinatarioNotificacion, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, DateTime fechaProgramacion, NpgsqlTransaction? transaction = null) {
			if (fechaProgramacion > DateTime.UtcNow) {
				HistorialNotificacion nuevo = new() {
					Id = 0,
					IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
					IdDestinatarioNotificacion = idDestinatarioNotificacion,
					IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
					CantAntelacion = cantAntelacion,
					FechaProgramacion = fechaProgramacion,
					Estado = 0, // Pendiente
					FechaCreacion = DateTime.UtcNow,
					FechaEliminacion = null,
					Vigencia = true
				};

				nuevo.Id = await historialNotificacionDao.Insertar(nuevo, transaction);

				return nuevo;
			} else {
				return null;
			}
		}

		public async Task Eliminar(HistorialNotificacion historialNotificacion, NpgsqlTransaction? transaction = null) {
			if (historialNotificacion.Vigencia) {
				historialNotificacion.FechaEliminacion = DateTime.UtcNow;
				historialNotificacion.Vigencia = false;

				await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
			}
		}
	}
}
