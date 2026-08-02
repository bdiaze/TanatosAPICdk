using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TemplateNormaFiscalizadorBcpTest {
		public static TemplateNormaFiscalizador TemplateNormaFiscalizadorDummy(
			long idTemplate = 10,
			long idNorma = 100,
			long idTipoFiscalizador = 1000
		) => new() { 
			IdTemplate = idTemplate,
			IdNorma = idNorma,
			IdTipoFiscalizador = idTipoFiscalizador
		};
	}
}
