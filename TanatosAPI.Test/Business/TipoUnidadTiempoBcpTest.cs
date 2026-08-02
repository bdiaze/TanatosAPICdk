using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TipoUnidadTiempoBcpTest {
		public static TipoUnidadTiempo TipoUnidadTiempoDummy(
			long id = 1,
			string nombre = "nombre-test",
			string nombrePlural = "nombre-plural-test",
			long cantSegundos = 3600,
			long? cantMinutos = 60,
			long? cantHoras = 1,
			long? cantDias = null,
			bool vigencia = true
		) => new() {
			Id = id,
			Nombre = nombre,
			NombrePlural = nombrePlural,
			CantSegundos = cantSegundos,
			CantMinutos = cantMinutos,
			CantHoras = cantHoras,
			CantDias = cantDias,
			Vigencia = vigencia,
		};	
	}
}
