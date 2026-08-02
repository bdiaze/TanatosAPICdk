using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class FiscalizadorNormaSuscritaBcpTest {
		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public static FiscalizadorNormaSuscrita FiscalizadorNormaSuscritaDummy(
			long id = 1,
			long idNormaSuscrita = 10,
			long idTipoFiscalizador = 100,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			IdNormaSuscrita = idNormaSuscrita,
			IdTipoFiscalizador = idTipoFiscalizador,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};
	}
}
