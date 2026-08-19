using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class KairosHelperTest {
		private readonly IKairosHttpClient httpClient = Substitute.For<IKairosHttpClient>();
		private readonly KairosHelper kairosHelper;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public KairosHelperTest() {
			kairosHelper = new(httpClient);
		}

		[Fact]
		public async Task IngresarProceso_Ok() {
			httpClient.PostAsync("Procesos", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalKairosIngresarProceso {
						IdProceso = "id-proceso-test",
						IdCalendarizacion = "id-calendarizacion-test",
						Nombre = "nombre-test",
						ArnRol = "arn-rol-test",
						ArnProceso = "arn-proceso-test",
						Parametros = "parametros-test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalKairosIngresarProceso retorno = await kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
				Nombre = "nombre-test",
				Cron = "cron-test",
				ArnRol = "arn-rol-test",
				ArnProceso = "arn-proceso-test",
				Parametros = "parametros-test"
			});
			Assert.Equal("id-proceso-test", retorno.IdProceso);
			Assert.Equal("id-calendarizacion-test", retorno.IdCalendarizacion);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("arn-rol-test", retorno.ArnRol);
			Assert.Equal("arn-proceso-test", retorno.ArnProceso);
			Assert.Equal("parametros-test", retorno.Parametros);
			await httpClient.Received(1).PostAsync("Procesos", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task IngresarProceso_Error() {
			httpClient.PostAsync("Procesos", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
				Nombre = "nombre-test",
				Cron = "cron-test",
				ArnRol = "arn-rol-test",
				ArnProceso = "arn-proceso-test",
				Parametros = "parametros-test"
			}));
			await httpClient.Received(1).PostAsync("Procesos", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task EliminarProceso_Ok() {
			httpClient.DeleteAsync(Arg.Is<string>(s => s.StartsWith("Procesos/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK
			});

			await kairosHelper.EliminarProceso("id-proceso-test");
			await httpClient.Received(1).DeleteAsync(Arg.Is<string>(s => s.StartsWith("Procesos/")));
		}

		[Fact]
		public async Task EliminarProceso_Error() {
			httpClient.DeleteAsync(Arg.Is<string>(s => s.StartsWith("Procesos/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => kairosHelper.EliminarProceso("id-proceso-test"));
			await httpClient.Received(1).DeleteAsync(Arg.Is<string>(s => s.StartsWith("Procesos/")));
		}
	}
}
