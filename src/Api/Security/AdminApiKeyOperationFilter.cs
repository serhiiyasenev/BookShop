using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Security
{
    public sealed class AdminApiKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var protectedMethod = method.IsDefined(typeof(RequireAdminApiKeyAttribute), true)
                || (method.DeclaringType?.IsDefined(typeof(RequireAdminApiKeyAttribute), true) ?? false);
            if (!protectedMethod) return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "AdminApiKey"
                    }
                }] = Array.Empty<string>()
            });
        }
    }
}
