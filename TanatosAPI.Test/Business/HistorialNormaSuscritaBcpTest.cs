using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
    public class HistorialNormaSuscritaBcpTest {
		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public static HistorialNormaSuscrita HistorialNormaSuscritaDummy(
            long id = 1,
            long idNormaSuscrita = 10,
            DateTime? fechaVencimiento = null,
            DateTime? fechaCompletitud = null,
            DateTime? fechaCreacion = null,
            DateTime? fechaEliminacion = null,
            bool vigencia = true
		) => new() { 
            Id = id,
            IdNormaSuscrita = idNormaSuscrita,
            FechaVencimiento = fechaVencimiento ?? FECHA_DUMMY,
            FechaCompletitud = fechaCompletitud,
            FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
            FechaEliminacion = fechaEliminacion,
            Vigencia = vigencia
		};
    }
}
