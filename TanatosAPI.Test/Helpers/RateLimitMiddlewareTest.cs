using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;
using TanatosAPI.Entities.Others;
using TanatosAPI.Entities.Others.RateLimit;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class RateLimitMiddlewareTest {
		private readonly IRateLimiter rateLimiter = Substitute.For<IRateLimiter>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly RequestDelegate requestDelegate = Substitute.For<RequestDelegate>();

		private readonly RateLimitMiddleware rateLimitMiddleware;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public RateLimitMiddlewareTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			variableEntorno.Obtener("RATE_LIMITS_SUBS_TO_SKIP").Returns("sub-skip-1,sub-skip-2");

			rateLimitMiddleware = new(requestDelegate, rateLimiter, variableEntorno, dateTimeProvider);
		}


		private static DefaultHttpContext DefaultHttpContextDummy(string path = "/test", string method = "GET", string? sub = null, string? ip = null) {
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
			DefaultHttpContext context = DefaultHttpContextDummy(path: "/public/Suscripcion/flow-webhook/test", method: "POST");

			await rateLimitMiddleware.InvokeAsync(context);

			await requestDelegate.Received(1)(context);
			await rateLimiter.DidNotReceive().CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Fact]
		public async Task InvokeAsyncTest_SkipSub() {
			DefaultHttpContext context = DefaultHttpContextDummy(sub: "sub-skip-1");

			await rateLimitMiddleware.InvokeAsync(context);

			await requestDelegate.Received(1)(context);
			await rateLimiter.DidNotReceive().CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Theory]
		[InlineData("/public/Auth/test", null, "1.2.3.4", "10", "9")]
		[InlineData("/otra/ruta", null, "1.2.3.4", "20", "19")]
		[InlineData("/otra/ruta", "sub-123", "1.2.3.4", "100", "99")]
		public async Task InvokeAsyncTest_Valido(string path, string? sub, string ip, string expectedMaxRequest, string expectedRemaining) {
			DefaultHttpContext context = DefaultHttpContextDummy(path: path, sub: sub, ip: ip);

			rateLimiter.CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>())
				.Returns(callInfo => new RateLimitResult { Allowed = true, Remaining = callInfo.ArgAt<int>(1) - 1, RetryAfter = DateTime.UtcNow });

			await rateLimitMiddleware.InvokeAsync(context);

			Assert.Equal(expectedMaxRequest, context.Response.Headers["X-RateLimit-Limit"]);
			Assert.Equal(expectedRemaining, context.Response.Headers["X-RateLimit-Remaining"]);
			await requestDelegate.Received(1)(context);
			await rateLimiter.Received(1).CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
		}

		[Theory]
		[InlineData("/public/Auth/test", null, "1.2.3.4", "10", "0")]
		[InlineData("/otra/ruta", null, "1.2.3.4", "20", "0")]
		[InlineData("/otra/ruta", null, null, "20", "0")]
		[InlineData("/otra/ruta", "sub-123", "1.2.3.4", "100", "0")]
		public async Task InvokeAsyncTest_NoValido(string path, string? sub, string? ip, string expectedMaxRequest, string expectedRemaining) {
			DefaultHttpContext context = DefaultHttpContextDummy(path: path, sub: sub, ip: ip);

			rateLimiter.CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>())
				.Returns(callInfo => new RateLimitResult { Allowed = false, Remaining = 0, RetryAfter = FECHA_DUMMY.AddMinutes(1) });

			await rateLimitMiddleware.InvokeAsync(context);

			Assert.Equal(expectedMaxRequest, context.Response.Headers["X-RateLimit-Limit"]);
			Assert.Equal(expectedRemaining, context.Response.Headers["X-RateLimit-Remaining"]);
			Assert.Equal("60", context.Response.Headers.RetryAfter);
			Assert.Equal(429, context.Response.StatusCode);
			Assert.Equal("application/json", context.Response.ContentType);
			await rateLimiter.Received(1).CheckAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<RateLimitContext>());
			await requestDelegate.DidNotReceive()(context);
		}
	}
}
