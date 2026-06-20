using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Helpers {
	public class RateLimitMiddlewareTest {
		private readonly IRateLimiter rateLimiter = Substitute.For<IRateLimiter>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private bool _nextCalled;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		private RateLimitMiddleware CrearMiddleware() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			variableEntorno.Obtener("RATE_LIMITS_SUBS_TO_SKIP").Returns("sub-skip-1,sub-skip-2");
			RequestDelegate next = _ => { _nextCalled = true; return Task.CompletedTask; };

			return new RateLimitMiddleware(next, rateLimiter, variableEntorno, dateTimeProvider);
		}

		private static DefaultHttpContext CrearContexto(string path = "/test", string method = "GET", string? sub = null, string? ip = null) {
			var context = new DefaultHttpContext();
			context.Request.Path = path;
			context.Request.Method = method;

			if (ip != null) {
				context.Request.Headers["X-Forwarded-For"] = ip;
			}

			if (sub != null) {
				var claims = new[] { new Claim(ClaimTypes.NameIdentifier, sub) };
				context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
			}

			context.Response.Body = new MemoryStream();

			return context;
		}

		[Fact]
		public async Task InvokeAsyncTest_SkipFlowWebhook() {
			RateLimitMiddleware middleware = CrearMiddleware();
			DefaultHttpContext context = CrearContexto(path: "/public/Suscripcion/flow-webhook/test", method: "POST");

			await middleware.InvokeAsync(context);

			Assert.True(_nextCalled);
			await rateLimiter.DidNotReceive().CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Fact]
		public async Task InvokeAsyncTest_SkipSub() {
			RateLimitMiddleware middleware = CrearMiddleware();
			DefaultHttpContext context = CrearContexto(sub: "sub-skip-1");

			await middleware.InvokeAsync(context);

			Assert.True(_nextCalled);
			await rateLimiter.DidNotReceive().CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Theory]
		[InlineData("/public/Auth/test", null, "1.2.3.4", "10", "9")]
		[InlineData("/otra/ruta", null, "1.2.3.4", "20", "19")]
		[InlineData("/otra/ruta", "sub-123", "1.2.3.4", "100", "99")]
		public async Task InvokeAsyncTest_Valido(string path, string? sub, string ip, string expectedMaxRequest, string expectedRemaining) {
			RateLimitMiddleware middleware = CrearMiddleware();
			DefaultHttpContext context = CrearContexto(path: path, sub: sub, ip: ip);

			rateLimiter.CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>())
				.Returns(callInfo => new RateLimitResult { Allowed = true, Remaining = callInfo.ArgAt<int>(1) - 1, RetryAfter = DateTime.UtcNow });

			await middleware.InvokeAsync(context);

			Assert.Equal(expectedMaxRequest, context.Response.Headers["X-RateLimit-Limit"]);
			Assert.Equal(expectedRemaining, context.Response.Headers["X-RateLimit-Remaining"]);
			await rateLimiter.Received(1).CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Theory]
		[InlineData("/public/Auth/test", null, "1.2.3.4", "10", "0")]
		[InlineData("/otra/ruta", null, "1.2.3.4", "20", "0")]
		[InlineData("/otra/ruta", null, null, "20", "0")]
		[InlineData("/otra/ruta", "sub-123", "1.2.3.4", "100", "0")]
		public async Task InvokeAsyncTest_NoValido(string path, string? sub, string? ip, string expectedMaxRequest, string expectedRemaining) {
			RateLimitMiddleware middleware = CrearMiddleware();
			DefaultHttpContext context = CrearContexto(path: path, sub: sub, ip: ip);

			rateLimiter.CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>())
				.Returns(callInfo => new RateLimitResult { Allowed = false, Remaining = 0, RetryAfter = FECHA_DUMMY.AddMinutes(1) });

			await middleware.InvokeAsync(context);

			Assert.Equal(expectedMaxRequest, context.Response.Headers["X-RateLimit-Limit"]);
			Assert.Equal(expectedRemaining, context.Response.Headers["X-RateLimit-Remaining"]);
			Assert.Equal("60", context.Response.Headers.RetryAfter);
			Assert.Equal(429, context.Response.StatusCode);
			Assert.Equal("application/json", context.Response.ContentType);
			await rateLimiter.Received(1).CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}
	}
}
