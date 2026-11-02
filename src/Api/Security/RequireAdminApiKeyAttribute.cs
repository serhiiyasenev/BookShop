using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Api.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireAdminApiKeyAttribute : TypeFilterAttribute
    {
        public RequireAdminApiKeyAttribute() : base(typeof(AdminApiKeyAuthorizationFilter)) { }
    }

    public sealed class AdminApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        public const string HeaderName = "X-Admin-Api-Key";
        private readonly IConfiguration _configuration;

        public AdminApiKeyAuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var expected = _configuration["Security:AdminApiKey"];
            if (string.IsNullOrWhiteSpace(expected) || Encoding.UTF8.GetByteCount(expected) < 32)
            {
                context.Result = new StatusCodeResult(503);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
                || provided.Count != 1 || string.IsNullOrEmpty(provided[0]))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
            var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided[0]));
            if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
