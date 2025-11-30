using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TanatosAPI.Helpers {
	public class AuthApplyOperationFilter : IOperationFilter {
		public void Apply(OpenApiOperation operation, OperationFilterContext context) {
			bool allowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

			if (allowAnonymous) {
				return;
			}

			bool requiresAuth = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any();

			if (!requiresAuth) {
				return;
			}

			operation.Security ??= [];
			operation.Security.Add(new OpenApiSecurityRequirement {
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Id = "Bearer",
							Type = ReferenceType.SecurityScheme
						}
					},
					Array.Empty<string>()
				}
			});
		}
	}
}
