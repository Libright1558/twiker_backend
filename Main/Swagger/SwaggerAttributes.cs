using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using twiker_backend.CustomAttributes.Authentication;

namespace twiker_backend.Swagger.SwaggerAttributes
{
    public class SwaggerAuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo.DeclaringType!.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>()
                .Any();

            if (hasAuthorize)
            {
                var JwtAuthorizeAttribute = context.MethodInfo.GetCustomAttributes(true)
                    .Union(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                    .OfType<JwtAuthorizeAttribute>()
                    .Any();

                var AccessAuthorizeAttribute = context.MethodInfo.GetCustomAttributes(true)
                    .Union(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                    .OfType<AccessAuthorizeAttribute>()
                    .Any();

                var securityScheme = JwtAuthorizeAttribute ? "JwtScheme" : 
                                    AccessAuthorizeAttribute ? "AccessScheme" : 
                                    "JwtScheme"; // Default to JwtScheme if not specified

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = securityScheme
                                }
                            },
                            new List<string>()
                        }
                    }
                };
            }
        }
    }
}
