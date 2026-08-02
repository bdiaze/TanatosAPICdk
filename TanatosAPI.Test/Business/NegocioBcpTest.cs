using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class NegocioBcpTest {
		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);
		public static Negocio NegocioDummy(
			long id = 5,
			string sub = "sub-test",
			string nombre = "nombre-test",
			string direccion = "direccion-test",
			long idTipoActividad = 100,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Sub = sub,
			Nombre = nombre,
			Direccion = direccion,
			IdTipoActividad = idTipoActividad,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};
	}
}
