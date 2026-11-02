using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Security;
using BusinessLayer.Models.Inbound;
using IntegrationTests.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using NUnit.Framework;

namespace IntegrationTests.Controllers
{
    public class AdminApiSecurityIntegrationTests
    {
        private static ProductInbound ValidProduct() => new ProductInbound
        {
            Name = "Security test product",
            Description = "A test book",
            Author = "Test Author",
            Price = 19.49m,
            ImageUrl = "https://example.com/book.png"
        };

        [Test]
        public async Task MissingServerConfiguration_FailsClosed()
        {
            using var factory = new BookShopTestFactory<Api.Startup>(configureAdminKey: false);
            using var client = factory.CreateAuthorizedClient();
            using var response = await client.PostAsJsonAsync("/Product", ValidProduct());
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        }

        [Test]
        public async Task MissingHeader_RejectsProductWrite_ButAllowsCatalogRead()
        {
            using var factory = new BookShopTestFactory<Api.Startup>();
            using var client = factory.CreateClient();
            using var denied = await client.PostAsJsonAsync("/Product", ValidProduct());
            Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            using var allowed = await client.GetAsync("/Product");
            Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task WrongHeader_RejectsBookingReads()
        {
            using var factory = new BookShopTestFactory<Api.Startup>();
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(AdminApiKeyAuthorizationFilter.HeaderName, new string('X', 40));
            using var response = await client.GetAsync("/Booking");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task SwaggerSecurity_AppliesOnlyToProtectedEndpoints()
        {
            using var factory = new BookShopTestFactory<Api.Startup>();
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/swagger/v1/swagger.json");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            var scheme = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("AdminApiKey");
            Assert.That(scheme.GetProperty("name").GetString(), Is.EqualTo(AdminApiKeyAuthorizationFilter.HeaderName));
            var paths = root.GetProperty("paths");
            Assert.That(paths.GetProperty("/Product").GetProperty("post").TryGetProperty("security", out _), Is.True);
            Assert.That(paths.GetProperty("/Product").GetProperty("get").TryGetProperty("security", out _), Is.False);
            Assert.That(paths.GetProperty("/Booking").GetProperty("get").TryGetProperty("security", out _), Is.True);
        }
    }


    public class AdminApiOperationFilterUnitTests
    {
        private sealed class PlainController
        {
            public void PublicAction() { }

            [RequireAdminApiKey]
            public void RestrictedAction() { }
        }

        [RequireAdminApiKey]
        private sealed class RestrictedController
        {
            public void RestrictedAction() { }
        }

        private static OperationFilterContext ContextFor(Type controller, string action)
        {
            var method = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance);
            return new OperationFilterContext(null, null, null, method);
        }

        [Test]
        public void Apply_DoesNotRequireKeyForUnprotectedAction()
        {
            var operation = new OpenApiOperation();
            new AdminApiKeyOperationFilter().Apply(operation, ContextFor(typeof(PlainController), nameof(PlainController.PublicAction)));
            Assert.That(operation.Security, Is.Null.Or.Empty);
        }

        [Test]
        public void Apply_InitializesMissingSecurityForMethodAttribute()
        {
            var operation = new OpenApiOperation { Security = null };
            new AdminApiKeyOperationFilter().Apply(operation, ContextFor(typeof(PlainController), nameof(PlainController.RestrictedAction)));
            Assert.That(operation.Security, Has.Count.EqualTo(1));
        }

        [Test]
        public void Apply_PreservesExistingSecurityForControllerAttribute()
        {
            var original = new OpenApiSecurityRequirement();
            var operation = new OpenApiOperation
            {
                Security = new List<OpenApiSecurityRequirement> { original }
            };
            new AdminApiKeyOperationFilter().Apply(operation, ContextFor(typeof(RestrictedController), nameof(RestrictedController.RestrictedAction)));
            Assert.That(operation.Security, Has.Count.EqualTo(2));
            Assert.That(operation.Security[0], Is.SameAs(original));
        }
    }
}
