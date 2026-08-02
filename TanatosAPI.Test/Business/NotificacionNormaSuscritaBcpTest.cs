using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class NotificacionNormaSuscritaBcpTest {
		public static NotificacionNormaSuscrita NotificacionNormaSuscritaDummy(
			long id = 1,
			long idNormaSuscrita = 10,
			long idTipoUnidadTiempoAntelacion = 100,
			int cantAntelacion = 2,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() {
			Id = id,
			IdNormaSuscrita = idNormaSuscrita,
			IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
			CantAntelacion = cantAntelacion,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia,
		};
	}
}
