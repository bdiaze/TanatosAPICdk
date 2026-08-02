using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TipoFiscalizadorBcpTest {
		public static TipoFiscalizador TipoFiscalizadorDummy(
			long id = 1,
			string nombre = "nombre-test",
			string? nombreCorto = "nombre-corto-test",
			bool vigencia = true
		) => new() {
			Id = id,
			Nombre = nombre,
			NombreCorto = nombreCorto,
			Vigencia = vigencia,
		};
	}
}
