using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class NotificacionPreviaHelperTest {

		private static TipoUnidadTiempo TipoUnidadTiempoDummy(
			long id = 1, 
			string nombre = "NombreTest",
			string nombrePlural = "NombrePluralTest",
			long cantSegundos = 1,
			long? cantMinutos = null,
			long? cantHoras = null,
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
			Vigencia = vigencia
		};

		public static TheoryData<DateTime, TipoUnidadTiempo, DateTime> FechasReferencias => new() {
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantDias: 1), new DateTime(2026, 6, 14, 12, 30, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantHoras: 1), new DateTime(2026, 6, 15, 11, 30, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantMinutos: 1), new DateTime(2026, 6, 15, 12, 29, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantSegundos: 1), new DateTime(2026, 6, 15, 12, 29, 59, DateTimeKind.Unspecified) },
		};
		[Theory]
		[MemberData(nameof(FechasReferencias))]
		public async Task ObtenerFechaChileNotificacionPreviaTest(DateTime fechaReferencia, TipoUnidadTiempo unidadTiempo, DateTime expectedDateTimeResult) {
			DateTime retorno = NotificacionPreviaHelper.ObtenerFechaChileNotificacionPrevia(fechaReferencia, 1, unidadTiempo);
			Assert.Equal(expectedDateTimeResult, retorno);
		}

		public static TheoryData<DateTime, TipoUnidadTiempo, DateTime> FechasNotificaciones => new() {
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantDias: 1), new DateTime(2026, 6, 16, 12, 30, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantHoras: 1), new DateTime(2026, 6, 15, 13, 30, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantMinutos: 1), new DateTime(2026, 6, 15, 12, 31, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), TipoUnidadTiempoDummy(cantSegundos: 1), new DateTime(2026, 6, 15, 12, 30, 1, DateTimeKind.Unspecified) },
		};
		[Theory]
		[MemberData(nameof(FechasNotificaciones))]
		public async Task ObtenerFechaReferenciaChileSegunNotificacionPrevia(DateTime fechaNotificacion, TipoUnidadTiempo unidadTiempo, DateTime expectedDateTimeResult) {
			DateTime retorno = NotificacionPreviaHelper.ObtenerFechaReferenciaChileSegunNotificacionPrevia(fechaNotificacion, 1, unidadTiempo);
			Assert.Equal(expectedDateTimeResult, retorno);
		}
	}
}
