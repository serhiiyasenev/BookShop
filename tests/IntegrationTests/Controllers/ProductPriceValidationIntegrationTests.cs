using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Helpers;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer;
using DataAccessLayer.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using static IntegrationTests.Helpers.WebPageHelpers;

namespace IntegrationTests.Controllers
{
    public class ProductPriceValidationIntegrationTests
    {
        private const string PriceError = "Price must have at most two decimal places.";

        [TestCase("0.001")]
        [TestCase("19.499")]
        public async Task ApiCreate_RejectsFractionalCents_WithoutSaving(string text)
        {
            using var factory = CreateFactory<Api.Startup>();
            using var client = factory.CreateClient();
            using var content = JsonHelper.ToStringContent(Product(decimal.Parse(text, CultureInfo.InvariantCulture)));
            using var response = await client.PostAsync("/Product", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var problem = await response.GetModelAsync<ValidationProblemDetails>();
            Assert.That(problem.Errors["Price"], Does.Contain(PriceError));
            using var scope = factory.Services.CreateScope();
            Assert.That(await scope.ServiceProvider.GetRequiredService<EfCoreContext>().Products.CountAsync(), Is.Zero);
        }

        [TestCase("0.001")]
        [TestCase("19.499")]
        public async Task ApiUpdate_RejectsFractionalCents_AndKeepsStoredPrice(string text)
        {
            using var factory = CreateFactory<Api.Startup>();
            using var client = factory.CreateClient();
            using var createContent = JsonHelper.ToStringContent(Product(12.34m));
            using var createdResponse = await client.PostAsync("/Product", createContent);
            var created = await createdResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();

            using var updateContent = JsonHelper.ToStringContent(Product(decimal.Parse(text, CultureInfo.InvariantCulture)));
            using var response = await client.PutAsync($"/Product/{created.Id}", updateContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var problem = await response.GetModelAsync<ValidationProblemDetails>();
            Assert.That(problem.Errors["Price"], Does.Contain(PriceError));

            using var readResponse = await client.GetAsync($"/Product/{created.Id}");
            var persisted = await readResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();
            Assert.That(persisted.Price, Is.EqualTo(12.34m));
        }

        [TestCase("0.0100")]
        [TestCase("19.4900")]
        [TestCase("9999999999999999.99")]
        public async Task ApiCreateAndUpdate_KeepAcceptedPricesConsistentWithReads(string text)
        {
            var price = decimal.Parse(text, CultureInfo.InvariantCulture);
            using var factory = CreateFactory<Api.Startup>();
            using var client = factory.CreateClient();
            using var createContent = JsonHelper.ToStringContent(Product(price));
            using var createdResponse = await client.PostAsync("/Product", createContent);
            var created = await createdResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();
            using var readResponse = await client.GetAsync($"/Product/{created.Id}");
            var persisted = await readResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();
            Assert.That(created.Price, Is.EqualTo(price));
            Assert.That(persisted.Price, Is.EqualTo(created.Price));

            // Seed a different value so this also proves that PUT persists the new price.
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EfCoreContext>();
                (await db.Products.SingleAsync(p => p.Id == created.Id)).Price = 12.34m;
                await db.SaveChangesAsync();
            }
            using var updateContent = JsonHelper.ToStringContent(Product(price));
            using var updatedResponse = await client.PutAsync($"/Product/{created.Id}", updateContent);
            var updated = await updatedResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();
            using var updatedReadResponse = await client.GetAsync($"/Product/{created.Id}");
            var updatedRead = await updatedReadResponse.EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();
            Assert.That(updated.Price, Is.EqualTo(price));
            Assert.That(updatedRead.Price, Is.EqualTo(updated.Price));
        }

        [TestCase(false, "0.001")]
        [TestCase(false, "19.499")]
        [TestCase(true, "0.001")]
        [TestCase(true, "19.499")]
        public async Task MvcCreateAndEdit_RejectFractionalCents_WithoutChangingStorage(bool edit, string text)
        {
            using var factory = CreateFactory<WebUI.Startup>();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var id = Guid.NewGuid();
            if (edit) await SeedProduct(factory, id);
            var path = edit ? $"/Products/Edit?id={id}" : "/Products/Create";
            using var form = await PriceForm(client, path, id, text);
            using var response = await client.PostAsync(path, form);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain(PriceError));
            using var scope = factory.Services.CreateScope();
            var products = scope.ServiceProvider.GetRequiredService<EfCoreContext>().Products.AsNoTracking();
            if (edit)
                Assert.That((await products.SingleAsync()).Price, Is.EqualTo(12.34m));
            else
                Assert.That(await products.CountAsync(), Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task MvcCreateAndEdit_AcceptHarmlessTrailingZeros(bool edit)
        {
            using var factory = CreateFactory<WebUI.Startup>();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var id = Guid.NewGuid();
            if (edit) await SeedProduct(factory, id);
            var path = edit ? $"/Products/Edit?id={id}" : "/Products/Create";
            using var form = await PriceForm(client, path, id, "19.4900");
            using var response = await client.PostAsync(path, form);

            Assert.That(response.StatusCode, Is.EqualTo(edit ? HttpStatusCode.OK : HttpStatusCode.Redirect));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain(PriceError));
            using var scope = factory.Services.CreateScope();
            var product = await scope.ServiceProvider.GetRequiredService<EfCoreContext>().Products.AsNoTracking().SingleAsync();
            Assert.That(product.Price, Is.EqualTo(19.49m));
        }

        private static WebApplicationFactory<TStartup> CreateFactory<TStartup>() where TStartup : class
        {
            var databaseName = "PriceValidation_" + Guid.NewGuid().ToString("N");
            return new WebApplicationFactory<TStartup>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(EfCoreContext));
                    services.RemoveAll(typeof(DbContextOptions<EfCoreContext>));
                    services.AddDbContext<EfCoreContext>(options => options.UseInMemoryDatabase(databaseName));
                }));
        }

        private static ProductInbound Product(decimal price) => new ProductInbound
        {
            Name = "Price validation book",
            Description = "A book for price validation tests",
            Author = "Test Author",
            Price = price,
            ImageUrl = "https://example.com/book.jpg"
        };

        private static async Task SeedProduct(WebApplicationFactory<WebUI.Startup> factory, Guid id)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfCoreContext>();
            db.Products.Add(new ProductDto { Id = id, Name = "Price validation book", Price = 12.34m });
            await db.SaveChangesAsync();
        }

        private static async Task<FormUrlEncodedContent> PriceForm(HttpClient client, string path, Guid id, string price)
        {
            using var page = await client.GetAsync(path);
            var token = GetRequestVerificationToken(await page.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [TokenTag] = token,
                ["Id"] = id.ToString(),
                ["Name"] = "Price validation book",
                ["Description"] = "A book for price validation tests",
                ["Author"] = "Test Author",
                ["Price"] = price,
                ["ImageUrl"] = "https://example.com/book.jpg"
            });
        }
    }
}
