using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Test.Business {
	public class TemplateBcpTest {
		public static Template TemplateDummy(
			long id = 10,
			long? idTemplatePadre = null,
			string nombre = "nombre-test",
			string descripcion = "descripcion-test",
			bool requierePlanEmpresa = false,
			bool vigencia = true
		) => new() { 
			Id = id,
			IdTemplatePadre = idTemplatePadre,
			Nombre = nombre,
			Descripcion = descripcion,
			RequierePlanEmpresa = requierePlanEmpresa,
			Vigencia = vigencia
		};
	}
}
