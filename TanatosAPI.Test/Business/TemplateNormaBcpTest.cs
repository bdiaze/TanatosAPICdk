using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TemplateNormaBcpTest {
		public static TemplateNorma TemplateNormaDummy(
			long idTemplate = 10,
			long idNorma = 100,
			string nombre = "nombre-test",
			string? descripcion = "descripcion-test",
			string? multa = "multa-test",
			long? idTipoPeriodicidad = 10,
			long idCategoriaNorma = 20,
			string? cronActivacionAutomatica = null,
			int? diasActivacionAutomatica = null
		) => new() {  
			IdTemplate = idTemplate,
			IdNorma = idNorma,
			Nombre = nombre,
			Descripcion = descripcion,
			Multa = multa,
			IdTipoPeriodicidad = idTipoPeriodicidad,
			IdCategoriaNorma = idCategoriaNorma,
			CronActivacionAutomatica = cronActivacionAutomatica,
			DiasActivacionAutomatica = diasActivacionAutomatica
		};
	}
}
