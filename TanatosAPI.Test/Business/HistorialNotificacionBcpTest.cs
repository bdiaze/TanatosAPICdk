using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class HistorialNotificacionBcpTest {
		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public static HistorialNotificacion HistorialNotificacionDummy(
			long id = 100,
			long idHistorialNormaSuscrita = 10,
			long idDestinatarioNotificacion = 1_000,
			long? idTipoUnidadTiempoAntelacion = null,
			int? cantAntelacion = null,
			DateTime? fechaProgramacion = null,
			DateTime? fechaEjecucion = null,
			short? estado = 0,
			string? observacion = null,
			string? codigoAcceso = null,
			DateTime? fechaCaducidadCodigoAcceso = null,
			string? hermesIdMensaje = null,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
			IdDestinatarioNotificacion = idDestinatarioNotificacion,
			IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
			CantAntelacion = cantAntelacion,
			FechaProgramacion = fechaProgramacion ?? FECHA_DUMMY,
			FechaEjecucion = fechaEjecucion,
			Estado = estado,
			Observacion = observacion,
			CodigoAcceso = codigoAcceso,
			FechaCaducidadCodigoAcceso = fechaCaducidadCodigoAcceso,
			HermesIdMensaje = hermesIdMensaje,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};
	}
}
