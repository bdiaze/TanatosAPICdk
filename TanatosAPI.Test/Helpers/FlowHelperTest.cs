using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Helpers {
	public class FlowHelperTest {
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly ISecretManagerHelper secretManagerHelper = Substitute.For<ISecretManagerHelper>();
		private readonly IFlowHttpClient httpClient = Substitute.For<IFlowHttpClient>();
		private readonly FlowHelper flowHelper;

		public FlowHelperTest() {
			variableEntorno.Obtener("SECRET_ARN_APP").Returns("SecretArnAppTest");
			variableEntorno.Obtener("FLOW_URL_CALLBACK").Returns("https://url.test/callback");
			Random rnd = new();
			string flowApiKeyDummy = rnd.Next(1000, 10000).ToString();
			string flowSecretKeyDummy = rnd.Next(1000, 10000).ToString();
			Dictionary<string, string> dummySecret = new() {
				["FlowApiKey"] = flowApiKeyDummy,
				["FlowSecretKey"] = flowSecretKeyDummy
			};
			secretManagerHelper.ObtenerSecreto("SecretArnAppTest").Returns(JsonSerializer.Serialize(dummySecret));

			flowHelper = new(variableEntorno, secretManagerHelper, httpClient);
		}

		[Fact]
		public async Task PlanCreate_Valido() {
			httpClient.PostAsync("plans/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowPlanCreate {
						PlanId = "PlanIdTest"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowPlanCreate retorno = await flowHelper.PlanCreate("PlanIdTest", "NombrePlanTest", 1000, 1);
			Assert.Equal("PlanIdTest", retorno.PlanId);
			await httpClient.Received(1).PostAsync("plans/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PlanCreate_StatusCodeError() {
			httpClient.PostAsync("plans/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.PlanCreate("PlanIdTest", "NombrePlanTest", 1000, 1));
			await httpClient.Received(1).PostAsync("plans/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PlanEdit_Valido() {
			httpClient.PostAsync("plans/edit", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowPlanEdit {
						PlanId = "PlanIdTest"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowPlanEdit retorno = await flowHelper.PlanEdit("PlanIdTest", "NombrePlanTest", 1000, 1);
			Assert.Equal("PlanIdTest", retorno.PlanId);
			await httpClient.Received(1).PostAsync("plans/edit", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PlanEdit_StatusCodeError() {
			httpClient.PostAsync("plans/edit", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.PlanEdit("PlanIdTest", "NombrePlanTest", 1000, 1));
			await httpClient.Received(1).PostAsync("plans/edit", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PlanDelete_Valido() {
			httpClient.PostAsync("plans/delete", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowPlanDelete {
						PlanId = "PlanIdTest"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowPlanDelete retorno = await flowHelper.PlanDelete("PlanIdTest");
			Assert.Equal("PlanIdTest", retorno.PlanId);
			await httpClient.Received(1).PostAsync("plans/delete", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PlanDelete_StatusCodeError() {
			httpClient.PostAsync("plans/delete", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.PlanDelete("PlanIdTest"));
			await httpClient.Received(1).PostAsync("plans/delete", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task CustomerCreate_Valido() {
			httpClient.PostAsync("customer/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowCustomerCreate {
						CustomerId = "CustomerIdTest"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowCustomerCreate retorno = await flowHelper.CustomerCreate("NombreTest", "CorreoTest", "SubTest");
			Assert.Equal("CustomerIdTest", retorno.CustomerId);
			await httpClient.Received(1).PostAsync("customer/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task CustomerCreate_StatusCodeError() {
			httpClient.PostAsync("customer/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.CustomerCreate("NombreTest", "CorreoTest", "SubTest"));
			await httpClient.Received(1).PostAsync("customer/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task CustomerRegister_Valido() {
			httpClient.PostAsync("customer/register", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowUrlToken {
						Url = "https://url.test",
						Token = "token-test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowUrlToken retorno = await flowHelper.CustomerRegister("CustomerIdTest");
			Assert.Equal("https://url.test", retorno.Url);
			Assert.Equal("token-test", retorno.Token);
			await httpClient.Received(1).PostAsync("customer/register", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task CustomerRegister_StatusCodeError() {
			httpClient.PostAsync("customer/register", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.CustomerRegister("CustomerIdTest"));
			await httpClient.Received(1).PostAsync("customer/register", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task CustomerGetRegisterStatus_Valido() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("customer/getRegisterStatus?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowCustomerGetRegisterStatus {
						CustomerId = "CustomerIdTest",
						Status = "1"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowCustomerGetRegisterStatus retorno = await flowHelper.CustomerGetRegisterStatus("TokenTest");
			Assert.Equal("CustomerIdTest", retorno.CustomerId);
			Assert.Equal("1", retorno.Status);
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("customer/getRegisterStatus?")));
		}

		[Fact]
		public async Task CustomerGetRegisterStatus_StatusCodeError() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("customer/getRegisterStatus?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.CustomerGetRegisterStatus("TokenTest"));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("customer/getRegisterStatus?")));
		}

		[Fact]
		public async Task SubscriptionCreate_Valido() {
			httpClient.PostAsync("subscription/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowSubscriptionCreate {
						SubscriptionId = "SubscriptionIdTest",
						Status = 1
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowSubscriptionCreate retorno = await flowHelper.SubscriptionCreate("PlanIdTest", "CustomerIdTest");
			Assert.Equal("SubscriptionIdTest", retorno.SubscriptionId);
			Assert.Equal((short)1, retorno.Status);
			await httpClient.Received(1).PostAsync("subscription/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task SubscriptionCreate_StatusCodeError() {
			httpClient.PostAsync("subscription/create", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.SubscriptionCreate("PlanIdTest", "CustomerIdTest"));
			await httpClient.Received(1).PostAsync("subscription/create", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task SubscriptionGet_Valido() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("subscription/get?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowSubscriptionGet {
						SubscriptionId = "SubscriptionIdTest"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowSubscriptionGet retorno = await flowHelper.SubscriptionGet("SubscriptionIdTest");
			Assert.Equal("SubscriptionIdTest", retorno.SubscriptionId);
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("subscription/get?")));
		}

		[Fact]
		public async Task SubscriptionGet_StatusCodeError() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("subscription/get?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.SubscriptionGet("SubscriptionIdTest"));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("subscription/get?")));
		}

		[Fact]
		public async Task SubscriptionCancel_Valido() {
			httpClient.PostAsync("subscription/cancel", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowSubscriptionCancel()),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowSubscriptionCancel retorno = await flowHelper.SubscriptionCancel("SubscriptionIdTest");
			await httpClient.Received(1).PostAsync("subscription/cancel", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task SubscriptionCancel_StatusCodeError() {
			httpClient.PostAsync("subscription/cancel", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.SubscriptionCancel("SubscriptionIdTest"));
			await httpClient.Received(1).PostAsync("subscription/cancel", Arg.Any<FormUrlEncodedContent>());
		}

		[Fact]
		public async Task PaymentGetStatus_Valido() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("payment/getStatus?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowPaymentGetStatus {
						CommerceOrder = "CommerceOrderTest",
						Status = 2,
						Amount = "1000",
						Currency = "CLP"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowPaymentGetStatus retorno = await flowHelper.PaymentGetStatus("TokenTest");
			Assert.Equal("CommerceOrderTest", retorno.CommerceOrder);
			Assert.Equal((short)2, retorno.Status);
			Assert.Equal("1000", retorno.Amount);
			Assert.Equal("CLP", retorno.Currency);
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("payment/getStatus?")));
		}

		[Fact]
		public async Task PaymentGetStatus_StatusCodeError() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("payment/getStatus?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.PaymentGetStatus("TokenTest"));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("payment/getStatus?")));
		}

		[Fact]
		public async Task InvoiceGet_Valido() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("invoice/get?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new SalFlowInvoiceGet()),
					Encoding.UTF8,
					"application/json"
				)
			});

			SalFlowInvoiceGet retorno = await flowHelper.InvoiceGet("InvoiceIdTest");
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("invoice/get?")));
		}

		[Fact]
		public async Task InvoiceGet_StatusCodeError() {
			httpClient.GetAsync(Arg.Is<string>(s => s.StartsWith("invoice/get?"))).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => flowHelper.InvoiceGet("InvoiceIdTest"));
			await httpClient.Received(1).GetAsync(Arg.Is<string>(s => s.StartsWith("invoice/get?")));
		}
	}
}
