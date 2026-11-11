using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PimBurguer.Api.Services
{
    // O Swagger mostra o cadeado somente nos endpoints que exigem uma conta.
    public class SwaggerAcessoFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var atributos = context.MethodInfo.GetCustomAttributes(true).Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? new object[] { });
            if (!atributos.OfType<AuthorizeAttribute>().Any())
            {
                return;
            }

            operation.Security = [new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }}] = []
            }

            ];
        }
    }
}
