using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Security;
using BusinessLayer.Models.Inbound;
using IntegrationTests.Helpers;
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
}
