using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TemplateNormaNotificacionBcpTest {
		public static TemplateNormaNotificacion TemplateNormaNotificacionDummy(
			long idTemplate = 10,
			long idNorma = 100,
			long idTipoUnidadTiempoAntelacion = 1,
			int cantAntelacion = 2
		) => new() { 
			IdTemplate = idTemplate,
			IdNorma = idNorma,
			IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
			CantAntelacion = cantAntelacion
		};
	}
}
