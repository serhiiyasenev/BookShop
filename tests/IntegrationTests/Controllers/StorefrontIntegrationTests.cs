using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccessLayer.DTO;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static IntegrationTests.Helpers.WebPageHelpers;

namespace IntegrationTests.Controllers
{
    public class StorefrontIntegrationTests
    {
        private BookShopTestFactory<WebUI.Startup> _factory;
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _factory = new BookShopTestFactory<WebUI.Startup>();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [TestCase("/", "Welcome to BookShop")]
        [TestCase("/Home/Privacy", "Privacy Policy")]
        [TestCase("/Home/Error", "Request ID:")]
        public async Task HomePages_RenderExpectedContent(string path, string content)
        {
            using var response = await _client.GetAsync(path);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain(content));
            if (path == "/Home/Error") Assert.That(response.Headers.CacheControl.NoStore, Is.True);
        }

        [Test]
        public async Task Details_DisplaysTheStoredBookAndEditLink()
        {
            var product = Product();
            await _factory.SeedAsync(product);

            using var response = await _client.GetAsync($"/Products/Details?id={product.Id}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var html = await response.Content.ReadAsStringAsync();
            Assert.That(html, Does.Contain(product.Name).And.Contain(product.Author).And.Contain(product.Description));
            Assert.That(html, Does.Contain($"/Products/Edit?id={product.Id}"));
        }

        [TestCase("Details")]
        [TestCase("Edit")]
        [TestCase("Delete")]
        public async Task MissingProduct_ReturnsNotFound(string action)
        {
            using var response = await _client.GetAsync($"/Products/{action}?id={Guid.NewGuid()}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Delete_WithAntiforgeryToken_RemovesOnlySelectedBook()
        {
            var product = Product();
            var retained = Product();
            await _factory.SeedAsync(product, retained);
            var path = $"/Products/Delete?id={product.Id}";
            var html = await _client.GetStringAsync(path);
            Assert.That(html, Does.Contain(product.Name));
            using var form = TokenForm(html, product.Id);

            using var response = await _client.PostAsync(path, form);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(response.Headers.Location.ToString(), Is.EqualTo("/Products"));
            var remaining = await _factory.QueryAsync(db => db.Products.AsNoTracking().SingleAsync());
            Assert.That(remaining.Id, Is.EqualTo(retained.Id));
            using var deleted = await _client.GetAsync($"/Products/Details?id={product.Id}");
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Delete_WithoutAntiforgeryToken_RejectsRequestAndKeepsBook()
        {
            var product = Product();
            await _factory.SeedAsync(product);
            using var response = await _client.PostAsync($"/Products/Delete?id={product.Id}", new FormUrlEncodedContent(new Dictionary<string, string>()));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await _factory.QueryAsync(db => db.Products.CountAsync()), Is.EqualTo(1));
        }

        [Test]
        public async Task Delete_WhenAlreadyRemoved_IsANoOp()
        {
            var product = Product();
            await _factory.SeedAsync(product);
            var path = $"/Products/Delete?id={product.Id}";
            var html = await _client.GetStringAsync(path);
            using (var firstForm = TokenForm(html, product.Id))
            using (var first = await _client.PostAsync(path, firstForm))
                Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

            using var repeatedForm = TokenForm(html, product.Id);
            using var repeated = await _client.PostAsync(path, repeatedForm);

            Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(await _factory.QueryAsync(db => db.Products.CountAsync()), Is.Zero);
        }

        [Test]
        public async Task Edit_WhenProductWasRemoved_ReturnsNotFoundWithoutCreatingIt()
        {
            var html = await _client.GetStringAsync("/Products/Create");
            var id = Guid.NewGuid();
            using var form = TokenForm(html, id);
            using var response = await _client.PostAsync($"/Products/Edit?id={id}", form);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(await _factory.QueryAsync(db => db.Products.CountAsync()), Is.Zero);
        }

        [Test]
        public async Task Index_FiltersCatalogByName()
        {
            var matching = Product();
            matching.Name = "Holiday reading";
            var other = Product();
            other.Name = "Office handbook";
            await _factory.SeedAsync(matching, other);

            var html = await _client.GetStringAsync("/Products?Name=Holiday");

            Assert.That(html, Does.Contain(matching.Name).And.Not.Contain(other.Name));
        }

        private static FormUrlEncodedContent TokenForm(string html, Guid id)
        {
            var token = GetRequestVerificationToken(html);
            Assert.That(token, Is.Not.Empty, "The actual page must supply an antiforgery token.");
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [TokenTag] = token, ["Id"] = id.ToString(), ["Name"] = "Storefront book", ["Price"] = "19.49"
            });
        }

        private static ProductDto Product() => new()
        {
            Id = Guid.NewGuid(), Name = "Storefront book", Description = "A reader-friendly description",
            Author = "Test Author", Price = 19.49m, ImageUrl = "https://example.com/cover.jpg"
        };
    }
}
