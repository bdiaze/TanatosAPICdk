using Microsoft.AspNetCore.Authorization;

namespace TanatosAPI.Helpers {
	public static class AuthorizationPolicyBuilderExtensions {
		public static AuthorizationPolicyBuilder RequireScopeOrAnyRole(this AuthorizationPolicyBuilder policy, string scope, params string[] roles) {
			return policy.RequireAssertion(context => 
				context.User.HasClaim("scope", scope) || 
				roles.Any(role => context.User.IsInRole(role))
			);
		}
	}
}
