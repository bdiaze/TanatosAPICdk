using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace TanatosAPI.Helpers {
	public class AllowAnonymousAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {
		protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
			return Task.FromResult(AuthenticateResult.Success(
				new AuthenticationTicket(
					new ClaimsPrincipal(
						new ClaimsPrincipal(
							new ClaimsIdentity(
								[ 
									new Claim(ClaimTypes.NameIdentifier, "DevUser-1a07-4018-b2c3-3eea6b80b831"),
									new Claim(ClaimTypes.Role, "Admin"),
								], 
								"AllowAnonymous"
							)
						)
					), 
					"AllowAnonymous"
				)
			));
		}
	}
}
