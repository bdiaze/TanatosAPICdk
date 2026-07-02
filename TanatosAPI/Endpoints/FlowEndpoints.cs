using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class FlowEndpoints {
		public static IEndpointRouteBuilder MapFlowEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder customerGroup = routes.MapGroup("/Flow/Customer");
			customerGroup.MapCustomerGetRegisterStatus();

			RouteGroupBuilder paymentGroup = routes.MapGroup("/Flow/Payment");
			paymentGroup.MapPaymentGetStatus();

			RouteGroupBuilder invoiceGroup = routes.MapGroup("/Flow/Invoice");
			invoiceGroup.MapInvoiceGet();

			RouteGroupBuilder subscriptionGroup = routes.MapGroup("/Flow/Subscription");
			subscriptionGroup.MapSubscriptionGet();

			return routes;
		}

		private static void MapCustomerGetRegisterStatus(this IEndpointRouteBuilder routes) {
			routes.MapGet("/GetRegisterStatus/{token}", async (string token, IHostEnvironment environment, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalFlowCustomerGetRegisterStatus retorno = await flowHelper.CustomerGetRegisterStatus(token);
					
					LambdaLogger.Log(
						$"[GET] - [Flow] - [CustomerGetRegisterStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del customer register status.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [CustomerGetRegisterStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [CustomerGetRegisterStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener el customer register status. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapPaymentGetStatus(this IEndpointRouteBuilder routes) {
			routes.MapGet("/GetStatus/{token}", async (string token, IHostEnvironment environment, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalFlowPaymentGetStatus retorno = await flowHelper.PaymentGetStatus(token);

					LambdaLogger.Log(
						$"[GET] - [Flow] - [PaymentGetStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del payment status.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [PaymentGetStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [PaymentGetStatus] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener el payment status. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapInvoiceGet(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Get/{invoiceId}", async (string invoiceId, IHostEnvironment environment, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalFlowInvoiceGet retorno = await flowHelper.InvoiceGet(invoiceId);

					LambdaLogger.Log(
						$"[GET] - [Flow] - [InvoiceGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del invoice.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [InvoiceGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [InvoiceGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener el invoice. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapSubscriptionGet(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Get/{subscriptionId}", async (string subscriptionId, IHostEnvironment environment, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalFlowSubscriptionGet retorno = await flowHelper.SubscriptionGet(subscriptionId);

					LambdaLogger.Log(
						$"[GET] - [Flow] - [SubscriptionGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la subscription.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [SubscriptionGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Flow] - [SubscriptionGet] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la subscription. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}
	}
}
