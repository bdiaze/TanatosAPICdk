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
									new Claim("sub", "local-dev-user"),
									new Claim("username", "local-dev-user"),
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
