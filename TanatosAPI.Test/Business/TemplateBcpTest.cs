using Npgsql;
using NSubstitute;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Business {
	public class TemplateBcpTest {
		private readonly ITemplateDao templateDao = Substitute.For<ITemplateDao>();
		private readonly TemplateBcp templateBcp;

		public TemplateBcpTest() {
			templateBcp = new(templateDao);
		}

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

		public static TheoryData<Template?, bool> EstaVigenteCases => new() {
			{ TemplateDummy(vigencia: true), true },
			{ TemplateDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(Template? item, bool expectedResult) {
			Assert.Equal(expectedResult, templateBcp.EstaVigente(item));
		}

		[Fact]
		public async Task ObtenerTest_SinParametros() {
			templateDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TemplateDummy(id: 1));

			Template? template = await templateBcp.Obtener(1);
			Assert.NotNull(template);
			Assert.Equal(1, template.Id);
			await templateDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerTest_FiltrandoVigentes() {
			templateDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TemplateDummy(id: 1, vigencia: false));

			Template? template = await templateBcp.Obtener(1, filtrarVigente: true);
			Assert.Null(template);
			await templateDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			templateDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateDummy(id: 1, vigencia: true),
				TemplateDummy(id: 2, vigencia: true),
				TemplateDummy(id: 3, vigencia: true),
			]);

			List<Template> retorno = await templateBcp.ObtenerVigentes();
			Assert.Equal(3, retorno.Count);
			await templateDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVariosSoloVigentesTest() {
			templateDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateDummy(id: 1, vigencia: true),
				TemplateDummy(id: 2, vigencia: true),
				TemplateDummy(id: 3, vigencia: true),
			]);

			List<Template> retorno = await templateBcp.ObtenerVariosSoloVigentes([2, 3]);
			Assert.Equal(2, retorno.Count);
			Assert.DoesNotContain(1, retorno.Select(t => t.Id));
			Assert.Contains(2, retorno.Select(t => t.Id));
			Assert.Contains(3, retorno.Select(t => t.Id));
		}
	}
}
