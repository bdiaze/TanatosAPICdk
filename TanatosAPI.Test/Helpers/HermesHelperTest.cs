using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Business;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class HermesHelperTest {
		private readonly IHermesHttpClient httpClient = Substitute.For<IHermesHttpClient>();
		private readonly HermesHelper hermesHelper;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public HermesHelperTest() {
			hermesHelper = new(httpClient);
		}

		[Fact]
		public async Task EnviarCorreo_Ok() {
			httpClient.PostAsync("Correo/Enviar", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalHermesEnviar {
						IdMensaje = "id-mensaje-test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar {
				Para = [
					new DireccionCorreo {
						Correo = "correo@test.cl"
					}
				],
				Asunto = "AsuntoTest",
				Cuerpo = "CuerpoTest"
			});
			Assert.Equal("id-mensaje-test", retorno.IdMensaje);
			await httpClient.Received(1).PostAsync("Correo/Enviar", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task EnviarCorreo_Error() {
			httpClient.PostAsync("Correo/Enviar", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar {
				Para = [
					new DireccionCorreo {
						Correo = "correo@test.cl"
					}
				],
				Asunto = "AsuntoTest",
				Cuerpo = "CuerpoTest"
			}));
			await httpClient.Received(1).PostAsync("Correo/Enviar", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task EnviarWhatsapp_Ok() {
			httpClient.PostAsync("Whatsapp/Enviar", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalHermesEnviar {
						IdMensaje = "id-mensaje-test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar {
				De = "de-test",
				Para = "para-test",
			});
			Assert.Equal("id-mensaje-test", retorno.IdMensaje);
			await httpClient.Received(1).PostAsync("Whatsapp/Enviar", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task EnviarWhatsapp_Error() {
			httpClient.PostAsync("Whatsapp/Enviar", Arg.Any<StringContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar {
				De = "de-test",
				Para = "para-test",
			}));
			await httpClient.Received(1).PostAsync("Whatsapp/Enviar", Arg.Any<StringContent>());
		}

		[Fact]
		public async Task ObtenerMedia_Ok() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Media/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalHermesWhatsappMedia {
						Url = "https://url.test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalHermesWhatsappMedia retorno = await hermesHelper.ObtenerMedia("whatsapp-message-id");
			Assert.Equal("https://url.test", retorno.Url);
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Media/")));
		}

		[Fact]
		public async Task ObtenerMedia_Error() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Media/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => hermesHelper.ObtenerMedia("whatsapp-message-id"));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Media/")));
		}

		public static TheoryData<string, DateTime?, DateTime?> TenantsConFecha => new() {
			{ "tenant-id-test", null, null },
			{ "tenant-id-test", FECHA_DUMMY, null},
			{ "tenant-id-test", FECHA_DUMMY, FECHA_DUMMY },
		};

		[Theory]
		[MemberData(nameof(TenantsConFecha))]
		public async Task ObtenerConversaciones_Ok(string tenantId, DateTime? desde, DateTime? hasta) {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Conversaciones/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new List<SalHermesWhatsappConversacion>([
						new SalHermesWhatsappConversacion {
							TenantId = tenantId,
							NumeroTelefono = "telefono-test",
							FechaUltimoMensaje = FECHA_DUMMY,
							CantidadNoLeidos = 5,
							Estado = "estado-test"
						}
					])),
					Encoding.UTF8,
					"application/json"
				)
			});

			List<SalHermesWhatsappConversacion> retorno = await hermesHelper.ObtenerConversaciones(tenantId, desde, hasta);
			Assert.Single(retorno);
			Assert.All(retorno, r => Assert.Equal(tenantId, r.TenantId));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Conversaciones/")));
		}

		[Fact]
		public async Task ObtenerConversaciones_Error() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Conversaciones/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => hermesHelper.ObtenerConversaciones("tenant-id-test", null, null));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Conversaciones/")));
		}

		public static TheoryData<string, string, DateTime?, DateTime?> TenantsTelefonoConFecha => new() {
			{ "tenant-id-test", "numero-telefono-test", null, null },
			{ "tenant-id-test", "numero-telefono-test", FECHA_DUMMY, null},
			{ "tenant-id-test", "numero-telefono-test", FECHA_DUMMY, FECHA_DUMMY },
		};

		[Theory]
		[MemberData(nameof(TenantsTelefonoConFecha))]
		public async Task ObtenerMensajes_Ok(string tenantId, string telefono, DateTime? desde, DateTime? hasta) {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Mensajes/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new List<SalHermesWhatsappMensaje>([
						new SalHermesWhatsappMensaje {
							TenantId = tenantId,
							NumeroTelefono = telefono,
							WhatsappMessageId = "whatsapp-message-id-test",
							Direccion = "direccion-test",
							Tipo = "tipo-test",
							Estado = "estado-test",
							FechaCreacion = FECHA_DUMMY
						}
					])),
					Encoding.UTF8,
					"application/json"
				)
			});

			List<SalHermesWhatsappMensaje> retorno = await hermesHelper.ObtenerMensajes(tenantId, telefono, desde, hasta);
			Assert.Single(retorno);
			Assert.All(retorno, r => Assert.Equal(tenantId, r.TenantId));
			Assert.All(retorno, r => Assert.Equal(telefono, r.NumeroTelefono));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Mensajes/")));
		}

		[Fact]
		public async Task ObtenerMensajes_Error() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Mensajes/"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => hermesHelper.ObtenerMensajes("tenant-id-test", "telefono-test", null, null));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("Whatsapp/Mensajes/")));
		}
	}
}
