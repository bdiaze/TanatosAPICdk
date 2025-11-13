using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TanatosAPI.Entities.Contexts;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Queries;

namespace TanatosAPI.Endpoints {
    public static class TipoReceptorNotificacionEndpoints {
        public static IEndpointRouteBuilder MapTipoReceptorNotificacionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/TipoReceptorNotificacion");
            group.MapCrearEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/", async (TipoReceptorNotificacion entrada, IHostEnvironment environment, TanatosDbContext dbContext) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoReceptorNotificacion? existente = await TipoReceptorNotificacionQueries.GetByIdAsync(dbContext, entrada.Id);

                    if (existente == null) {
                        await dbContext.AddAsync(entrada);
                        await dbContext.SaveChangesAsync();
                        existente = entrada;
                    }

                    LambdaLogger.Log(
                        $"[POST] - [TipoReceptorNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del tipo de receptor de notificación - ID: {entrada.Id}.");

                    return Results.Ok(existente);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [TipoReceptorNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del tipo de receptor de notificación - ID: {entrada.Id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
