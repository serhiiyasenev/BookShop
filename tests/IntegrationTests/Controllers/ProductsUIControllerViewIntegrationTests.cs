using DataAccessLayer;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static IntegrationTests.Helpers.WebPageHelpers;

namespace IntegrationTests.Controllers
{
    [TestFixture]
    public class ProductsUIControllerViewIntegrationTests
    {
        private readonly string _requestUri;
        private readonly HttpClient _httpClient;
        private readonly WebApplicationFactory<WebUI.Startup> _factory;

        public ProductsUIControllerViewIntegrationTests()
        {
            _requestUri = "/Products";
            _factory = new WebApplicationFactory<WebUI.Startup>()
                .WithWebHostBuilder(builder => 
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(EfCoreContext));
                        services.RemoveAll(typeof(DbContextOptions<EfCoreContext>));
                        services.AddDbContext<EfCoreContext>(options => options.UseInMemoryDatabase("TestDb"));
                    }
            ));
            _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true } );
        }

        [Test]
        public async Task CreateProduct_Returns_CreatedMessage()
        {
            // Load Products Page
            var totalCountRegex = "<label>Total Count: (\\d)</label>";
            var responseProductsPage = await _httpClient.GetAsync($"{_requestUri}");
            responseProductsPage.EnsureSuccessStatusCode();
            var productsCounterInit = int.Parse(RegexSearch(totalCountRegex, 
                await responseProductsPage.Content.ReadAsStringAsync()));

            // Load Create Page
            var response = await _httpClient.GetAsync($"{_requestUri}/Create");
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            var requestVerificationToken = GetRequestVerificationToken(stringResponse);

            // Arrange
            var productName = $"Book {DateTime.UtcNow.Ticks}";
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Name", productName),
                new KeyValuePair<string, string>("Description", "Description Test"),
                new KeyValuePair<string, string>("Author", "Author 1"),
                new KeyValuePair<string, string>("Price", "19.49"),
                new KeyValuePair<string, string>("ImageUrl", string.Empty),
                new KeyValuePair<string, string>(TokenTag, requestVerificationToken)
            };

            var formContent = new FormUrlEncodedContent(keyValues);

            // Act
            var postResponse = await _httpClient.PostAsync($"{_requestUri}/Create", formContent);
            postResponse.EnsureSuccessStatusCode();
            var stringPostResponse = await postResponse.Content.ReadAsStringAsync();
            var productsCounterUpdated = int.Parse(RegexSearch(totalCountRegex, stringPostResponse));

            // Assert
            stringPostResponse.Should().Contain(productName);
            stringPostResponse.Should().Contain("New product created!");
            productsCounterUpdated.Should().Be(productsCounterInit + 1);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _httpClient.Dispose();
            await _factory.DisposeAsync();
        }
    }
}
