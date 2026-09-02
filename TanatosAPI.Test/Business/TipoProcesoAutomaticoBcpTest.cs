using Npgsql;
using NSubstitute;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Test.Business {
	public class TipoProcesoAutomaticoBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly ITipoProcesoAutomaticoDao tipoProcesoAutomaticoDao = Substitute.For<ITipoProcesoAutomaticoDao>();
		private readonly TipoProcesoAutomaticoBcp tipoProcesoAutomaticoBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public TipoProcesoAutomaticoBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			tipoProcesoAutomaticoBcp = new(dateTimeProvider, tipoProcesoAutomaticoDao);
		}

		public static TipoProcesoAutomatico TipoProcesoAutomaticoDummy(
			long id = 1,
			string nombre = "nombre-test",
			string? descripcion = "descripcion-test",
			bool habilitado = true,
			int orden = 1,
			DateTime? fechaCreacion = null
		) => new() { 
			Id = id,
			Nombre = nombre,
			Descripcion = descripcion,
			Habilitado = habilitado,
			Orden = orden,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
		};

		public static TheoryData<TipoProcesoAutomatico, bool> EstaHabilitadoCases => new() {
			{ TipoProcesoAutomaticoDummy(habilitado: true), true },
			{ TipoProcesoAutomaticoDummy(habilitado: false), false },
		};
		[Theory]
		[MemberData(nameof(EstaHabilitadoCases))]
		public void EstaHabilitadoTest(TipoProcesoAutomatico item, bool expectedResult) {
			Assert.Equal(expectedResult, tipoProcesoAutomaticoBcp.EstaHabilitado(item));
		}

		[Fact]
		public void FiltrarHabilitadosTest() {
			List<TipoProcesoAutomatico> items = [
				TipoProcesoAutomaticoDummy(id: 1, habilitado: true),
				TipoProcesoAutomaticoDummy(id: 2, habilitado: false),
				TipoProcesoAutomaticoDummy(id: 3, habilitado: true)
			];

			List<TipoProcesoAutomatico> retorno = tipoProcesoAutomaticoBcp.FiltrarHabilitados(items);
			Assert.Equal(2, retorno.Count);
			Assert.Contains(1, retorno.Select(r => r.Id));
			Assert.Contains(3, retorno.Select(r => r.Id));
			Assert.DoesNotContain(2, retorno.Select(r => r.Id));
		}

		[Fact]
		public async Task ObtenerTest_SinParametros() {
			tipoProcesoAutomaticoDao.Obtener(10, Arg.Any<NpgsqlTransaction>()).Returns(TipoProcesoAutomaticoDummy(id: 10));

			TipoProcesoAutomatico? retorno = await tipoProcesoAutomaticoBcp.Obtener(10);
			Assert.NotNull(retorno);
			Assert.Equal(10, retorno.Id);
			await tipoProcesoAutomaticoDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task ObtenerTest_ValidandoNoHabilitado() {
			tipoProcesoAutomaticoDao.Obtener(10, Arg.Any<NpgsqlTransaction>()).Returns(TipoProcesoAutomaticoDummy(id: 10, habilitado: false));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoProcesoAutomaticoBcp.Obtener(10, validarHabilitado: true));
			Assert.Equal(TipoErrorValidacion.EstadoNoValido, ex.TipoErrorValidacion);
			await tipoProcesoAutomaticoDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task ObtenerTest_FiltrandoNoHabilitados() {
			tipoProcesoAutomaticoDao.Obtener(10, Arg.Any<NpgsqlTransaction>()).Returns(TipoProcesoAutomaticoDummy(id: 10, habilitado: false));

			TipoProcesoAutomatico? retorno = await tipoProcesoAutomaticoBcp.Obtener(10, filtrarHabilitado: true);
			Assert.Null(retorno);
			await tipoProcesoAutomaticoDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task ObtenerTodos_SinFiltros() {
			tipoProcesoAutomaticoDao.ObtenerTodos(Arg.Any<NpgsqlTransaction>()).Returns([
				TipoProcesoAutomaticoDummy(id: 1, habilitado: false),
				TipoProcesoAutomaticoDummy(id: 2, habilitado: true),
				TipoProcesoAutomaticoDummy(id: 3, habilitado: false),
				TipoProcesoAutomaticoDummy(id: 4, habilitado: true),
			]);

			List<TipoProcesoAutomatico> retorno = await tipoProcesoAutomaticoBcp.ObtenerTodos();
			Assert.Equal(4, retorno.Count);
			Assert.Contains(1, retorno.Select(r => r.Id));
			Assert.Contains(2, retorno.Select(r => r.Id));
			Assert.Contains(3, retorno.Select(r => r.Id));
			Assert.Contains(4, retorno.Select(r => r.Id));
			await tipoProcesoAutomaticoDao.Received(1).ObtenerTodos(Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task ObtenerTodos_TodosLosFiltros() {
			tipoProcesoAutomaticoDao.ObtenerTodos(Arg.Any<NpgsqlTransaction>()).Returns([
				TipoProcesoAutomaticoDummy(id: 1, habilitado: false),
				TipoProcesoAutomaticoDummy(id: 2, habilitado: true),
				TipoProcesoAutomaticoDummy(id: 3, habilitado: false),
				TipoProcesoAutomaticoDummy(id: 4, habilitado: true),
			]);

			List<TipoProcesoAutomatico> retorno = await tipoProcesoAutomaticoBcp.ObtenerTodos(filtrarHabilitados: true);
			Assert.Equal(2, retorno.Count);
			Assert.DoesNotContain(1, retorno.Select(r => r.Id));
			Assert.Contains(2, retorno.Select(r => r.Id));
			Assert.DoesNotContain(3, retorno.Select(r => r.Id));
			Assert.Contains(4, retorno.Select(r => r.Id));
			await tipoProcesoAutomaticoDao.Received(1).ObtenerTodos(Arg.Any<NpgsqlTransaction>());
		}
	}
}
